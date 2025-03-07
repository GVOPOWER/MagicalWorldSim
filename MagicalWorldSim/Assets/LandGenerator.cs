using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandGenerator : NetworkBehaviour
{
    // Tilemaps and their unzoomed counterparts
    public Tilemap waterDeepTilemap, waterTilemap, waterUndeepTilemap, sandTilemap, grassTilemap, forestTilemap, mountainLowTilemap, mountainHighTilemap;
    public Tilemap waterDeepTilemapUnzoom, waterTilemapUnzoom, waterUndeepTilemapUnzoom, sandTilemapUnzoom, grassTilemapUnzoom, forestTilemapUnzoom, mountainLowTilemapUnzoom, mountainHighTilemapUnzoom;

    // Tile bases and their unzoomed counterparts
    public TileBase waterDeepTile, waterTile, waterUndeepTile, sandTile, grassTile, forestGrassTile, mountainLowTile, mountainHighTile;
    public TileBase waterDeepTileUnzoom, waterTileUnzoom, waterUndeepTileUnzoom, sandTileUnzoom, grassTileUnzoom, forestGrassTileUnzoom, mountainLowTileUnzoom, mountainHighTileUnzoom;

    // Map configuration
    public int mapWidth = 100, mapHeight = 100;
    public float noiseScale = 0.1f;
    public int numberOfLandmasses = 3, numberOfIslands = 5;
    public float baseLandSizeFactor = 30f, baseIslandSizeFactor = 10f, mountainBias = 0.2f;

    // Camera and zoom configuration
    public Camera mainCamera;
    public float zoomThreshold = 20f, maxZoomOutSize = 50f;

    // Internal state
    private Vector2[] landCenters, islandCenters;
    private bool isZoomedOut;
    private Dictionary<Vector3Int, TileBase> generatedTiles = new Dictionary<Vector3Int, TileBase>();
    public int chunkSize = 10;
    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();

    [SyncVar] private bool mapGenerated = false;

    public override void OnStartServer()
    {
        Debug.Log("Server started, initiating map generation.");
        if (!mapGenerated)
        {
            CenterCameraOnMap();
            GenerateLandCenters();
            GenerateIslandCenters();
            StartCoroutine(GenerateMapInChunks());
            mapGenerated = true;
            Debug.Log("Map generation complete.");
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        bool shouldZoomOut = mainCamera.orthographicSize > zoomThreshold;
        if (shouldZoomOut != isZoomedOut)
        {
            isZoomedOut = shouldZoomOut;
            UpdateTileVisibility();
        }
        LimitZoom();
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
        return Mathf.Exp(-distance * distance / (2 * sizeFactor * sizeFactor));
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
                    RpcSyncChunk(x, y);
                    yield return null;
                }
            }
        }
    }

    void GenerateChunk(int startX, int startY, float adjustedLandSizeFactor, float adjustedIslandSizeFactor)
    {
        Debug.Log($"Generating chunk at {startX}, {startY}");
        for (int x = startX; x < startX + chunkSize && x < mapWidth; x++)
        {
            for (int y = startY; y < startY + chunkSize && y < mapHeight; y++)
            {
                float height = GetHeight(x, y, adjustedLandSizeFactor, adjustedIslandSizeFactor);
                AssignTileBasedOnHeight(height, x, y);
            }
        }
    }

    [ClientRpc]
    void RpcSyncChunk(int startX, int startY)
    {
        Debug.Log($"Synchronizing chunk at {startX}, {startY} on client.");
        if (!isServer)
            GenerateChunk(startX, startY, baseLandSizeFactor, baseIslandSizeFactor);
    }

    void AssignTileBasedOnHeight(float height, int x, int y)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        TileBase tile;

        if (height < 0.2f)
            tile = waterDeepTile;
        else if (height < 0.4f)
            tile = waterTile;
        else if (height < 0.5f)
            tile = waterUndeepTile;
        else if (height < 0.56f)
            tile = sandTile;
        else if (height < 0.725f)
            tile = grassTile;
        else if (height < 0.85f)
            tile = forestGrassTile;
        else if (height < 0.925f)
            tile = mountainLowTile;
        else
            tile = mountainHighTile;

        // Assign the tile to the appropriate tilemap based on its type
        if (tile == waterDeepTile) waterDeepTilemap.SetTile(pos, tile);
        else if (tile == waterTile) waterTilemap.SetTile(pos, tile);
        else if (tile == waterUndeepTile) waterUndeepTilemap.SetTile(pos, tile);
        else if (tile == sandTile) sandTilemap.SetTile(pos, tile);
        else if (tile == grassTile) grassTilemap.SetTile(pos, tile);
        else if (tile == forestGrassTile) forestTilemap.SetTile(pos, tile);
        else if (tile == mountainLowTile) mountainLowTilemap.SetTile(pos, tile);
        else if (tile == mountainHighTile) mountainHighTilemap.SetTile(pos, tile);
    }
}
