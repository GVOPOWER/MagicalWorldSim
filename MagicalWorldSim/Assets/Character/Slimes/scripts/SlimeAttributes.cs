using UnityEngine;

[System.Serializable]
public class SlimeAttributes
{
    public string characterName;
    public float currentHunger;
    public float maxHunger = 80f;
    public float hungerDecreaseRate = 0.5f;
    public float currentHp;
    public float maxHp = 80f;
    public float currentAge;
    public float ageIncrementRate = 1f / 60f; // Assuming similar to CharacterAttributes
    public float maxAge = 50f;

    public void InitializeAttributes()
    {
        currentHunger = maxHunger;
        currentHp = maxHp;
        currentAge = 0f;
        characterName = GenerateSlimeName();
    }

    public void HandleHunger(float deltaTime)
    {
        currentHunger -= hungerDecreaseRate * deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        if (currentHunger <= 0)
        {
            TakeDamage(hungerDecreaseRate * deltaTime);
        }
    }

    public void IncrementAge(float deltaTime)
    {
        currentAge += ageIncrementRate * deltaTime;
        if (currentAge > maxAge)
        {
            currentHp = 0; // The slime dies when it surpasses its max age
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        if (currentHp <= 0)
        {
            Debug.Log($"{characterName} has died.");
            // Additional logic for when the slime dies, such as notifying a game manager
        }
    }

    public static string GenerateSlimeName()
    {
        string[] prefixes = { "Goopy", "Blob", "Gel", "Slippy", "Squish" };
        string[] suffixes = { "ster", "ball", "oooze", "drop", "mash" };
        return $"{prefixes[Random.Range(0, prefixes.Length)]} {suffixes[Random.Range(0, suffixes.Length)]}";
    }
}
