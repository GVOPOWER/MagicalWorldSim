using UnityEngine;

[System.Serializable]
public class City
{
    public string cityName;
    public Vector3Int position;
    public int currentPopulation;
    public int maxPopulation;
    public int currentHouses;
    public int maxHouses;

    public City(string name, Vector3Int pos, int maxPop, int maxHouses)
    {
        cityName = name;
        position = pos;
        maxPopulation = maxPop;
        this.maxHouses = maxHouses;
        currentPopulation = 0;
        currentHouses = 0;
    }

    public void AddPopulation(int amount)
    {
        currentPopulation = Mathf.Clamp(currentPopulation + amount, 0, maxPopulation);
    }

    public void AddHouse()
    {
        if (currentHouses < maxHouses)
        {
            currentHouses++;
        }
        else
        {
            Debug.LogWarning($"{cityName} has reached maximum house capacity.");
        }
    }

    public bool CanExpand()
    {
        return currentHouses < maxHouses && currentPopulation < maxPopulation;
    }
}
