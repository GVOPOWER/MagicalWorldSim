using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelHumans : MonoBehaviour
{
    public int level = 1;
    public int currentXp = 0;
    public int XpNeededForNextLevel = 10;
    public string XpProgress = "0/10 XP";

    public int HpAddPerLevel = 10;
    public int DamageAddPerLevel = 5;
    public float SpeedAddPerLevel = 0.2f;
    public float VisionAddPerLevel = 0.5f;

    private RandomWalker randomWalker;

    private void Start()
    {
        randomWalker = GetComponent<RandomWalker>();
        if (randomWalker == null)
        {
            Debug.LogError("RandomWalker component not found!");
            enabled = false;
            return;
        }

        UpdateXpProgressUI();
    }

    private void UpdateXpProgressUI()
    {
        XpProgress = $"{currentXp}/{XpNeededForNextLevel} XP";
        Debug.Log(XpProgress);
    }

    public void GainXp(int amount)
    {
        currentXp += amount;
        CheckLevelUp();
        UpdateXpProgressUI();
    }

    private void CheckLevelUp()
    {
        while (currentXp >= XpNeededForNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        currentXp -= XpNeededForNextLevel;
        XpNeededForNextLevel += 5; // Increment XP requirement for next level

        // Update character attributes
        randomWalker.attributes.maxHp += HpAddPerLevel;
        randomWalker.moveSpeed += SpeedAddPerLevel;
        randomWalker.visionRange += VisionAddPerLevel;

        randomWalker.attributes.currentHp = randomWalker.attributes.maxHp; // Heal to full HP on level-up

        Debug.Log($"Level Up! New Level: {level}, Max HP: {randomWalker.attributes.maxHp}, Speed: {randomWalker.moveSpeed}, Vision: {randomWalker.visionRange}");
    }
}
