using UnityEngine;
using UnityEngine.UI;

public class PlayerHunger : MonoBehaviour
{
    public float maxHunger = 100f;
    public float currentHunger;
    public float hungerDecreaseRate = 1f;
    public int vision = 5; // Vision level
    public float childCreationCooldown = 10f;  // Cooldown period in seconds
    private float lastChildCreationTime = -Mathf.Infinity;
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    public Sprite circleSprite; // The sprite used for parents and children

    private void Start()
    {
        currentHunger = maxHunger;
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = circleSprite;
    }

    private void Update()
    {
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        if (currentHunger <= 0)
        {
            Debug.Log("Player is starving!");
            // Implement any effects of starving (e.g., reduce health)
        }
    }

    public bool CanCreateChild()
    {
        return Time.time >= lastChildCreationTime + childCreationCooldown;
    }

    public void Eat(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        Debug.Log("Player ate and gained hunger.");
    }

    public void CreateChild(PlayerHunger parent1, PlayerHunger parent2)
    {
        if (!parent1.CanCreateChild() || !parent2.CanCreateChild())
        {
            Debug.Log("Cannot create child, cooldown active.");
            return;
        }

        PlayerHunger chosenParent = Random.value < 0.5f ? parent1 : parent2;

        GameObject childObject = new GameObject("Child");
        childObject.transform.position = chosenParent.transform.position;

        PlayerHunger childAttributes = childObject.AddComponent<PlayerHunger>();
        childAttributes.vision = chosenParent.vision;
        childAttributes.maxHunger = chosenParent.maxHunger;
        childAttributes.currentHunger = chosenParent.currentHunger;
        childAttributes.circleSprite = chosenParent.circleSprite;

        SpriteRenderer childSpriteRenderer = childObject.AddComponent<SpriteRenderer>();
        childSpriteRenderer.sprite = chosenParent.circleSprite;

        parent1.lastChildCreationTime = Time.time;
        parent2.lastChildCreationTime = Time.time;

        Debug.Log($"Child created as a clone of {chosenParent.name}");
    }
}
