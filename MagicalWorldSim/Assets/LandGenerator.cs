using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandGenerator : NetworkBehaviour
{
    public Tilemap waterDeepTilemap, waterTilemap, waterUndeepTilemap, sandTilemap, grassTilemap, forestTilemap, mountainLowTilemap, mountainHighTilemap;
    public Tilemap waterDeepTilemapUnzoom, waterTilemapUnzoom, waterUndeepTilemapUnzoom, sandTilemapUnzoom, grassTilemapUnzoom, forestTilemapUnzoom, mountainLowTilemapUnzoom, mountainHighTilemapUnzoom;

    public TileBase waterDeepTile, waterTile, waterUndeepTile, sandTile, grassTile, forestGrassTile, mountainLowTile, mountainHighTile;
    public TileBase waterDeepTileUnzoom, waterTileUnzoom, waterUndeepTileUnzoom, sandTileUnzoom, grassTileUnzoom, forestGrassTileUnzoom, mountainLowTileUnzoom, mountainHighTileUnzoom;

    public int mapWidth = 100, mapHeight = 100;
    public float noiseScale = 0.1f;
    public int numberOfLandmasses = 3, numberOfIslands = 5;
    public float baseLandSizeFactor = 30f, baseIslandSizeFactor = 10f, mountainBias = 0.2f;

    public Camera mainCamera;
    public float zoomThreshold = 20f, maxZoomOutSize = 50f;

    private Vector2[] landCenters, islandCenters;
    private bool isZoomedOut;
    private Dictionary<Vector3Int, TileBase> generatedTiles = new Dictionary<Vector3Int, TileBase>();
    public int chunkSize = 10;
    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();

    void Start()
    {
        if (isServer)
        {
            CenterCameraOnMap();
            GenerateLandCenters();
            GenerateIslandCenters();
            StartCoroutine(GenerateMapInChunks());
        }
        isZoomedOut = mainCamera.orthographicSize > zoomThreshold;
        UpdateTileVisibility();
    }

    void Update()
    {
        bool shouldZoomOut = mainCamera.orthographicSize > zoomThreshold;
        if (shouldZoomOut != isZoomedOut)
        {
            isZoomedOut = shouldZoomOut;
            UpdateTileVisibility();
        }
        LimitZoom();
    }

    void CenterCameraOnMap()
    {
        float mapWidthInUnits = mapWidth * waterDeepTilemap.cellSize.x;
        float mapHeightInUnits = mapHeight * waterDeepTilemap.cellSize.y;
        mainCamera.transform.position = new Vector3(mapWidthInUnits / 2, mapHeightInUnits / 2, mainCamera.transform.position.z);
    }

    void LimitZoom()
    {
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, 1f, maxZoomOutSize);
    }

    void GenerateLandCenters()
    {
        landCenters = new Vector2[numberOfLandmasses];
        for (int i = 0; i < numberOfLandmasses; i++)
            landCenters[i] = new Vector2(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
    }

    void GenerateIslandCenters()
    {
        islandCenters = new Vector2[numberOfIslands];
        for (int i = 0; i < numberOfIslands; i++)
            islandCenters[i] = new Vector2(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
    }

    void UpdateTileVisibility()
    {
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
        var renderer = tilemap.GetComponent<TilemapRenderer>();
        if (renderer != null) renderer.enabled = isVisible;
    }

    [Server]
    IEnumerator GenerateMapInChunks()
    {
        float adjustedLandSizeFactor = baseLandSizeFactor * (3.0f / numberOfLandmasses);
        float adjustedIslandSizeFactor = baseIslandSizeFactor * (3.0f / numberOfIslands);

        for (int y = 0; y < mapHeight; y += chunkSize)
        {
            for (int x = 0; x < mapWidth; x += chunkSize)
            {
                Vector2Int chunkCoord = new Vector2Int(x / chunkSize, y / chunkSize);
                if (!generatedChunks.Contains(chunkCoord))
                {
                    generatedChunks.Add(chunkCoord);
                    GenerateChunk(x, y, adjustedLandSizeFactor, adjustedIslandSizeFactor);
                    yield return null;
                }
            }
        }
    }

    void GenerateChunk(int startX, int startY, float adjustedLandSizeFactor, float adjustedIslandSizeFactor)
    {
        for (int x = startX; x < startX + chunkSize && x < mapWidth; x++)
        {
            for (int y = startY; y < startY + chunkSize && y < mapHeight; y++)
            {
                float height = GetHeight(x, y, adjustedLandSizeFactor, adjustedIslandSizeFactor);
                AssignTileBasedOnHeight(height, x, y);
            }
        }
    }

    float GetHeight(int x, int y, float landSize, float islandSize)
    {
        float maxHeight = 0f;

        foreach (var center in landCenters)
            maxHeight = Mathf.Max(maxHeight, CalculateHeight(x, y, center, landSize));

        foreach (var center in islandCenters)
            maxHeight = Mathf.Max(maxHeight, CalculateHeight(x, y, center, islandSize));

        return maxHeight + mountainBias;
    }

    float CalculateHeight(int x, int y, Vector2 center, float sizeFactor)
    {
        float distance = Vector2.Distance(new Vector2(x, y), center);
        if (distance >= sizeFactor) return 0;
        return Mathf.Max(0, 1 - (distance / sizeFactor)) * Mathf.PerlinNoise((x + center.x) * noiseScale, (y + center.y) * noiseScale);
    }

    void AssignTileBasedOnHeight(float height, int x, int y)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        TileBase tile = height switch
        {
            < 0.2f => waterDeepTile,
            < 0.4f => waterTile,
            < 0.5f => waterUndeepTile,
            < 0.56f => sandTile,
            < 0.725f => grassTile,
            < 0.85f => forestGrassTile,
            < 0.925f => mountainLowTile,
            _ => mountainHighTile
        };
        generatedTiles[pos] = tile;
    }

    void OnApplicationQuit()
    {
        foreach (Tilemap tm in FindObjectsOfType<Tilemap>())
            if (tm != null) DestroyImmediate(tm.gameObject);
    }
}
