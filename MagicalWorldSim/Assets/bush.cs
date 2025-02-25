using UnityEngine;

public class Bush : MonoBehaviour
{
    public float foodAmount = 5f; // Amount of food the bush provides
    public int maxUses = 5; // Maximum times the bush can be consumed
    private int currentUses = 0; // Track current uses

    public float hungerThresholdPercentage = 0.75f; // 75% threshold
    public float separationDuration = 1f; // Duration to move away after eating

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Ensure the player has the correct tag
        {
            RandomWalker walker = collision.GetComponent<RandomWalker>();
            if (walker != null && walker.attributes.currentHunger < walker.attributes.maxHunger * hungerThresholdPercentage)
            {
                Consume(walker);
            }
        }
    }

    private void Consume(RandomWalker walker)
    {
        if (currentUses < maxUses)
        {
            // Adjust hunger using attributes
            walker.attributes.currentHunger += foodAmount;
            walker.attributes.currentHunger = Mathf.Clamp(walker.attributes.currentHunger, 0, walker.attributes.maxHunger);
            Debug.Log($"{walker.attributes.characterName} ate and gained hunger.");

            currentUses++;

            // Trigger separation after eating
            walker.TriggerSeparation(separationDuration);

            if (currentUses >= maxUses)
            {
                Destroy(gameObject); // Destroy the bush after max uses
            }
        }
    }
}
