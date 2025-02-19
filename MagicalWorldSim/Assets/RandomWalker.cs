using UnityEngine;

public class RandomWalker : MonoBehaviour
{

    public string name = "Bob";
    public float maxHunger = 100f;
    public float currentHunger;
    public float hungerDecreaseRate = 1f;
    public float vision = 5f; // Vision level
    public float childCreationCooldown = 10f;  // Cooldown period in seconds
    private float lastChildCreationTime = -Mathf.Infinity;
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    public Sprite circleSprite; // The sprite used for parents and children
    public float maxChildren = 5;
    public float children = 0;
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
    public float separationDuration = 1f;

    public float maxAge = 100;
    
    public float moveSpeedHunger = 0;
    private Vector2 movementDirection;
    private Vector2 targetDirection;
    private bool shouldChaseBush = false;
    private bool shouldChasePlayer = false;
    private bool isGrounded = false;
    private Transform targetBush;
    private Transform targetPlayer;
    private float lastEatTime = -Mathf.Infinity; // Track the last time the player ate
    private float separationEndTime = -Mathf.Infinity; // Time when separation ends

    private void Start()
    {
        currentHunger = maxHunger;
        currentHp = maxHp; // Initialize currentHp to maxHp
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = circleSprite;

        InvokeRepeating("SetNewTargetDirection", 0f, changeDirectionInterval);
        targetDirection = GetRandomDirection();
        movementDirection = targetDirection; // Initialize movement direction
    }

    private void Update()
    {
        DieOfAge();
        HandleHungerAndHealth();
        IncrementAge(); // Increment the age over time

        if (Time.time < separationEndTime)
        {
            // Continue moving in a random direction during separation
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
            // Move towards the nearest ground
            MoveToNearestGround();
        }
    }


    public void TriggerSeparation(float duration)
    {
        separationEndTime = Time.time + duration; // Set separation end time
        SetNewTargetDirection(); // Change direction to move away
    }

    private void HandleHungerAndHealth()
    {
        moveSpeedHunger = moveSpeed / 3;
        currentHunger -= hungerDecreaseRate * Time.deltaTime * moveSpeedHunger;
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

private void DieOfAge() 
{
    if (currentAge >= maxAge) 
    {
        Debug.Log($"{name} has died of old age at {currentAge}");
        currentHp = 0; // Destroy the object
    }
}

 private static readonly Random random = new Random();

    // Lists of first and last names
    private static readonly List<string> firstNames = new List<string>
    {
        "Arin", "Borin", "Celdor", "Durnan", "Elandor", "Faelan", "Gorim", "Haldir", "Ithil", "Jareth"
    };

    private static readonly List<string> lastNames = new List<string>
    {
        "Stormwind", "Ironfist", "Moonshadow", "Duskbringer", "Starlight", "Thunderstrike", "Silverleaf", "Shadowbane", "Brightstar", "Nightwhisper"
    };

    // Method to generate a random first name
    public static string GenerateRandomFirstName()
    {
        return firstNames[random.Next(firstNames.Count)];
    }

    // Method to generate a random last name
    public static string GenerateRandomLastName()
    {
        return lastNames[random.Next(lastNames.Count)];
    }

    // Method to generate a full random fantasy name
    public static string GenerateRandomFantasyName()
    {
        string firstName = GenerateRandomFirstName();
        string lastName = GenerateRandomLastName();
        return $"{firstName} {lastName}";
    }


class Program
{
    static void Main(string[] args)
    {
        // Generate and print a random fantasy name
        string fantasyName = FantasyNameGenerator.GenerateRandomFantasyName();
        Console.WriteLine($"Generated Fantasy Name: {fantasyName}");
    }
}
    
    private void IncrementAge()
    {
        currentAge += ageIncrementRate * Time.deltaTime;
    }

    public bool CanCreateChild()
    {
        bool isWithinReproductiveAge = currentAge >= minReproductiveAge && currentAge <= maxReproductiveAge && children != maxChildren;
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
        string firstName = GenerateRandomFirstName();
        string lastName = GenerateRandomLastName();
        float averageMaxAge = (parent1.maxAge + parent2.maxAge) / 2f;
        float averageSpeed = (parent1.moveSpeed + parent2.moveSpeed) / 2f;
        float averageVision = (parent1.visionRange + parent2.visionRange) / 2f;
        float visionThreshold = 0.2f; // 20% variability
        float speedThreshold = 0.6f; // 60% variability

        // Calculate the child's vision with variability
        float minVision = averageVision - (averageVision * visionThreshold);
        float maxVision = averageVision + (averageVision * visionThreshold);
        float childVision = Mathf.Round(Random.Range(minVision, maxVision) * 10) / 10f; // One decimal precision

        float minspeed = averageSpeed - (averageSpeed * speedThreshold);
        float maxspeed = averageSpeed + (averageSpeed * speedThreshold);
        float childSpeed = Mathf.Round(Random.Range(minspeed, maxspeed) * 10) / 10f; // One decimal precision
        float MaxMaxAge = averageMaxAge + (averageMaxAge * visionThreshold);
        float MinMaxAge = averageMaxAge - (averageMaxAge * visionThreshold);
        float childMaxAge = Mathf.Round(Random.Range(MinMaxAge, MaxMaxAge) * 10) / 10f; // One decimal precision
        string firstName = GenerateRandomFirstName();
        string lastName = GenerateRandomLastName();

        GameObject childObject = Instantiate(parent1.gameObject, parent1.transform.position, Quaternion.identity);
        childObject.name = $"{firstName} {lastName}";

        // Get the RandomWalker component from the cloned object
        RandomWalker childAttributes = childObject.GetComponent<RandomWalker>();

        // Assign the calculated vision to the child
        childAttributes.visionRange = childVision;
        childAttributes.moveSpeed = childSpeed;
        // Reset other attributes as needed
        childAttributes.currentHunger = childAttributes.maxHunger; // Reset hunger for the child
        childAttributes.currentHp = childAttributes.maxHp; // Reset health for the child
        childAttributes.currentAge = 0f; // Reset age for the child
        childAttributes.maxAge = childMaxAge;
        // Set cooldown times for both parents
        parent1.lastChildCreationTime = Time.time;
        parent2.lastChildCreationTime = Time.time;

        parent1.children += 1;
        parent2.children += 1;

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
