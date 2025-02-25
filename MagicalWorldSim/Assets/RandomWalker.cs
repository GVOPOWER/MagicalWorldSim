using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomWalker : MonoBehaviour
{
    public CharacterAttributes attributes = new CharacterAttributes();
    public float moveSpeed = 2f;
    public float slowMoveSpeedFactor = 0.5f;
    public float changeDirectionInterval = 2f;
    public float directionChangeSmoothness = 5f;
    public float visionRange = 5f;
    public LayerMask groundLayer;
    public string bushTag = "Bush";
    public string playerTag = "Player";
    public float attackCooldown = 1f;
    public float attackRange = 0.5f;
    public int attackDamage = 10;
    public float eatCooldown = 2f;
    public float edgeAvoidanceRange = 0.5f;
    public float separationDuration = 1f;

    private Vector2 movementDirection;
    private Vector2 targetDirection;
    private bool shouldChaseBush = false;
    private bool shouldChasePlayer = false;
    private Transform targetBush;
    private Transform targetPlayer;
    private float lastAttackTime = -Mathf.Infinity;
    private float lastEatTime = -Mathf.Infinity;
    private float separationEndTime = -Mathf.Infinity;
    private bool isPaused = false;
    private float pauseTime = 0f;
    private Animator animator;
    private Rigidbody2D rb;
    private List<Tilemap> tilemaps = new List<Tilemap>();
    public TileBase[] walkableTiles;
    public TileBase[] slowWalkableTiles;
    public TileBase[] unwalkableTiles;
    public CityCreation cityCreation;
    public string slimeTag = "Slime";

    private void Start()
    {
        attributes.InitializeAttributes();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        tilemaps.AddRange(FindObjectsOfType<Tilemap>());

        if (rb == null || animator == null || tilemaps.Count == 0)
        {
            Debug.LogError("Essential components are missing.");
            enabled = false;
            return;
        }

        InvokeRepeating("SetNewTargetDirection", 0f, changeDirectionInterval);
        targetDirection = GetRandomDirection();
        movementDirection = targetDirection;
    }

    private void Update()
    {
        if (isPaused)
        {
            SetIdleAnimation();
            if (Time.time >= pauseTime)
                isPaused = false;
            return;
        }
        else if (Random.value < 0.0005f)
        {
            isPaused = true;
            pauseTime = Time.time + Random.Range(0.5f, 3f);
            return;
        }

        if (CanCreateCity())
            CreateCity();

        if (Time.time < separationEndTime)
            return;

        HandleCharacterLifecycle();
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        Tilemap currentTilemap = GetCurrentTilemap();
        if (currentTilemap == null)
        {
            Debug.LogWarning("No current tilemap found.");
            return;
        }

        Vector2 currentPosition = transform.position;
        Vector2 nextPosition = currentPosition + (movementDirection * moveSpeed * Time.deltaTime);
        Vector3Int nextCellPosition = currentTilemap.WorldToCell(nextPosition);
        TileBase nextTile = currentTilemap.GetTile(nextCellPosition);

        if (nextTile == null || !IsTileWalkable(nextTile))
        {
            if (IsNearUnwalkableTile(currentPosition, currentTilemap))
            {
                AdjustDirectionAvoidingUnwalkable();
            }
        }
        else
        {
            float currentSpeed = AdjustSpeedBasedOnTile(nextTile);
            transform.Translate(movementDirection * currentSpeed * Time.deltaTime);
            UpdateAnimationDirection();
        }
    }

    private bool IsTileWalkable(TileBase tile)
    {
        return System.Array.Exists(walkableTiles, walkableTile => walkableTile == tile);
    }

    private bool IsNearUnwalkableTile(Vector2 position, Tilemap tilemap)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(position);
        foreach (Vector2 direction in GetSurroundingDirections())
        {
            Vector3Int adjacentPosition = cellPosition + new Vector3Int((int)direction.x, (int)direction.y, 0);
            TileBase adjacentTile = tilemap.GetTile(adjacentPosition);
            if (adjacentTile != null && System.Array.Exists(unwalkableTiles, tile => tile == adjacentTile))
            {
                return true;
            }
        }
        return false;
    }

    private void AdjustDirectionAvoidingUnwalkable()
    {
        Vector2 avoidanceDirection = Vector2.zero;
        Tilemap currentTilemap = GetCurrentTilemap();
        Vector3Int currentPosition = currentTilemap.WorldToCell(transform.position);

        foreach (Vector2 direction in GetSurroundingDirections())
        {
            Vector3Int checkPosition = currentPosition + new Vector3Int((int)direction.x, (int)direction.y, 0);
            TileBase tile = currentTilemap.GetTile(checkPosition);
            if (tile != null && System.Array.Exists(unwalkableTiles, tile => tile == tile))
            {
                avoidanceDirection -= direction;
            }
        }

        if (avoidanceDirection != Vector2.zero)
        {
            targetDirection = (movementDirection + avoidanceDirection).normalized;
            movementDirection = targetDirection;
        }
    }

    private IEnumerable<Vector2> GetSurroundingDirections()
    {
        return new List<Vector2>
        {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            Vector2.up + Vector2.left, Vector2.up + Vector2.right,
            Vector2.down + Vector2.left, Vector2.down + Vector2.right
        };
    }

    private void HandleCharacterLifecycle()
    {
        attributes.HandleHunger(Time.deltaTime);
        attributes.IncrementAge(Time.deltaTime);
        DieOfAge();

        if (attributes.currentHp <= 0)
        {
            Debug.Log($"{attributes.characterName} has died due to low health.");
            Destroy(gameObject);
            return;
        }

        UpdateTarget();

        if (IsGrounded())
        {
            if (shouldChaseBush && targetBush != null)
                ChaseTarget(targetBush);
            else if (shouldChasePlayer && targetPlayer != null)
                ChaseTarget(targetPlayer);
            else
            {
                SmoothlyChangeDirection();
                AvoidEdgesAndMove();
            }
        }
        else
        {
            MoveToNearestGround();
        }
    }

    private Tilemap GetCurrentTilemap()
    {
        foreach (Tilemap tilemap in tilemaps)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
            if (tilemap.GetTile(cellPosition) != null)
                return tilemap;
        }
        return null;
    }

    private bool CanCreateCity()
    {
        return attributes.currentAge >= 16f && attributes.currentHunger > 50f && string.IsNullOrEmpty(attributes.currentCity);
    }

    private void CreateCity()
    {
        if (cityCreation != null)
        {
            cityCreation.CreateCity(transform.position);
            attributes.currentCity = cityCreation.GenerateRandomCityName();
        }
        else
        {
            Debug.LogError("CityCreation reference is not set on RandomWalker");
        }
    }

    private void SetIdleAnimation()
    {
        animator.SetBool("isIdle", true);
        animator.SetBool("isWalkingLeft", false);
        animator.SetBool("isWalkingRight", false);
        animator.SetBool("isWalkingUp", false);
        animator.SetBool("isWalkingDown", false);
    }

    private Vector2 GetRandomDirection()
    {
        float randomX = UnityEngine.Random.Range(-1f, 1f);
        float randomY = UnityEngine.Random.Range(-1f, 1f);
        return new Vector2(randomX, randomY).normalized;
    }

    private void SetNewTargetDirection()
    {
        targetDirection = GetRandomDirection();
    }

    private void UpdateAnimationDirection()
    {
        if (movementDirection.magnitude > 0.01f)
        {
            animator.SetBool("isIdle", false);
            if (Mathf.Abs(movementDirection.x) > Mathf.Abs(movementDirection.y))
            {
                animator.SetBool("isWalkingLeft", movementDirection.x < 0);
                animator.SetBool("isWalkingRight", movementDirection.x >= 0);
            }
            else
            {
                animator.SetBool("isWalkingUp", movementDirection.y > 0);
                animator.SetBool("isWalkingDown", movementDirection.y <= 0);
            }
        }
        else
        {
            SetIdleAnimation();
        }
    }

    private float AdjustSpeedBasedOnTile(TileBase currentTile)
    {
        if (System.Array.Exists(slowWalkableTiles, tile => tile == currentTile))
            return moveSpeed * slowMoveSpeedFactor;
        else if (System.Array.Exists(unwalkableTiles, tile => tile == currentTile))
            return 0;
        else
            return moveSpeed;
    }

    private void DieOfAge()
    {
        if (attributes.currentAge >= attributes.maxAge)
        {
            Debug.Log($"{attributes.characterName} has died of old age at {attributes.currentAge}");
            Destroy(gameObject);
        }
    }

    private void UpdateTarget()
    {
        if (attributes.currentHunger <= 75f)
        {
            FindNearestBush();
            shouldChaseBush = targetBush != null && Vector2.Distance(transform.position, targetBush.position) <= visionRange;
        }
        else
        {
            shouldChaseBush = false;
            targetBush = null;

            if (attributes.CanCreateChild(Time.time))
            {
                FindNearestPlayer();
                shouldChasePlayer = targetPlayer != null;

                if (shouldChasePlayer)
                {
                    RandomWalker targetWalker = targetPlayer.GetComponent<RandomWalker>();
                    if (targetWalker != null && targetWalker.attributes.CanCreateChild(Time.time))
                    {
                        CreateChild(this, targetWalker);
                    }
                }
            }
            else
            {
                shouldChasePlayer = false;
                targetPlayer = null;
            }
        }

        FindNearestSlime();
    }

    private void FindNearestSlime()
    {
        GameObject[] slimes = GameObject.FindGameObjectsWithTag(slimeTag);
        float closestDistance = Mathf.Infinity;
        Transform targetSlime = null;

        foreach (GameObject slime in slimes)
        {
            float distance = Vector2.Distance(transform.position, slime.transform.position);
            if (distance < closestDistance && distance <= visionRange)
            {
                closestDistance = distance;
                targetSlime = slime.transform;
            }
        }

        if (targetSlime != null)
        {
            ChaseAndAttackSlime(targetSlime);
        }
    }

    private void ChaseAndAttackSlime(Transform target)
    {
        Vector2 direction = (target.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance < attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            SlimeWalker slimeWalker = target.GetComponent<SlimeWalker>();
            if (slimeWalker != null)
            {
                Vector3 damageSourcePosition = transform.position; // Position of the RandomWalker as the damage source
                slimeWalker.attributes.TakeDamage(attackDamage, damageSourcePosition, slimeWalker);
                lastAttackTime = Time.time;
                Debug.Log($"{attributes.characterName} attacked slime {slimeWalker.attributes.characterName} for {attackDamage} damage.");
            }
        }
        else
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }


    private void FindNearestBush()
    {
        GameObject[] bushes = GameObject.FindGameObjectsWithTag(bushTag);
        float closestDistance = Mathf.Infinity;
        targetBush = null;

        foreach (GameObject bush in bushes)
        {
            float distance = Vector2.Distance(transform.position, bush.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetBush = bush.transform;
            }
        }
    }

    private void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        float closestDistance = Mathf.Infinity;
        targetPlayer = null;

        foreach (GameObject player in players)
        {
            if (player == gameObject)
                continue;

            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closestDistance && distance <= visionRange)
            {
                closestDistance = distance;
                targetPlayer = player.transform;
            }
        }
    }

    private void SmoothlyChangeDirection()
    {
        movementDirection = Vector2.Lerp(movementDirection, targetDirection, directionChangeSmoothness * Time.deltaTime);
    }

    private void ChaseTarget(Transform target)
    {
        Vector2 direction = (target.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target.position);

        if (target == targetBush && distance < 0.5f)
        {
            if (Time.time >= lastEatTime + eatCooldown)
            {
                EatBush(targetBush);
                lastEatTime = Time.time;
                SetNewTargetDirection();
                shouldChaseBush = false;
                targetBush = null;
            }
        }
        else
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }

    private void EatBush(Transform bush)
    {
        Destroy(bush.gameObject);
        attributes.currentHunger += 20f;
        attributes.currentHunger = Mathf.Clamp(attributes.currentHunger, 0, attributes.maxHunger);
        Debug.Log("Character ate and gained hunger.");
        GetComponent<LevelHumans>().GainXp(5);
    }

    public void CreateChild(RandomWalker parent1, RandomWalker parent2)
    {
        // Verify if both parents can create a child
        if (!parent1.attributes.CanCreateChild(Time.time) || !parent2.attributes.CanCreateChild(Time.time))
        {
            Debug.Log("Cannot create child, cooldown active or age not suitable.");
            return;
        }

        // Proceed with child creation
        string firstName = CharacterAttributes.GenerateRandomFantasyName();
        float averageMaxAge = (parent1.attributes.maxAge + parent2.attributes.maxAge) / 2f;
        float averageSpeed = (parent1.moveSpeed + parent2.moveSpeed) / 2f;
        float averageVision = (parent1.visionRange + parent2.visionRange) / 2f;
        float visionThreshold = 0.2f;
        float speedThreshold = 0.6f;

        float minVision = averageVision - (averageVision * visionThreshold);
        float maxVision = averageVision + (averageVision * visionThreshold);
        float childVision = Mathf.Round(Random.Range(minVision, maxVision) * 10) / 10f;

        float minSpeed = averageSpeed - (averageSpeed * speedThreshold);
        float maxSpeed = averageSpeed + (averageSpeed * speedThreshold);
        float childSpeed = Mathf.Round(Random.Range(minSpeed, maxSpeed) * 10) / 10f;

        float maxMaxAge = averageMaxAge + (averageMaxAge * visionThreshold);
        float minMaxAge = averageMaxAge - (averageMaxAge * visionThreshold);
        float childMaxAge = Mathf.Round(Random.Range(minMaxAge, maxMaxAge) * 10) / 10f;

        Vector3 childPositionOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
        GameObject childObject = Instantiate(parent1.gameObject, parent1.transform.position + childPositionOffset, Quaternion.identity);
        childObject.name = firstName;

        RandomWalker childWalker = childObject.GetComponent<RandomWalker>();
        childWalker.visionRange = childVision;
        childWalker.moveSpeed = childSpeed;
        childWalker.attributes.currentHunger = childWalker.attributes.maxHunger;
        childWalker.attributes.currentHp = childWalker.attributes.maxHp;
        childWalker.attributes.currentAge = 0f;
        childWalker.attributes.maxAge = childMaxAge;
        childWalker.attributes.characterName = firstName;

        GameObject humansParent = GameObject.Find("Humans");
        if (humansParent != null)
        {
            Vector3 worldPosition = childObject.transform.position;
            childObject.transform.SetParent(humansParent.transform);
            childObject.transform.position = worldPosition;
        }

        parent1.attributes.RecordChildCreation(Time.time);
        parent2.attributes.RecordChildCreation(Time.time);
        parent1.GetComponent<LevelHumans>().GainXp(10);
        parent2.GetComponent<LevelHumans>().GainXp(10);

        Debug.Log($"Child created: {childWalker.attributes.characterName}, Vision: {childWalker.visionRange}, Speed: {childWalker.moveSpeed}");
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, groundLayer);
        return hit.collider != null;
    }

    private void AvoidEdgesAndMove()
    {
        if (IsNearEdge())
        {
            SetNewTargetDirection();
        }
        else
        {
            transform.Translate(movementDirection * moveSpeed * Time.deltaTime);
        }
    }

    private bool IsNearEdge()
    {
        RaycastHit2D hitForward = Physics2D.Raycast(transform.position, movementDirection, edgeAvoidanceRange, groundLayer);
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position + (Vector3)movementDirection * edgeAvoidanceRange, Vector2.down, 0.1f, groundLayer);
        return hitForward.collider == null || hitDown.collider == null;
    }

    private void MoveToNearestGround()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, visionRange, Vector2.zero, 0, groundLayer);

        if (hits.Length > 0)
        {
            Vector3 nearestGround = hits[0].point;
            float shortestDistance = Vector2.Distance(transform.position, nearestGround);

            foreach (RaycastHit2D hit in hits)
            {
                float distance = Vector2.Distance(transform.position, hit.point);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestGround = hit.point;
                }
            }

            Vector2 directionToGround = (nearestGround - transform.position).normalized;
            transform.Translate(directionToGround * moveSpeed * Time.deltaTime);
        }
    }

    public void TriggerSeparation(float duration)
    {
        separationEndTime = Time.time + duration;
        SetNewTargetDirection();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.1f);
    }
}
