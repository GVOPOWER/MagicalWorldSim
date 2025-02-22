using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandGenerator : MonoBehaviour
{
    // Tilemap references for different tile types
    public Tilemap waterDeepTilemap;
    public Tilemap waterTilemap;
    public Tilemap waterUndeepTilemap;
    public Tilemap sandTilemap;
    public Tilemap grassTilemap;
    public Tilemap forestTilemap;
    public Tilemap mountainLowTilemap;
    public Tilemap mountainHighTilemap;

    // Tiles for detailed and unzoomed states
    public TileBase waterDeepTile;
    public TileBase waterTile;
    public TileBase waterUndeepTile;
    public TileBase sandTile;
    public TileBase grassTile;
    public TileBase forestGrassTile;
    public TileBase mountainLowTile;
    public TileBase mountainHighTile;

    public TileBase waterDeepTileUnzoom;
    public TileBase waterTileUnzoom;
    public TileBase waterUndeepTileUnzoom;
    public TileBase sandTileUnzoom;
    public TileBase grassTileUnzoom;
    public TileBase forestGrassTileUnzoom;
    public TileBase mountainLowTileUnzoom;
    public TileBase mountainHighTileUnzoom;

    public int mapWidth = 100;
    public int mapHeight = 100;
    public float noiseScale = 0.1f;
    public int numberOfLandmasses = 3;
    public int numberOfIslands = 5;
    public float baseLandSizeFactor = 30f;
    public float baseIslandSizeFactor = 10f;
    public float mountainBias = 0.2f;

    public Camera mainCamera;
    public float zoomThreshold = 20f;

    private Vector2[] landCenters;
    private Vector2[] islandCenters;
    private bool isZoomedIn = false;
    private int batchSize;

    void Start()
    {
        GenerateLandCenters();
        GenerateIslandCenters();
        GenerateLands();
        batchSize = mapHeight * mapWidth / 2 * 10;
    }

    void Update()
    {
        bool shouldZoomOut = mainCamera.orthographicSize > zoomThreshold;
        if (shouldZoomOut != isZoomedIn)
        {
            isZoomedIn = shouldZoomOut;
            Debug.Log("Zoom state changed: " + (isZoomedIn ? "Zoomed Out" : "Zoomed In"));
            StartCoroutine(UpdateTilesInZoomState());
        }
    }

    IEnumerator UpdateTilesInZoomState()
    {
        Debug.Log("Updating tiles due to zoom change.");

        List<Vector3Int> updatedTiles = new List<Vector3Int>();

        // Gather tiles to update
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase tileToUpdate = isZoomedIn ? GetTileFromZoomedMaps(position) : GetTileFromDetailedMaps(position);

                if (tileToUpdate != null)
                {
                    updatedTiles.Add(position);
                }
            }
        }

        // Apply updates in batches using the predefined batch size
        for (int i = 0; i < updatedTiles.Count; i++)
        {
            Vector3Int position = updatedTiles[i];
            TileBase tileToUpdate = isZoomedIn ? GetTileFromZoomedMaps(position) : GetTileFromDetailedMaps(position);
            SetTileInCorrespondingMap(tileToUpdate, position);

            // Yield based on the defined batch size
            if (i > 0 && i % batchSize == 0)
            {
                yield return null; // Yield to allow other processes to run
            }
        }

        // Final batch update if there are remaining tiles
        if (updatedTiles.Count % batchSize != 0)
        {
            yield return null; // Ensure the final batch gets processed
        }
    }




    TileBase GetTileFromZoomedMaps(Vector3Int position)
    {
        // Check each Tilemap for the tile at the given position
        if (mountainHighTilemap.GetTile(position) != null)
            return mountainHighTileUnzoom;
        if (mountainLowTilemap.GetTile(position) != null)
            return mountainLowTileUnzoom;
        if (waterDeepTilemap.GetTile(position) != null)
            return waterDeepTileUnzoom;
        if (waterTilemap.GetTile(position) != null)
            return waterTileUnzoom;
        if (waterUndeepTilemap.GetTile(position) != null)
            return waterUndeepTileUnzoom;
        if (sandTilemap.GetTile(position) != null)
            return sandTileUnzoom;
        if (grassTilemap.GetTile(position) != null)
            return grassTileUnzoom;
        if (forestTilemap.GetTile(position) != null)
            return forestGrassTileUnzoom;

        return null;
    }

    TileBase GetTileFromDetailedMaps(Vector3Int position)
    {
        // Check each Tilemap for the tile at the given position
        if (mountainHighTilemap.GetTile(position) != null)
            return mountainHighTile;
        if (mountainLowTilemap.GetTile(position) != null)
            return mountainLowTile;
        if (waterDeepTilemap.GetTile(position) != null)
            return waterDeepTile;
        if (waterTilemap.GetTile(position) != null)
            return waterTile;
        if (waterUndeepTilemap.GetTile(position) != null)
            return waterUndeepTile;
        if (sandTilemap.GetTile(position) != null)
            return sandTile;
        if (grassTilemap.GetTile(position) != null)
            return grassTile;
        if (forestTilemap.GetTile(position) != null)
            return forestGrassTile;

        return null;
    }

    void SetTileInCorrespondingMap(TileBase tileToUpdate, Vector3Int position)
    {
        // Determine which Tilemap to set the tile in based on the zoom state
        if (tileToUpdate == waterDeepTileUnzoom || tileToUpdate == waterDeepTile)
        {
            waterDeepTilemap.SetTile(position, tileToUpdate);
        }
        else if (tileToUpdate == waterTileUnzoom || tileToUpdate == waterTile)
        {
            waterTilemap.SetTile(position, tileToUpdate);
        }
        else if (tileToUpdate == waterUndeepTileUnzoom || tileToUpdate == waterUndeepTile)
        {
            waterUndeepTilemap.SetTile(position, tileToUpdate);
        }
        else if (tileToUpdate == sandTileUnzoom || tileToUpdate == sandTile)
        {
            sandTilemap.SetTile(position, tileToUpdate);
        }
        else if (tileToUpdate == grassTileUnzoom || tileToUpdate == grassTile)
        {
            grassTilemap.SetTile(position, tileToUpdate);
        }
        else if (tileToUpdate == forestGrassTileUnzoom || tileToUpdate == forestGrassTile)
        {
            forestTilemap.SetTile(position, tileToUpdate);
        }
        else if (tileToUpdate == mountainLowTileUnzoom || tileToUpdate == mountainLowTile)
        {
            mountainLowTilemap.SetTile(position, tileToUpdate);
        }
        else if (tileToUpdate == mountainHighTileUnzoom || tileToUpdate == mountainHighTile)
        {
            mountainHighTilemap.SetTile(position, tileToUpdate);
        }
    }

    void GenerateLandCenters()
    {
        landCenters = new Vector2[numberOfLandmasses];
        for (int i = 0; i < numberOfLandmasses; i++)
        {
            float x = Random.Range(0, mapWidth);
            float y = Random.Range(0, mapHeight);
            landCenters[i] = new Vector2(x, y);
        }
    }

    void GenerateIslandCenters()
    {
        islandCenters = new Vector2[numberOfIslands];
        for (int i = 0; i < numberOfIslands; i++)
        {
            float x = Random.Range(0, mapWidth);
            float y = Random.Range(0, mapHeight);
            islandCenters[i] = new Vector2(x, y);
        }
    }

    void GenerateLands()
    {
        float adjustedLandSizeFactor = baseLandSizeFactor * (3.0f / numberOfLandmasses);
        float adjustedIslandSizeFactor = baseIslandSizeFactor * (3.0f / numberOfIslands);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float maxHeight = 0f;
                foreach (var center in landCenters)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance < adjustedLandSizeFactor)
                    {
                        float sample = SamplePerlinNoise(x, y, center);
                        float height = Mathf.Max(0, 1 - (distance / adjustedLandSizeFactor)) * sample;
                        maxHeight = Mathf.Max(maxHeight, height);
                    }
                }

                foreach (var center in islandCenters)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance < adjustedIslandSizeFactor)
                    {
                        float sample = SamplePerlinNoise(x, y, center);
                        float height = Mathf.Max(0, 1 - (distance / adjustedIslandSizeFactor)) * sample;
                        maxHeight = Mathf.Max(maxHeight, height);
                    }
                }

                // Adjust height with mountain bias
                float adjustedHeight = maxHeight + mountainBias;

                // Select the appropriate tile based on height and assign to the correct Tilemap
                AssignTileBasedOnHeight(adjustedHeight, x, y);
            }
        }
    }

    float SamplePerlinNoise(int x, int y, Vector2 center)
    {
        float xCoord = (x + center.x) * noiseScale;
        float yCoord = (y + center.y) * noiseScale;
        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    void AssignTileBasedOnHeight(float height, int x, int y)
    {
        TileBase selectedTile = SelectTileBasedOnHeight(height);
        Vector3Int position = new Vector3Int(x, y, 0);

        // Set the tile in the corresponding Tilemap based on the selected tile type
        if (selectedTile == waterDeepTile)
        {
            waterDeepTilemap.SetTile(position, selectedTile);
        }
        else if (selectedTile == waterTile)
        {
            waterTilemap.SetTile(position, selectedTile);
        }
        else if (selectedTile == waterUndeepTile)
        {
            waterUndeepTilemap.SetTile(position, selectedTile);
        }
        else if (selectedTile == sandTile)
        {
            sandTilemap.SetTile(position, selectedTile);
        }
        else if (selectedTile == grassTile)
        {
            grassTilemap.SetTile(position, selectedTile);
        }
        else if (selectedTile == forestGrassTile)
        {
            forestTilemap.SetTile(position, selectedTile);
        }
        else if (selectedTile == mountainLowTile)
        {
            mountainLowTilemap.SetTile(position, selectedTile);
        }
        else if (selectedTile == mountainHighTile)
        {
            mountainHighTilemap.SetTile(position, selectedTile);
        }
    }

    TileBase SelectTileBasedOnHeight(float height)
    {
        if (height < 0.2f)
            return waterDeepTile;
        else if (height < 0.4f)
            return waterTile;
        else if (height < 0.5f)
            return waterUndeepTile;
        else if (height < 0.55f)
            return sandTile;
        else if (height < 0.65f)
            return grassTile;
        else if (height < 0.75f)
            return forestGrassTile;
        else if (height < 0.85f)
            return mountainLowTile;
        else
            return mountainHighTile;
    }
}
