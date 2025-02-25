using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityCreation : MonoBehaviour
{
    public GameObject cityPrefab; // Prefab representing the city
    public Text cityNameTextPrefab; // Prefab for displaying the city name

    private static readonly List<string> cityNames = new List<string>
    {
        "New Haven", "Eldoria", "Rivermouth", "Brightvale", "Silverwood",
        "Stormwatch", "Ironhold", "Suncrest", "Duskwood", "Frosthaven"
    };

    // Method to create a city at a specified position
    public void CreateCity(Vector3 position)
    {
        // Instantiate the city
        GameObject city = Instantiate(cityPrefab, position, Quaternion.identity);

        // Generate a random city name
        string cityName = GenerateRandomCityName();

        // Create a UI Text object for the city name
        Text cityNameText = Instantiate(cityNameTextPrefab, position + Vector3.up, Quaternion.identity);
        cityNameText.text = cityName;

        // Set the name of the city object for easier identification
        city.name = cityName;

        // Optionally adjust the position of the city name text
        RectTransform rectTransform = cityNameText.GetComponent<RectTransform>();
        rectTransform.position = Camera.main.WorldToScreenPoint(position + Vector3.up);

        // Optional: Set the current city name in the RandomWalker
        RandomWalker walker = GetComponent<RandomWalker>();
        if (walker != null)
        {
            walker.attributes.characterName = cityName; // Set the city name for the walker
        }
    }

    // Method to generate a random city name
    public string GenerateRandomCityName()
    {
        return cityNames[Random.Range(0, cityNames.Count)];
    }
}
