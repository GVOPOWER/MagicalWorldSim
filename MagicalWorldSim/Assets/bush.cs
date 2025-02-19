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
            if (walker != null && walker.currentHunger < walker.maxHunger * hungerThresholdPercentage)
            {
                Consume(walker);
            }
        }
    }

    private void Consume(RandomWalker walker)
    {
        if (currentUses < maxUses)
        {
            walker.Eat(foodAmount); // Use the Eat method from RandomWalker
            currentUses++;
            Debug.Log("Bush consumed by player! Providing " + foodAmount + " food. Uses left: " + (maxUses - currentUses));

            // Trigger separation after eating
            walker.TriggerSeparation(separationDuration);

            if (currentUses >= maxUses)
            {
                Debug.Log("Bush is depleted and will be destroyed.");
                Destroy(gameObject); // Destroy the bush after max uses
            }
        }
    }
}
