using UnityEngine;

public class RandomWalker : MonoBehaviour
{
    // Hunger and Reproduction
    public float maxHunger = 100f;
    public float currentHunger;
    public float hungerDecreaseRate = 1f;
    public float vision = 5f; // Vision level
    public float childCreationCooldown = 10f;  // Cooldown period in seconds
    private float lastChildCreationTime = -Mathf.Infinity;
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    public Sprite circleSprite; // The sprite used for parents and children

    // Health Attributes
    public float maxHp = 100f;
    public float currentHp;

    // Age Attributes
    public float currentAge = 0f; // Current age in "years"
    public float ageIncrementRate = 1f / 60f; // Age increment rate per second (e.g., 1 year per minute)
    public float minReproductiveAge = 18f; // Minimum age for reproduction
    public float maxReproductiveAge = 50f; // Maximum age for reproduction

    // Movement and Interaction
    public float moveSpeed = 2f;
    public float changeDirectionInterval = 2f;
    public float directionChangeSmoothness = 5f; // Increased smoothness factor for quicker direction changes
    public float visionRange = 5f;
    public LayerMask groundLayer;
    public float hungerThreshold = 75f;
    public string bushTag = "Bush";
    public float eatCooldown = 2f; // Cooldown between eating actions in seconds
    public float edgeAvoidanceRange = 0.5f; // Distance to avoid edges
    public float separationDuration = 1f; // Duration to move away from another player after creating a child

    private Vector2 movementDirection;
    private Vector2 targetDirection;
    private bool shouldChaseBush = false;
    private bool shouldChasePlayer = false;
    private bool isGrounded = false;
    private Transform targetBush;
    private Transform targetPlayer;
    private float lastEatTime = -Mathf.Infinity; // Track the last time the player ate
    private float separationEndTime = -Mathf.Infinity; // Time when separation ends
public float cityCreationDistance = 1.0f; // Distance to consider for city creation
    public float cityCreationProbability = 0.5f; // 50% chance to form a city when conditions are met
    public GameObject cityPrefab;
    private void Start()
    {
        currentHunger = maxHunger;
        currentHp = maxHp;

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = circleSprite;
        
        // Ensure the player has the correct tag for city detection
        gameObject.tag = "Player";

        InvokeRepeating("SetNewTargetDirection", 0f, changeDirectionInterval);
        targetDirection = GetRandomDirection();
        movementDirection = targetDirection;
    }

    private void Update()
    {
        HandleHungerAndHealth();
        IncrementAge();

        if (Time.time < separationEndTime)
        {
            transform.Translate(movementDirection * moveSpeed * Time.deltaTime);
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

        CheckForCityCreation();
    }

    private void CheckForCityCreation()
    {
        RandomWalker[] players = FindObjectsOfType<RandomWalker>();

        foreach (RandomWalker otherPlayer in players)
        {
            if (otherPlayer == this || !otherPlayer.CanCreateChild())
                continue;

            float distance = Vector2.Distance(transform.position, otherPlayer.transform.position);

            if (distance <= cityCreationDistance && Random.value < cityCreationProbability)
            {
                CreateCity(this, otherPlayer);
                break;
            }
        }
    }

    private void CreateCity(RandomWalker player1, RandomWalker player2)
    {
        if (cityPrefab == null)
        {
            Debug.LogError("City Prefab is not assigned!");
            return;
        }

        Vector3 cityPosition = (player1.transform.position + player2.transform.position) / 2;
        Instantiate(cityPrefab, cityPosition, Quaternion.identity);

        Debug.Log($"City created by {player1.name} and {player2.name}");

        player1.lastChildCreationTime = Time.time;
        player2.lastChildCreationTime = Time.time;

        player1.TriggerSeparation(separationDuration);
        player2.TriggerSeparation(separationDuration);
    }

    public void TriggerSeparation(float duration)
    {
        separationEndTime = Time.time + duration; // Set separation end time
        SetNewTargetDirection(); // Change direction to move away
    }

    private void HandleHungerAndHealth()
    {
        // Decrease hunger over time
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        if (currentHunger <= 0)
        {
            Debug.Log("Player is starving!");
            // Decrease health when starving
            currentHp -= hungerDecreaseRate * Time.deltaTime; // You can adjust the rate
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        }

        // Check if health is zero
        if (currentHp <= 0)
        {
            Debug.Log("Player has died due to starvation.");
            Destroy(gameObject); // Destroy the object
        }
    }

    private void IncrementAge()
    {
        currentAge += ageIncrementRate * Time.deltaTime;
    }

    public bool CanCreateChild()
    {
        bool isWithinReproductiveAge = currentAge >= minReproductiveAge && currentAge <= maxReproductiveAge;
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
        // Check if both parents can create a child
        if (!parent1.CanCreateChild() || !parent2.CanCreateChild())
        {
            Debug.Log("Cannot create child, cooldown active or age not suitable.");
            return;
        }

        // Example attribute: Vision
        float averageVision = (parent1.visionRange + parent2.visionRange) / 2f;
        float visionThreshold = 0.2f; // 20% variability

        // Calculate the child's vision with variability
        float minVision = averageVision - (averageVision * visionThreshold);
        float maxVision = averageVision + (averageVision * visionThreshold);
        float childVision = Mathf.Round(Random.Range(minVision, maxVision) * 10) / 10f; // One decimal precision

        // Clone the parent GameObject
        GameObject childObject = Instantiate(parent1.gameObject, parent1.transform.position, Quaternion.identity);
        childObject.name = "Child";

        // Get the RandomWalker component from the cloned object
        RandomWalker childAttributes = childObject.GetComponent<RandomWalker>();

        // Assign the calculated vision to the child
        childAttributes.visionRange = childVision;

        // Reset other attributes as needed
        childAttributes.currentHunger = childAttributes.maxHunger; // Reset hunger for the child
        childAttributes.currentHp = childAttributes.maxHp; // Reset health for the child
        childAttributes.currentAge = 0f; // Reset age for the child

        // Set cooldown times for both parents
        parent1.lastChildCreationTime = Time.time;
        parent2.lastChildCreationTime = Time.time;

        Debug.Log($"Child created with vision: {childVision}, as a result of {parent1.name} and {parent2.name}");
    }

    private void SetNewTargetDirection()
    {
        targetDirection = GetRandomDirection();
    }

    private Vector2 GetRandomDirection()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomY = Random.Range(-1f, 1f);
        return new Vector2(randomX, randomY).normalized;
    }

    private void SmoothlyChangeDirection()
    {
        // Interpolate movement direction towards the target direction
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

                // After eating, change direction to avoid the bush
                SetNewTargetDirection();
                shouldChaseBush = false; // Stop chasing the bush after eating
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

            // Set separation time
            separationEndTime = Time.time + separationDuration;
            SetNewTargetDirection(); // Change direction for separation
            targetPlayer = null; // Reset to avoid continuous creation
        }
    }

    private void EatBush(Transform bush)
    {
        // Here we assume that eating the bush will destroy it
        Destroy(bush.gameObject);
        Eat(20f); // Assume eating the bush restores a certain amount of hunger
        Debug.Log("Player ate the bush and gained hunger.");
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
        // Use raycasts in multiple directions to find the nearest ground
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

            // Move towards the nearest ground
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

            if (CanCreateChild()) // Only look for a mate if not on cooldown
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
