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
    public float ageIncrementRate = 1f / 60f;
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
            // Assuming SlimeWalker will handle knockback
            currentHp -= hungerDecreaseRate * deltaTime;
        }
    }

    public void IncrementAge(float deltaTime)
    {
        currentAge += ageIncrementRate * deltaTime;
        if (currentAge > maxAge)
        {
            currentHp = 0;
        }
    }

    public void TakeDamage(float damage, Vector3 damageSourcePosition, SlimeWalker slimeWalker)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        // Notify SlimeWalker to apply knockback
        slimeWalker.ApplyKnockback(damageSourcePosition);

        if (currentHp <= 0)
        {
            Debug.Log($"{characterName} has died.");
        }
    }

    public static string GenerateSlimeName()
    {
        string[] prefixes = {
    "Goopy", "Blob", "Gel", "Slippy", "Squish", "Gloop", "Sludge", "Mucus", "Ooze", "Slimer",
    "Drip", "Glop", "Goo", "Splurt", "Dribble", "Sloshy", "Snail", "Viscous", "Sappy", "Glum",
    "Slick", "Slosher", "Mushy", "Slurry", "Slobber", "Gummy", "Slob", "Squidge", "Glaze", "Muddle",
    "Wobble", "Flubber", "Squeegee", "Blubber", "Plop", "Squirtle", "Blobfish", "Jiggly", "Booger", "Wiggly"
};

        string[] suffixes = {
    "ster", "ball", "oooze", "drop", "mash", "blob", "drip", "glob", "muck", "slosh",
    "smooze", "puddle", "drizzle", "slick", "splash", "gum", "squirt", "trail", "slime", "slip",
    "jelly", "melt", "squash", "gush", "trickle", "leak", "wet", "sog", "brew", "goo",
    "zilla", "pants", "meister", "puff", "whiz", "giggle", "blobber", "snicker", "snort", "splatter"
};

        return $"{prefixes[Random.Range(0, prefixes.Length)]} {suffixes[Random.Range(0, suffixes.Length)]}";
    }
}
