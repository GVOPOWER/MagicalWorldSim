using UnityEngine;

[System.Serializable]
public class CharacterAttributes
{
    public string characterName;
    public float currentHunger;
    public float maxHunger = 100f;
    public float hungerDecreaseRate = 1f;
    public float currentHp;
    public float maxHp = 100f;
    public float currentAge;
    public float ageIncrementRate = 1f / 60f;
    public float minReproductiveAge = 1f;
    public float maxReproductiveAge = 50f;
    public float maxAge = 100f;
    public float children;
    public float maxChildren = 5;
    public float childCreationCooldown = 10f;
    private float lastChildCreationTime = -Mathf.Infinity;
    public string currentCity = "";

    public void InitializeAttributes()
    {
        currentHunger = maxHunger;
        currentHp = maxHp;
        currentAge = 0f;
        characterName = GenerateRandomFantasyName();
    }

    public bool CanCreateChild(float currentTime)
    {
        bool isWithinReproductiveAge = currentAge >= minReproductiveAge && currentAge <= maxReproductiveAge && children < maxChildren;
        return isWithinReproductiveAge && currentTime >= lastChildCreationTime + childCreationCooldown;
    }

    public void IncrementAge(float deltaTime)
    {
        currentAge += ageIncrementRate * deltaTime;
    }

    public void HandleHunger(float deltaTime)
    {
        currentHunger -= hungerDecreaseRate * deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        if (currentHunger <= 0)
        {
            currentHp -= hungerDecreaseRate * deltaTime;
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        }
    }

    public static string GenerateRandomFantasyName()
    {
        string firstName = new[] { "Arin", "Borin", "Celdor", "Durnan", "Elandor", "Faelan", "Gorim", "Haldir", "Ithil", "Jareth" }[Random.Range(0, 10)];
        string lastName = new[] { "Stormwind", "Ironfist", "Moonshadow", "Duskbringer", "Starlight", "Thunderstrike", "Silverleaf", "Shadowbane", "Brightstar", "Nightwhisper" }[Random.Range(0, 10)];
        return $"{firstName} {lastName}";
    }

    public void RecordChildCreation(float currentTime)
    {
        lastChildCreationTime = currentTime;
        children += 1;
    }
}
