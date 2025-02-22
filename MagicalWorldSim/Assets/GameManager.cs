using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject characterPrefab; // Reference to the character prefab
    public GameObject cityCreationPrefab; // Reference to the CityCreation prefab

    private CityCreation cityCreationInstance; // Instance of the CityCreation

    void Start()
    {
        // Instantiate the CityCreation if necessary
        GameObject cityCreationObject = Instantiate(cityCreationPrefab);
        cityCreationInstance = cityCreationObject.GetComponent<CityCreation>();

        // Now instantiate the character
        GameObject characterInstance = Instantiate(characterPrefab, Vector3.zero, Quaternion.identity);

        // Access the RandomWalker component and assign the cityCreation reference
        RandomWalker walker = characterInstance.GetComponent<RandomWalker>();
        if (walker != null)
        {
            walker.cityCreation = cityCreationInstance; // Set the reference
        }
    }
}
