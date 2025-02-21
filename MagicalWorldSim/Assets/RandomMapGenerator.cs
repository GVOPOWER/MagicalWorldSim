using UnityEngine;
using UnityEngine.Tilemaps;

public class LandGenerator : MonoBehaviour
{
    public Tilemap tilemap;
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

    public Camera mainCamera;
    public float zoomThreshold = 20f; // Threshold for camera size

    private Vector2[] landCenters;

    void Start()
    {
        GenerateLandCenters();
        GenerateLands();
    }

    void Update()
    {
        if (mainCamera.orthographicSize > zoomThreshold)
        {
            UseUnzoomTiles();
        }
        else
        {
            UseDetailedTiles();
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

    void GenerateLands()
    {
        float adjustedLandSizeFactor = baseLandSizeFactor * (3.0f / numberOfLandmasses);

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
                        for (int j = 0; j < numberOfIslands; j++)
                        {
                            float xCoord = (x + center.x + j * 10) * noiseScale;
                            float yCoord = (y + center.y + j * 10) * noiseScale;
                            float sample = Mathf.PerlinNoise(xCoord, yCoord);
                            float height = Mathf.Max(0, 1 - (distance / adjustedLandSizeFactor)) * sample;
                            maxHeight = Mathf.Max(maxHeight, height);
                        }
                    }
                }

                TileBase selectedTile = SelectTileBasedOnHeight(maxHeight);
                tilemap.SetTile(new Vector3Int(x, y, 0), selectedTile);
            }
        }
    }

    void UseUnzoomTiles()
    {
        tilemap.SwapTile(waterDeepTile, waterDeepTileUnzoom);
        tilemap.SwapTile(waterTile, waterTileUnzoom);
        tilemap.SwapTile(waterUndeepTile, waterUndeepTileUnzoom);
        tilemap.SwapTile(sandTile, sandTileUnzoom);
        tilemap.SwapTile(grassTile, grassTileUnzoom);
        tilemap.SwapTile(forestGrassTile, forestGrassTileUnzoom);
        tilemap.SwapTile(mountainLowTile, mountainLowTileUnzoom);
        tilemap.SwapTile(mountainHighTile, mountainHighTileUnzoom);
    }

    void UseDetailedTiles()
    {
        tilemap.SwapTile(waterDeepTileUnzoom, waterDeepTile);
        tilemap.SwapTile(waterTileUnzoom, waterTile);
        tilemap.SwapTile(waterUndeepTileUnzoom, waterUndeepTile);
        tilemap.SwapTile(sandTileUnzoom, sandTile);
        tilemap.SwapTile(grassTileUnzoom, grassTile);
        tilemap.SwapTile(forestGrassTileUnzoom, forestGrassTile);
        tilemap.SwapTile(mountainLowTileUnzoom, mountainLowTile);
        tilemap.SwapTile(mountainHighTileUnzoom, mountainHighTile);
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
