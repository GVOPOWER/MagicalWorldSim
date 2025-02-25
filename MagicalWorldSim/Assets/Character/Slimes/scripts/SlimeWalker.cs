using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SlimeWalker : MonoBehaviour
{

    public SlimeAttributes attributes = new SlimeAttributes();


    public float moveSpeed = 1.5f;
    public float slowMoveSpeedFactor = 0.6f;
    public float changeDirectionInterval = 3f;
    public float directionChangeSmoothness = 3f;
    public float visionRange = 3f;
    public LayerMask groundLayer;
    public string foodTag = "Food";
    public float eatCooldown = 1.5f;
    public float edgeAvoidanceRange = 0.3f;

    private Vector2 movementDirection;
    private Vector2 targetDirection;
    private bool shouldChaseFood = false;
    private Transform targetFood;
    private float lastEatTime = -Mathf.Infinity;
    private Rigidbody2D rb;
    private List<Tilemap> tilemaps = new List<Tilemap>();
    public TileBase[] walkableTiles;
    public TileBase[] slowWalkableTiles;
    public TileBase[] unwalkableTiles;

    private float separationEndTime = -Mathf.Infinity;
    private const float reproductionAge = 5f;

    // Knockback variables
    public float knockbackDistance = 2f;
    public float knockbackDuration = 0.5f;
    private bool isKnockedBack = false;
    private Vector2 knockbackTarget;
    private float knockbackStartTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing.");
            enabled = false;
            return;
        }
        rb.isKinematic = true;
        tilemaps.AddRange(FindObjectsOfType<Tilemap>());

        if (tilemaps.Count == 0)
        {
            Debug.LogError("No Tilemaps found.");
            enabled = false;
            return;
        }

        InvokeRepeating("SetNewTargetDirection", 0f, changeDirectionInterval);
        targetDirection = GetRandomDirection();
        movementDirection = targetDirection;
    }

    private void Update()
    {
        if (attributes.currentHp <= 0)
        {
            Debug.Log($"{attributes.characterName} has dissolved due to low health.");
            Destroy(gameObject);
            return;
        }

        if (Time.time < separationEndTime)
            return;

        if (isKnockedBack)
        {
            HandleKnockback();
        }
        else
        {
            HandleSlimeLifecycle();
            MoveCharacter();
        }
    }

    public void ApplyKnockback(Vector3 damageSourcePosition)
    {
        Vector2 knockbackDirection = ((Vector2)transform.position - (Vector2)damageSourcePosition).normalized;
        knockbackTarget = (Vector2)transform.position + knockbackDirection * knockbackDistance;
        knockbackStartTime = Time.time;
        isKnockedBack = true;
    }

    private void HandleKnockback()
    {
        float elapsed = Time.time - knockbackStartTime;
        if (elapsed < knockbackDuration)
        {
            transform.position = Vector2.Lerp(transform.position, knockbackTarget, elapsed / knockbackDuration);
        }
        else
        {
            transform.position = knockbackTarget;
            isKnockedBack = false;
        }
    }

    private void HandleSlimeLifecycle()
    {
        attributes.HandleHunger(Time.deltaTime);
        attributes.IncrementAge(Time.deltaTime);
        UpdateTarget();

        if (attributes.currentAge >= reproductionAge)
        {
            Reproduce();
            return;
        }

        if (IsGrounded())
        {
            if (shouldChaseFood && targetFood != null)
            {
                ChaseTarget(targetFood);
            }
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
            transform.position = currentPosition + (movementDirection * currentSpeed * Time.deltaTime);
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

    private float AdjustSpeedBasedOnTile(TileBase currentTile)
    {
        if (System.Array.Exists(slowWalkableTiles, tile => tile == currentTile))
            return moveSpeed * slowMoveSpeedFactor;
        else if (System.Array.Exists(unwalkableTiles, tile => tile == currentTile))
            return 0;
        else
            return moveSpeed;
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

    private void UpdateTarget()
    {
        if (attributes.currentHunger <= attributes.maxHunger * 0.10f)
        {
            FindNearestFood();
            shouldChaseFood = targetFood != null && Vector2.Distance(transform.position, targetFood.position) <= visionRange;
        }
        else
        {
            shouldChaseFood = false;
            targetFood = null;
        }
    }

    private void FindNearestFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag(foodTag);
        float closestDistance = Mathf.Infinity;
        targetFood = null;

        foreach (GameObject food in foods)
        {
            float distance = Vector2.Distance(transform.position, food.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetFood = food.transform;
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

        if (target == targetFood && distance < 0.5f)
        {
            if (Time.time >= lastEatTime + eatCooldown)
            {
                EatFood(targetFood);
                lastEatTime = Time.time;
                SetNewTargetDirection();
                shouldChaseFood = false;
                targetFood = null;
            }
        }
        else
        {
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
        }
    }

    private void EatFood(Transform food)
    {
        Destroy(food.gameObject);
        attributes.currentHunger += 15f;
        attributes.currentHunger = Mathf.Clamp(attributes.currentHunger, 0, attributes.maxHunger);
        Debug.Log("Slime consumed and gained hunger.");
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
            transform.position += (Vector3)(movementDirection * moveSpeed * Time.deltaTime);
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
            transform.position += (Vector3)(directionToGround * moveSpeed * Time.deltaTime);
        }
    }

    public void TriggerSeparation(float duration)
    {
        separationEndTime = Time.time + duration;
        SetNewTargetDirection();
    }

    private void Reproduce()
    {
        Debug.Log($"{attributes.characterName} is reproducing!");

        CreateOffspring(new Vector3(0.5f, 0, 0));
        CreateOffspring(new Vector3(-0.5f, 0, 0));

        Destroy(gameObject);
    }

    private void CreateOffspring(Vector3 offset)
    {
        GameObject newSlime = Instantiate(gameObject, transform.position + offset, Quaternion.identity);
        SlimeWalker slimeWalker = newSlime.GetComponent<SlimeWalker>();

        if (slimeWalker != null)
        {
            slimeWalker.attributes.InitializeAttributes();
            slimeWalker.attributes.currentAge = 0;
            slimeWalker.attributes.characterName = SlimeAttributes.GenerateSlimeName();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}
