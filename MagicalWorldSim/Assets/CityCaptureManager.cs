using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CityCaptureManager : MonoBehaviour
{
    public Tilemap cityCaptureTilemap;
    public Sprite cityOverlaySprite;
    public float borderExpansionInterval = 5f;
    public int maxExpansionRadius = 10;

    private List<City> cities = new List<City>();

    private void Start()
    {
        if (cityOverlaySprite == null)
        {
            Debug.LogWarning("City overlay sprite is not set.");
        }
    }

    public void CreateVillage(Vector3Int position)
    {
        if (cityOverlaySprite == null)
        {
            Debug.LogWarning("City overlay sprite is not set.");
            return;
        }

        string cityName = GenerateRandomCityName();
        int maxPopulation = 100;
        int maxHouses = 20;

        if (cities.Exists(c => c.position == position))
        {
            Debug.LogWarning("A city already exists at this position.");
            return;
        }

        City newCity = new City(cityName, position, maxPopulation, maxHouses);
        cities.Add(newCity);

        ExpandBorders(newCity);
        Debug.Log($"City created: {newCity.cityName} at {position}");
    }

    private void Update()
    {
        foreach (var city in cities)
        {
            if (Time.time >= city.position.y + borderExpansionInterval)
            {
                ExpandBorders(city);
            }
        }
    }

    private void ExpandBorders(City city)
    {
        int currentRadius = Mathf.Min(maxExpansionRadius, (int)((Time.time - city.position.y) / borderExpansionInterval));

        for (int x = -currentRadius; x <= currentRadius; x++)
        {
            for (int y = -currentRadius; y <= currentRadius; y++)
            {
                Vector3Int position = city.position + new Vector3Int(x, y, 0);
                if (cityCaptureTilemap.GetTile(position) == null)
                {
                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = cityOverlaySprite;
                    cityCaptureTilemap.SetTile(position, tile);
                }
            }
        }
    }

    public string GenerateRandomCityName()
    {
        string[] prefixes = { "New", "Old", "North", "South", "East", "West", "Fort", "Port", "Lake", "River" };
        string[] suffixes = { "ton", "ville", "burg", "field", "land", "ford", "bridge", "dale", "haven", "view" };

        string prefix = prefixes[Random.Range(0, prefixes.Length)];
        string suffix = suffixes[Random.Range(0, suffixes.Length)];

        return $"{prefix}{suffix}";
    }

    public void AddPopulationToCity(string cityName, int amount)
    {
        City city = cities.Find(c => c.cityName.Equals(cityName, System.StringComparison.OrdinalIgnoreCase));
        if (city != null)
        {
            city.AddPopulation(amount);
            Debug.Log($"{amount} population added to {cityName}. Current population: {city.currentPopulation}");
        }
        else
        {
            Debug.LogWarning($"City {cityName} not found.");
        }
    }

    public void BuildHouseInCity(string cityName)
    {
        City city = cities.Find(c => c.cityName.Equals(cityName, System.StringComparison.OrdinalIgnoreCase));
        if (city != null)
        {
            city.AddHouse();
            Debug.Log($"House built in {cityName}. Current houses: {city.currentHouses}");
        }
        else
        {
            Debug.LogWarning($"City {cityName} not found.");
        }
    }
}
