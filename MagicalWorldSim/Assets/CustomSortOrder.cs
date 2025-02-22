using UnityEngine;
using UnityEngine.Tilemaps;

public class CustomSortOrder : MonoBehaviour
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;

    private void Awake()
    {
        // Get the Tilemap component
        tilemap = GetComponent<Tilemap>();

        // Get the TilemapRenderer component
        tilemapRenderer = GetComponent<TilemapRenderer>();
    }

    private void Update()
    {
        if (tilemapRenderer != null)
        {
            // Adjust the sorting order based on the Y position of the Tilemap
            tilemapRenderer.sortingOrder = -(int)(transform.position.y * 100);
        }
        else
        {
            Debug.LogWarning("TilemapRenderer component is missing on the Tilemap GameObject.");
        }
    }
}
