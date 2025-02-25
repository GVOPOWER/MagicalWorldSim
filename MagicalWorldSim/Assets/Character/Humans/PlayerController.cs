using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float visionRange = 5f;
    public float attackCooldown = 1f;
    public int damage = 20;
    public string slimeTag = "Slime"; // Tag for identifying slimes

    public CharacterAttributes attributes = new CharacterAttributes();

    private Transform targetSlime;
    private float lastAttackTime = -Mathf.Infinity;
    private Animator animator;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        attributes.InitializeAttributes();

        if (rb == null || animator == null)
        {
            Debug.LogError("Essential components are missing on PlayerController.");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        attributes.HandleHunger(Time.deltaTime);
        FindNearestSlime();

        if (targetSlime != null)
            MoveTowardsSlime();
        else
            SetIdleAnimation();

        if (attributes.currentHp <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void FindNearestSlime()
    {
        GameObject[] slimes = GameObject.FindGameObjectsWithTag(slimeTag);
        float closestDistance = Mathf.Infinity;
        targetSlime = null;

        foreach (GameObject slime in slimes)
        {
            float distance = Vector2.Distance(transform.position, slime.transform.position);
            if (distance < closestDistance && distance <= visionRange)
            {
                closestDistance = distance;
                targetSlime = slime.transform;
            }
        }
    }

    private void MoveTowardsSlime()
    {
        if (targetSlime == null)
            return;

        Vector2 direction = (targetSlime.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetSlime.position);

        if (distance > 0.5f)
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
            UpdateAnimationDirection(direction);
        }
        else
        {
            AttackSlime(targetSlime.GetComponent<SlimeAttributes>());
        }
    }

    private void AttackSlime(SlimeAttributes slime)
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            slime.TakeDamage(damage);
        }
    }

    private void UpdateAnimationDirection(Vector2 direction)
    {
        animator.SetBool("isWalking", true);
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
    }

    private void SetIdleAnimation()
    {
        animator.SetBool("isWalking", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}
