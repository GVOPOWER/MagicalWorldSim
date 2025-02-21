using System.Collections.Generic; // Add this line
using UnityEngine;
using UnityEngine.UI;

public class CityCreation : MonoBehaviour
{
    public GameObject cityPrefab; // Prefab representing the city
    public Text cityNameTextPrefab; // Prefab for displaying the city name
    public float creationAreaWidth = 20f; // Width of the area where cities can be created
    public float creationAreaHeight = 20f; // Height of the area where cities can be created
    public float cityCreationInterval = 5f; // Time between city creations

    private static readonly List<string> cityNames = new List<string>
    {
        "New Haven", "Eldoria", "Rivermouth", "Brightvale", "Silverwood",
        "Stormwatch", "Ironhold", "Suncrest", "Duskwood", "Frosthaven"
    };

    private void Start()
    {
        // Start the city creation process
        InvokeRepeating("CreateCity", 0f, cityCreationInterval);
    }

    // Method to create a city at a random position
    private void CreateCity()
    {
        // Generate a random position within the specified area
        Vector3 position = new Vector3(
            Random.Range(-creationAreaWidth / 2, creationAreaWidth / 2),
            0, // Assuming the ground is at y = 0
            Random.Range(-creationAreaHeight / 2, creationAreaHeight / 2)
        );

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
    }

    // Method to generate a random city name
    private string GenerateRandomCityName()
    {
        return cityNames[Random.Range(0, cityNames.Count)];
    }
}
