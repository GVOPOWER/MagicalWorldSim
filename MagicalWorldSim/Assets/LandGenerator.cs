using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandGenerator : MonoBehaviour
{
    // Tilemap references for detailed and unzoomed tiles
    public Tilemap waterDeepTilemap;
    public Tilemap waterTilemap;
    public Tilemap waterUndeepTilemap;
    public Tilemap sandTilemap;
    public Tilemap grassTilemap;
    public Tilemap forestTilemap;
    public Tilemap mountainLowTilemap;
    public Tilemap mountainHighTilemap;

    public Tilemap waterDeepTilemapUnzoom;
    public Tilemap waterTilemapUnzoom;
    public Tilemap waterUndeepTilemapUnzoom;
    public Tilemap sandTilemapUnzoom;
    public Tilemap grassTilemapUnzoom;
    public Tilemap forestTilemapUnzoom;
    public Tilemap mountainLowTilemapUnzoom;
    public Tilemap mountainHighTilemapUnzoom;

    // Tile references for each type of terrain
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
    public float maxZoomOutSize = 50f; // Maximum allowable zoom out size

    private Vector2[] landCenters;
    private Vector2[] islandCenters;
    private bool isZoomedOut;

    // Batch processing variables
    public int batchSize = 100;
    private int currentX = 0;
    private int currentY = 0;

    void Start()
    {
        CenterCameraOnMap();
        GenerateLandCenters();
        GenerateIslandCenters();
        StartCoroutine(GenerateLandsInBatches());

        isZoomedOut = mainCamera.orthographicSize > zoomThreshold;
        UpdateTileVisibility();
    }

    void Update()
    {
        bool shouldZoomOut = mainCamera.orthographicSize > zoomThreshold;
        if (shouldZoomOut != isZoomedOut)
        {
            isZoomedOut = shouldZoomOut;
            Debug.Log("Zoom state changed: " + (isZoomedOut ? "Zoomed Out" : "Zoomed In"));
            UpdateTileVisibility();
        }

        LimitZoom();
    }

    void CenterCameraOnMap()
    {
        float mapWidthInUnits = mapWidth * waterDeepTilemap.cellSize.x;
        float mapHeightInUnits = mapHeight * waterDeepTilemap.cellSize.y;

        Vector3 centerPosition = new Vector3(mapWidthInUnits / 2, mapHeightInUnits / 2, mainCamera.transform.position.z);
        mainCamera.transform.position = centerPosition;
    }

    void LimitZoom()
    {
        // Ensure the camera doesn't zoom out beyond the max zoom out size
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, 1f, maxZoomOutSize);
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

    void UpdateTileVisibility()
    {
        Debug.Log("Updating tile visibility: " + (isZoomedOut ? "Zoomed Out" : "Zoomed In"));

        bool detailedVisibility = !isZoomedOut;
        bool unzoomedVisibility = isZoomedOut;

        ToggleTilemapRenderer(waterDeepTilemap, detailedVisibility);
        ToggleTilemapRenderer(waterTilemap, detailedVisibility);
        ToggleTilemapRenderer(waterUndeepTilemap, detailedVisibility);
        ToggleTilemapRenderer(sandTilemap, detailedVisibility);
        ToggleTilemapRenderer(grassTilemap, detailedVisibility);
        ToggleTilemapRenderer(forestTilemap, detailedVisibility);
        ToggleTilemapRenderer(mountainLowTilemap, detailedVisibility);
        ToggleTilemapRenderer(mountainHighTilemap, detailedVisibility);

        ToggleTilemapRenderer(waterDeepTilemapUnzoom, unzoomedVisibility);
        ToggleTilemapRenderer(waterTilemapUnzoom, unzoomedVisibility);
        ToggleTilemapRenderer(waterUndeepTilemapUnzoom, unzoomedVisibility);
        ToggleTilemapRenderer(sandTilemapUnzoom, unzoomedVisibility);
        ToggleTilemapRenderer(grassTilemapUnzoom, unzoomedVisibility);
        ToggleTilemapRenderer(forestTilemapUnzoom, unzoomedVisibility);
        ToggleTilemapRenderer(mountainLowTilemapUnzoom, unzoomedVisibility);
        ToggleTilemapRenderer(mountainHighTilemapUnzoom, unzoomedVisibility);
    }

    void ToggleTilemapRenderer(Tilemap tilemap, bool isVisible)
    {
        TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.enabled = isVisible;
        }
    }

    IEnumerator GenerateLandsInBatches()
    {
        float adjustedLandSizeFactor = baseLandSizeFactor * (3.0f / numberOfLandmasses);
        float adjustedIslandSizeFactor = baseIslandSizeFactor * (3.0f / numberOfIslands);

        while (currentY < mapHeight)
        {
            for (int i = 0; i < batchSize && currentX < mapWidth; i++)
            {
                float maxHeight = 0f;
                foreach (var center in landCenters)
                {
                    float distance = Vector2.Distance(new Vector2(currentX, currentY), center);
                    if (distance < adjustedLandSizeFactor)
                    {
                        float sample = SamplePerlinNoise(currentX, currentY, center);
                        float height = Mathf.Max(0, 1 - (distance / adjustedLandSizeFactor)) * sample;
                        maxHeight = Mathf.Max(maxHeight, height);
                    }
                }

                foreach (var center in islandCenters)
                {
                    float distance = Vector2.Distance(new Vector2(currentX, currentY), center);
                    if (distance < adjustedIslandSizeFactor)
                    {
                        float sample = SamplePerlinNoise(currentX, currentY, center);
                        float height = Mathf.Max(0, 1 - (distance / adjustedIslandSizeFactor)) * sample;
                        maxHeight = Mathf.Max(maxHeight, height);
                    }
                }

                float adjustedHeight = maxHeight + mountainBias;
                AssignTileBasedOnHeight(adjustedHeight, currentX, currentY);

                currentX++;
                if (currentX >= mapWidth)
                {
                    currentX = 0;
                    currentY++;
                }
            }
            yield return null; // Wait for the next frame before continuing
        }

        Debug.Log("Map generation complete.");
    }

    float SamplePerlinNoise(int x, int y, Vector2 center)
    {
        float xCoord = (x + center.x) * noiseScale;
        float yCoord = (y + center.y) * noiseScale;
        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    void AssignTileBasedOnHeight(float height, int x, int y)
    {
        Vector3Int position = new Vector3Int(x, y, 0);

        if (height < 0.2f)
        {
            waterDeepTilemap.SetTile(position, waterDeepTile);
            waterDeepTilemapUnzoom.SetTile(position, waterDeepTileUnzoom);
        }
        else if (height < 0.4f)
        {
            waterTilemap.SetTile(position, waterTile);
            waterTilemapUnzoom.SetTile(position, waterTileUnzoom);
        }
        else if (height < 0.5f)
        {
            waterUndeepTilemap.SetTile(position, waterUndeepTile);
            waterUndeepTilemapUnzoom.SetTile(position, waterUndeepTileUnzoom);
        }
        else if (height < 0.55f)
        {
            sandTilemap.SetTile(position, sandTile);
            sandTilemapUnzoom.SetTile(position, sandTileUnzoom);
        }
        else if (height < 0.65f)
        {
            grassTilemap.SetTile(position, grassTile);
            grassTilemapUnzoom.SetTile(position, grassTileUnzoom);
        }
        else if (height < 0.75f)
        {
            forestTilemap.SetTile(position, forestGrassTile);
            forestTilemapUnzoom.SetTile(position, forestGrassTileUnzoom);
        }
        else if (height < 0.85f)
        {
            mountainLowTilemap.SetTile(position, mountainLowTile);
            mountainLowTilemapUnzoom.SetTile(position, mountainLowTileUnzoom);
        }
        else
        {
            mountainHighTilemap.SetTile(position, mountainHighTile);
            mountainHighTilemapUnzoom.SetTile(position, mountainHighTileUnzoom);
        }
    }

    void OnApplicationQuit()
    {
        DestroyTilemapObjects();
    }

    void DestroyTilemapObjects()
    {
        // Disable rendering for all tilemaps before destruction to minimize rendering overhead
        List<TilemapRenderer> tilemapRenderers = new List<TilemapRenderer>
        {
            waterDeepTilemap.GetComponent<TilemapRenderer>(), waterTilemap.GetComponent<TilemapRenderer>(), waterUndeepTilemap.GetComponent<TilemapRenderer>(), sandTilemap.GetComponent<TilemapRenderer>(),
            grassTilemap.GetComponent<TilemapRenderer>(), forestTilemap.GetComponent<TilemapRenderer>(), mountainLowTilemap.GetComponent<TilemapRenderer>(), mountainHighTilemap.GetComponent<TilemapRenderer>(),
            waterDeepTilemapUnzoom.GetComponent<TilemapRenderer>(), waterTilemapUnzoom.GetComponent<TilemapRenderer>(), waterUndeepTilemapUnzoom.GetComponent<TilemapRenderer>(), sandTilemapUnzoom.GetComponent<TilemapRenderer>(),
            grassTilemapUnzoom.GetComponent<TilemapRenderer>(), forestTilemapUnzoom.GetComponent<TilemapRenderer>(), mountainLowTilemapUnzoom.GetComponent<TilemapRenderer>(), mountainHighTilemapUnzoom.GetComponent<TilemapRenderer>()
        };

        foreach (TilemapRenderer renderer in tilemapRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        // Use DestroyImmediate as we're quitting and don't expect to interact with these objects anymore
        List<GameObject> tilemapObjects = new List<GameObject>
        {
            waterDeepTilemap.gameObject, waterTilemap.gameObject, waterUndeepTilemap.gameObject, sandTilemap.gameObject,
            grassTilemap.gameObject, forestTilemap.gameObject, mountainLowTilemap.gameObject, mountainHighTilemap.gameObject,
            waterDeepTilemapUnzoom.gameObject, waterTilemapUnzoom.gameObject, waterUndeepTilemapUnzoom.gameObject, sandTilemapUnzoom.gameObject,
            grassTilemapUnzoom.gameObject, forestTilemapUnzoom.gameObject, mountainLowTilemapUnzoom.gameObject, mountainHighTilemapUnzoom.gameObject
        };

        foreach (GameObject tilemapObject in tilemapObjects)
        {
            if (tilemapObject != null)
            {
                DestroyImmediate(tilemapObject);
            }
        }
    }
}
