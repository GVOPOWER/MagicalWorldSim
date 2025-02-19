using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class BushSpawner2D : MonoBehaviour
{
    public GameObject bushPrefab;  // Assign your bush prefab in the Inspector
    public float spawnInterval = 5.0f; // Time interval in seconds between spawns
    public int bushesPerInterval = 5;  // Number of bushes to spawn per interval

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
        for (int i = 0; i < numberOfBushes; i++)
        {
            Vector3 randomPosition = GetRandomPosition();
            Instantiate(bushPrefab, randomPosition, Quaternion.identity);
        }
    }

    private Vector3 GetRandomPosition()
    {
        // Calculate a random position within the bounds of the tilemap
        BoundsInt cellBounds = tilemap.cellBounds;
        Vector3Int randomCellPosition = new Vector3Int(
            Random.Range(cellBounds.xMin, cellBounds.xMax),
            Random.Range(cellBounds.yMin, cellBounds.yMax),
            0
        );

        // Convert the cell position to world position
        Vector3 randomWorldPosition = tilemap.CellToWorld(randomCellPosition) + tilemap.tileAnchor;

        return randomWorldPosition;
    }
}
