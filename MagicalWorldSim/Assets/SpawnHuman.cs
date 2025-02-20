using UnityEngine;
using UnityEngine.UI;

public class SpawnHuman : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn; // Drag your prefab here in the inspector
    [SerializeField] private Button spawnButton; // Reference to the UI Button

    private bool isSpawningEnabled = false; // Track if spawning is enabled

    private void Start()
    {
        // Add a listener to the button
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(ToggleSpawning);
        }
    }

    private void Update()
    {
        // Check for mouse click and spawning enabled
        if (isSpawningEnabled && Input.GetMouseButtonDown(0))
        {
            SpawnPrefabAtMousePosition();
        }
    }

    private void ToggleSpawning()
    {
        // Toggle the spawning state
        isSpawningEnabled = !isSpawningEnabled;
    }

    private void SpawnPrefabAtMousePosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0; // Ensure the prefab spawns on the correct Z level for 2D

        // Check if mouse is over a 2D collider
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);
        if (hitCollider != null)
        {
            // Instantiate the prefab
            GameObject newHuman = Instantiate(prefabToSpawn, worldPosition, Quaternion.identity);

            // Find the "Humans" GameObject to set as parent
            GameObject humansParent = GameObject.Find("Humans");
            if (humansParent != null)
            {
                // Set the parent of the instantiated object to the "Humans" GameObject
                newHuman.transform.SetParent(humansParent.transform);
            }
        }
    }
}
