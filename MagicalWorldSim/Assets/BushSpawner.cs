using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class BushSpawner2D : MonoBehaviour
{
    public GameObject bushPrefab;  // Assign your bush prefab in the Inspector
    public float spawnInterval = 5.0f; // Time interval in seconds between spawns
    public int bushesPerInterval = 5;  // Number of bushes to spawn per interval
    public TileBase[] spawnableTiles; // Array of tiles on which bushes can spawn

    private Tilemap tilemap;

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError("No Tilemap component found. Please attach the script to a GameObject with a Tilemap.");
            return;
        }

        StartCoroutine(SpawnBushesContinuously());
    }

    private IEnumerator SpawnBushesContinuously()
    {
        while (true)
        {
            SpawnBushes(bushesPerInterval);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBushes(int numberOfBushes)
    {
        // Find the "Bushes" GameObject to set as parent
        GameObject bushParent = GameObject.Find("Bushes");
        if (bushParent == null)
        {
            Debug.LogWarning("No GameObject named 'Bushes' found in the scene.");
            return;
        }

        for (int i = 0; i < numberOfBushes; i++)
        {
            Vector3? spawnPosition = GetValidRandomPosition();
            if (spawnPosition.HasValue)
            {
                // Instantiate the bush and set its parent to the bushParent
                GameObject bush = Instantiate(bushPrefab, spawnPosition.Value, Quaternion.identity);
                bush.transform.SetParent(bushParent.transform);
            }
        }
    }

    private Vector3? GetValidRandomPosition()
    {
        // Calculate a random position within the bounds of the tilemap
        BoundsInt cellBounds = tilemap.cellBounds;

        for (int attempt = 0; attempt < 100; attempt++) // Limit attempts to prevent infinite loops
        {
            Vector3Int randomCellPosition = new Vector3Int(
                Random.Range(cellBounds.xMin, cellBounds.xMax),
                Random.Range(cellBounds.yMin, cellBounds.yMax),
                0
            );

            // Check if the tile at the random position is in the allowed tiles array
            TileBase tileAtPosition = tilemap.GetTile(randomCellPosition);

            if (System.Array.Exists(spawnableTiles, tile => tile == tileAtPosition))
            {
                // Convert the cell position to world position
                Vector3 randomWorldPosition = tilemap.CellToWorld(randomCellPosition) + tilemap.tileAnchor;
                return randomWorldPosition;
            }
        }

        return null; // Return null if no valid position is found
    }
}
