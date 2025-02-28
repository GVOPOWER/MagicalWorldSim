using UnityEngine;

public class Bush : MonoBehaviour
{
    public float foodAmount = 5f; // Amount of food the bush provides
    public int maxUses = 5; // Maximum times the bush can be consumed
    private int currentUses = 0; // Track current uses

    public float hungerThresholdPercentage = 0.75f; // 75% threshold
    public float separationDuration = 1f; // Duration to move away after eating

    public float growthFactor = 0.25f; // Amount by which the slime grows each time it eats

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            RandomWalker walker = collision.GetComponent<RandomWalker>();
            if (walker != null && walker.attributes.currentHunger < walker.attributes.maxHunger * hungerThresholdPercentage)
            {
                Consume(walker);
            }
        }
        else if (collision.CompareTag("Slime")) // Check for Slime tag
        {
            SlimeWalker slime = collision.GetComponent<SlimeWalker>();
            if (slime != null && slime.attributes.currentHunger < slime.attributes.maxHunger * hungerThresholdPercentage)
            {
                Consume(slime);
            }
        }
    }

    private void Consume(RandomWalker walker)
    {
        if (currentUses < maxUses)
        {
            walker.attributes.currentHunger += foodAmount;
            walker.attributes.currentHunger = Mathf.Clamp(walker.attributes.currentHunger, 0, walker.attributes.maxHunger);
            Debug.Log($"{walker.attributes.characterName} ate and gained hunger.");

            currentUses++;

            walker.TriggerSeparation(separationDuration);

            if (currentUses >= maxUses)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Consume(SlimeWalker slime)
    {
        if (currentUses < maxUses)
        {
            slime.attributes.currentHunger += foodAmount;
            slime.attributes.currentHunger = Mathf.Clamp(slime.attributes.currentHunger, 0, slime.attributes.maxHunger);
            Debug.Log($"{slime.attributes.characterName} consumed and gained hunger.");

            // Grow the slime
            slime.transform.localScale += new Vector3(growthFactor, growthFactor, 0);

            currentUses++;

            slime.TriggerSeparation(separationDuration);

            if (currentUses >= maxUses)
            {
                Destroy(gameObject);
            }
        }
    }
}
