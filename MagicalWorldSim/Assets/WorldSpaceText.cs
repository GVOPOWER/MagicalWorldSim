using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class WorldSpaceText : MonoBehaviour
{
    public GameObject textPrefab; // Assign this in the Inspector with your TextMeshPro prefab
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset to position the text directly underneath the character
    public float ageRequirement = 16f; // Age requirement to create a city
    public RandomWalker character; // Reference to the RandomWalker script to get current city and age

    private TextMeshPro characterText; // Changed to TextMeshPro type
    private bool needsUpdate = true; // Flag to track if the text needs an update

    private void Start()
    {
        // Instantiate the text prefab at the character's position with the offset
        GameObject textObj = Instantiate(textPrefab, transform.position + offset, Quaternion.identity);

        // Set the parent of the text object to the World Space Canvas
        Canvas worldSpaceCanvas = FindWorldSpaceCanvas();
        if (worldSpaceCanvas != null)
        {
            textObj.transform.SetParent(worldSpaceCanvas.transform, false);
        }
        else
        {
            Debug.LogError("No World Space Canvas found in the scene.");
            return;
        }

        characterText = textObj.GetComponent<TextMeshPro>(); // Get the TextMeshPro component

        // Set the initial text to display
        UpdateCharacterText();
    }

    private Canvas FindWorldSpaceCanvas()
    {
        // First, try to find a Canvas of type Canvas
        Canvas canvas = FindObjectOfType<Canvas>();

        // If a canvas is found, check if its name matches "canvas2"
        if (canvas != null && canvas.name == "canvas2")
        {
            return canvas;
        }

        // If no canvas of name "canvas2" is found, try finding all canvases
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "canvas2")
            {
                return c;
            }
        }

        // If no matching canvas is found, return null
        return null;
    }

    private void UpdateCharacterText()
    {
        // Ensure the character reference is valid
        if (character != null)
        {
            // Set the text to display the city name based on the character's age and current city
            if (character.attributes.currentAge >= ageRequirement)
            {
                characterText.text = !string.IsNullOrEmpty(character.attributes.characterName) ? character.attributes.characterName : "No City";
            }
            else
            {
                characterText.text = "Too Young";
            }

            // Set the flag to false after updating
            needsUpdate = false;
        }
        else
        {
            Debug.LogWarning("Character reference is not set. Please assign the RandomWalker character.");
        }
    }

    private void LateUpdate()
    {
        // Update the text position to keep it above the character
        if (character != null)
        {
            characterText.transform.position = character.transform.position + offset;
        }
    }

    private void Update()
    {
        // Check if we need to update the text if the character's age or city has changed
        if (needsUpdate)
        {
            UpdateCharacterText();
        }
    }

    // Call this method to trigger an update manually
    public void TriggerUpdate()
    {
        needsUpdate = true;
    }
}
