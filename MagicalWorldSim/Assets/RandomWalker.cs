using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomWalker : MonoBehaviour
{
    public string name = "Bob";
    public float maxHunger = 100f;
    public float currentHunger;
    public float hungerDecreaseRate = 1f;
    public float vision = 5f;
    public float childCreationCooldown = 10f;
    private float lastChildCreationTime = -Mathf.Infinity;
    public SpriteRenderer spriteRenderer;
    public Sprite circleSprite;
    public float maxChildren = 5;
    public float children = 0;

    public float maxHp = 100f;
    public float currentHp;

    public float currentAge = 0f;
    public float ageIncrementRate = 1f / 60f;
    public float minReproductiveAge = 18f;
    public float maxReproductiveAge = 50f;

    public float moveSpeed = 2f;
    public float slowMoveSpeedFactor = 0.5f;
    public float changeDirectionInterval = 2f;
    public float directionChangeSmoothness = 5f;
    public float visionRange = 5f;
    public LayerMask groundLayer;
    public float hungerThreshold = 75f;
    public string bushTag = "Bush";
    public float eatCooldown = 2f;
    public float edgeAvoidanceRange = 0.5f;
    public float separationDuration = 1f;

    public float maxAge = 100;

    private Vector2 movementDirection;
    private Vector2 targetDirection;
    private bool shouldChaseBush = false;
    private bool shouldChasePlayer = false;
    private bool isGrounded = false;
    private Transform targetBush;
    private Transform targetPlayer;
    private float lastEatTime = -Mathf.Infinity;
    private float separationEndTime = -Mathf.Infinity;
    private float pauseTime = 0f;
    private bool isPaused = false;
    Animator animator;
    Rigidbody2D rb;

    // Changed from a single Tilemap to a List of Tilemaps
    private List<Tilemap> tilemaps = new List<Tilemap>();
    public TileBase[] walkableTiles;
    public TileBase[] slowWalkableTiles;
    public TileBase[] unwalkableTiles;
    public CityCreation cityCreation; // Ensure this is defined in the class
    public string currentCity = ""; // Ensure this is defined in the class

    private void Start()
    {
        currentHunger = maxHunger;
        currentHp = maxHp;
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = circleSprite;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is not assigned. Please ensure it is attached to the GameObject.");
        }

        // Find all Tilemaps in the scene with the tag "Tilemap"
        Tilemap[] foundTilemaps = GameObject.FindObjectsOfType<Tilemap>();
        tilemaps.AddRange(foundTilemaps);

        if (tilemaps.Count == 0)
        {
            Debug.LogError("No Tilemaps found in the scene. Ensure there are Tilemap components present.");
            return;
        }

        InvokeRepeating("SetNewTargetDirection", 0f, changeDirectionInterval);
        targetDirection = GetRandomDirection();
        movementDirection = targetDirection;
    }

    private void Update()
    {
        if (rb == null || tilemaps.Count == 0)
        {
            return; // Exit if essential components are missing
        }

        // Handle pause logic
        if (isPaused)
        {
            SetIdleAnimation();
            if (Time.time >= pauseTime)
            {
                isPaused = false; // Resume movement after pause
            }
            return;
        }
        else if (Random.value < 0.005f) // Adjusted to 0.5% chance to pause each frame
        {
            isPaused = true;
            pauseTime = Time.time + Random.Range(0.5f, 1.5f);
            return;
        }

        if (CanCreateCity())
        {
            CreateCity();
        }

        // Determine current tile from all tilemaps
        Vector3Int currentCellPosition = GetCurrentTilemap().WorldToCell(transform.position);
        TileBase currentTile = GetCurrentTilemap().GetTile(currentCellPosition);

        // Determine current movement speed based on the tile type
        float currentSpeed = AdjustSpeedBasedOnTile(currentTile);

        // Check next position
        Vector3 nextPosition = transform.position + (Vector3)(movementDirection * currentSpeed * Time.deltaTime);
        Vector3Int nextCellPosition = GetCurrentTilemap().WorldToCell(nextPosition);
        TileBase nextTile = GetCurrentTilemap().GetTile(nextCellPosition);

        if (System.Array.Exists(unwalkableTiles, tile => tile == nextTile))
        {
            // If next tile is unwalkable, find a new direction away from unwalkable tiles
            SetNewTargetDirectionAwayFromUnwalkable();
            SetIdleAnimation();
        }
        else
        {
            // Move in the current direction
            transform.Translate(movementDirection * currentSpeed * Time.deltaTime);

            // Update animation based on movement direction
            UpdateAnimationDirection();
        }

        // Other behaviors
        DieOfAge();
        HandleHungerAndHealth();
        IncrementAge();

        if (Time.time < separationEndTime)
        {
            return;
        }

        UpdateTarget();

        isGrounded = IsGrounded();

        if (isGrounded)
        {
            if (shouldChaseBush && targetBush != null)
            {
                ChaseTarget(targetBush);
            }
            else if (shouldChasePlayer && targetPlayer != null)
            {
                ChaseTarget(targetPlayer);
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

    private Tilemap GetCurrentTilemap()
    {
        // Select the first tilemap that contains the current position
        foreach (Tilemap tilemap in tilemaps)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
            if (tilemap.GetTile(cellPosition) != null)
            {
                return tilemap;
            }
        }
        return null; // Return null if no tilemap contains the position
    }

    private bool CanCreateCity()
    {
        // Define your conditions for city creation
        return currentAge >= 30f && currentHunger > 50f && string.IsNullOrEmpty(currentCity); // Ensure not in a city
    }

    private void CreateCity()
    {
        if (cityCreation != null)
        {
            cityCreation.CreateCity(transform.position); // Call the CreateCity method in CityCreation
            currentCity = cityCreation.GenerateRandomCityName(); // Assign a new city name when created
        }
        else
        {
            Debug.LogError("CityCreation reference is not set on RandomWalker");
        }
    }

    public void SetCurrentCity(string cityName)
    {
        currentCity = cityName;
    }

    // Method to clear the current city (e.g., when the character leaves a city)
    public void ClearCurrentCity()
    {
        currentCity = "";
    }

    private void SetIdleAnimation()
    {
        animator.SetBool("isIdle", true);
        animator.SetBool("isWalkingLeft", false);
        animator.SetBool("isWalkingRight", false);
        animator.SetBool("isWalkingUp", false);
        animator.SetBool("isWalkingDown", false);
    }

    private void UpdateAnimationDirection()
    {
        if (movementDirection.magnitude > 0.01f) // Ensure significant movement
        {
            animator.SetBool("isIdle", false); // Clear idle state
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
        {
            return moveSpeed * slowMoveSpeedFactor; // Slow down movement on slow walkable tiles
        }
        else if (System.Array.Exists(unwalkableTiles, tile => tile == currentTile))
        {
            return 0; // No movement on unwalkable tiles
        }
        else
        {
            return moveSpeed; // Normal movement on walkable tiles
        }
    }

    private IEnumerable<Vector3Int> GetAdjacentPositions(Vector3Int position)
    {
        // Returns a list of adjacent positions (right, left, up, down) relative to the given position
        return new List<Vector3Int>
        {
            position + new Vector3Int(1, 0, 0),  // Right
            position + new Vector3Int(-1, 0, 0), // Left
            position + new Vector3Int(0, 1, 0),  // Up
            position + new Vector3Int(0, -1, 0)  // Down
        };
    }

    private void SetNewTargetDirectionAwayFromUnwalkable()
    {
        List<Vector2> possibleDirections = new List<Vector2>
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right,
            Vector2.up + Vector2.left,  // Diagonal
            Vector2.up + Vector2.right, // Diagonal
            Vector2.down + Vector2.left, // Diagonal
            Vector2.down + Vector2.right // Diagonal
        };

        foreach (var direction in possibleDirections)
        {
            Vector3Int checkPosition = GetCurrentTilemap().WorldToCell(transform.position + (Vector3)direction);
            TileBase tile = GetCurrentTilemap().GetTile(checkPosition);
            if (System.Array.Exists(walkableTiles, walkableTile => walkableTile == tile))
            {
                targetDirection = direction.normalized;
                movementDirection = targetDirection;
                return; // Exit as soon as a valid direction is found
            }
        }

        // If no walkable direction is found, use a random direction
        targetDirection = GetRandomDirection();
        movementDirection = targetDirection;
    }

    private void MoveToNearestWalkableTile()
    {
        Vector3Int currentCellPosition = GetCurrentTilemap().WorldToCell(transform.position);
        Vector3Int nearestWalkableTilePosition = currentCellPosition;
        float shortestDistance = Mathf.Infinity;

        foreach (Vector3Int pos in GetAdjacentPositions(currentCellPosition))
        {
            TileBase tile = GetCurrentTilemap().GetTile(pos);
            if (System.Array.Exists(walkableTiles, walkableTile => walkableTile == tile))
            {
                float distance = Vector3.Distance(GetCurrentTilemap().CellToWorld(pos), transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestWalkableTilePosition = pos;
                }
            }
        }

        // Move towards the nearest walkable tile
        Vector3 directionToWalkable = (GetCurrentTilemap().CellToWorld(nearestWalkableTilePosition) - transform.position).normalized;
        transform.Translate(directionToWalkable * moveSpeed * Time.deltaTime);
    }

    public void TriggerSeparation(float duration)
    {
        separationEndTime = Time.time + duration; // Set separation end time
        SetNewTargetDirection(); // Change direction to move away
    }

    private void HandleHungerAndHealth()
    {
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        if (currentHunger <= 0)
        {
            currentHp -= hungerDecreaseRate * Time.deltaTime; // Decrease health when starving
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        }

        // Check if health is zero
        if (currentHp <= 0)
        {
            Debug.Log("Player has died due to starvation.");
            Destroy(gameObject); // Destroy the object
        }
    }

    private void DieOfAge()
    {
        if (currentAge >= maxAge)
        {
            Debug.Log($"{name} has died of old age at {currentAge}");
            Destroy(gameObject); // Destroy the object
        }
    }

    private static readonly List<string> firstNames = new List<string>
    {
        "Arin", "Borin", "Celdor", "Durnan", "Elandor", "Faelan", "Gorim", "Haldir", "Ithil", "Jareth"
    };

    private static readonly List<string> lastNames = new List<string>
    {
        "Stormwind", "Ironfist", "Moonshadow", "Duskbringer", "Starlight", "Thunderstrike", "Silverleaf", "Shadowbane", "Brightstar", "Nightwhisper"
    };

    public static string GenerateRandomFirstName()
    {
        return firstNames[UnityEngine.Random.Range(0, firstNames.Count)];
    }

    public static string GenerateRandomLastName()
    {
        return lastNames[UnityEngine.Random.Range(0, lastNames.Count)];
    }

    public static string GenerateRandomFantasyName()
    {
        string firstName = GenerateRandomFirstName();
        string lastName = GenerateRandomLastName();
        return $"{firstName} {lastName}";
    }

    private void IncrementAge()
    {
        currentAge += ageIncrementRate * Time.deltaTime;
    }

    public bool CanCreateChild()
    {
        bool isWithinReproductiveAge = currentAge >= minReproductiveAge && currentAge <= maxReproductiveAge && children < maxChildren;
        return isWithinReproductiveAge && Time.time >= lastChildCreationTime + childCreationCooldown;
    }

    public void Eat(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        Debug.Log("Player ate and gained hunger.");
    }

    public void CreateChild(RandomWalker parent1, RandomWalker parent2)
    {
        if (!parent1.CanCreateChild() || !parent2.CanCreateChild())
        {
            Debug.Log("Cannot create child, cooldown active or age not suitable.");
            return;
        }

        string firstName = GenerateRandomFirstName();
        string lastName = GenerateRandomLastName();
        float averageMaxAge = (parent1.maxAge + parent2.maxAge) / 2f;
        float averageSpeed = (parent1.moveSpeed + parent2.moveSpeed) / 2f;
        float averageVision = (parent1.visionRange + parent2.visionRange) / 2f;
        float visionThreshold = 0.2f; // 20% variability
        float speedThreshold = 0.6f; // 60% variability

        float minVision = averageVision - (averageVision * visionThreshold);
        float maxVision = averageVision + (averageVision * visionThreshold);
        float childVision = Mathf.Round(UnityEngine.Random.Range(minVision, maxVision) * 10) / 10f;

        float minSpeed = averageSpeed - (averageSpeed * speedThreshold);
        float maxSpeed = averageSpeed + (averageSpeed * speedThreshold);
        float childSpeed = Mathf.Round(UnityEngine.Random.Range(minSpeed, maxSpeed) * 10) / 10f;

        float maxMaxAge = averageMaxAge + (averageMaxAge * visionThreshold);
        float minMaxAge = averageMaxAge - (averageMaxAge * visionThreshold);
        float childMaxAge = Mathf.Round(UnityEngine.Random.Range(minMaxAge, maxMaxAge) * 10) / 10f;

        GameObject childObject = Instantiate(parent1.gameObject, parent1.transform.position, Quaternion.identity);
        childObject.name = $"{firstName} {lastName}";

        RandomWalker childAttributes = childObject.GetComponent<RandomWalker>();

        GameObject humansParent = GameObject.Find("Humans");
        if (humansParent != null)
        {
            Vector3 worldPosition = childObject.transform.position;
            childObject.transform.SetParent(humansParent.transform);
            childObject.transform.position = worldPosition;
        }
        childAttributes.visionRange = childVision;
        childAttributes.moveSpeed = childSpeed;
        childAttributes.currentHunger = childAttributes.maxHunger;
        childAttributes.currentHp = childAttributes.maxHp;
        childAttributes.currentAge = 0f;
        childAttributes.maxAge = childMaxAge;
        childAttributes.name = $"{firstName} {lastName}";

        parent1.lastChildCreationTime = Time.time;
        parent2.lastChildCreationTime = Time.time;

        parent1.children += 1;
        parent2.children += 1;

        Debug.Log($"Child created with vision: {childAttributes.visionRange}, as a result of {parent1.name} and {parent2.name}");
    }

    private void SetNewTargetDirection()
    {
        targetDirection = GetRandomDirection();
    }

    private Vector2 GetRandomDirection()
    {
        float randomX = UnityEngine.Random.Range(-1f, 1f);
        float randomY = UnityEngine.Random.Range(-1f, 1f);
        return new Vector2(randomX, randomY).normalized;
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

        if (target == targetPlayer && distance < 0.5f)
        {
            RandomWalker targetPlayerWalker = target.GetComponent<RandomWalker>();
            CreateChild(this, targetPlayerWalker);

            separationEndTime = Time.time + separationDuration;
            SetNewTargetDirection();
            targetPlayer = null;
        }
    }

    private void EatBush(Transform bush)
    {
        Destroy(bush.gameObject);
        Eat(20f);
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

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, groundLayer);
        return hit.collider != null;
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

    private void UpdateTarget()
    {
        if (currentHunger <= hungerThreshold)
        {
            FindNearestBush();
            shouldChaseBush = targetBush != null && Vector2.Distance(transform.position, targetBush.position) <= visionRange;
        }
        else
        {
            shouldChaseBush = false;
            targetBush = null;

            if (CanCreateChild())
            {
                FindNearestPlayer();
                shouldChasePlayer = targetPlayer != null;
            }
            else
            {
                shouldChasePlayer = false;
                targetPlayer = null;
            }
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
        RandomWalker[] players = FindObjectsOfType<RandomWalker>();
        float closestDistance = Mathf.Infinity;
        targetPlayer = null;

        foreach (RandomWalker otherPlayer in players)
        {
            if (otherPlayer == this || otherPlayer.currentHunger <= hungerThreshold || !otherPlayer.CanCreateChild())
                continue;

            float distance = Vector2.Distance(transform.position, otherPlayer.transform.position);
            if (distance < closestDistance && distance <= visionRange)
            {
                closestDistance = distance;
                targetPlayer = otherPlayer.transform;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.1f);
    }
}
