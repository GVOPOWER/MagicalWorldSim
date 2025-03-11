using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandGenerator : NetworkBehaviour
{
    // Tilemaps and their corresponding tiles
    public Tilemap waterDeepTilemap, waterTilemap, waterUndeepTilemap, sandTilemap, grassTilemap, forestTilemap, mountainLowTilemap, mountainHighTilemap;
    public TileBase waterDeepTile, waterTile, waterUndeepTile, sandTile, grassTile, forestGrassTile, mountainLowTile, mountainHighTile;

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
    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    private int chunkSize = 10;

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
        // Ensure this logic only runs for the local player
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

        ToggleTilemapRenderer(waterDeepTilemap, detailedVisibility);
        ToggleTilemapRenderer(waterTilemap, detailedVisibility);
        ToggleTilemapRenderer(waterUndeepTilemap, detailedVisibility);
        ToggleTilemapRenderer(sandTilemap, detailedVisibility);
        ToggleTilemapRenderer(grassTilemap, detailedVisibility);
        ToggleTilemapRenderer(forestTilemap, detailedVisibility);
        ToggleTilemapRenderer(mountainLowTilemap, detailedVisibility);
        ToggleTilemapRenderer(mountainHighTilemap, detailedVisibility);
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

                    // Create a new list to hold tile data for this chunk
                    List<TileData> chunkTiles = new List<TileData>();

                    // Generate the chunk and populate the chunkTiles list
                    GenerateChunk(x, y, adjustedLandSizeFactor, adjustedIslandSizeFactor, chunkTiles);

                    // Call the RPC to sync this chunk's tiles with clients
                    RpcSyncChunk(chunkTiles.ToArray());

                    yield return null;
                }
            }
        }
    }

    void GenerateChunk(int startX, int startY, float adjustedLandSizeFactor, float adjustedIslandSizeFactor, List<TileData> chunkTiles)
    {
        Debug.Log($"Generating chunk at {startX}, {startY}");
        for (int x = startX; x < startX + chunkSize && x < mapWidth; x++)
        {
            for (int y = startY; y < startY + chunkSize && y < mapHeight; y++)
            {
                float height = GetHeight(x, y, adjustedLandSizeFactor, adjustedIslandSizeFactor);
                TileType tileType = AssignTileBasedOnHeight(height, x, y);

                // Add the tile data to the chunkTiles list
                chunkTiles.Add(new TileData(new Vector3Int(x, y, 0), tileType));
            }
        }
    }

    [ClientRpc]
    void RpcSyncChunk(TileData[] chunkTiles)
    {
        Debug.Log($"Synchronizing chunk on client.");
        foreach (var tileData in chunkTiles)
        {
            SetTileOnClient(tileData.position, tileData.tileType);
        }
    }

    void SetTileOnClient(Vector3Int position, TileType tileType)
    {
        TileBase tile = GetTileFromType(tileType);
        if (tile != null)
        {
            Tilemap targetTilemap = GetTilemapFromType(tileType);
            targetTilemap.SetTile(position, tile);
        }
    }

    Tilemap GetTilemapFromType(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.WaterDeep: return waterDeepTilemap;
            case TileType.Water: return waterTilemap;
            case TileType.WaterUndeep: return waterUndeepTilemap;
            case TileType.Sand: return sandTilemap;
            case TileType.Grass: return grassTilemap;
            case TileType.ForestGrass: return forestTilemap;
            case TileType.MountainLow: return mountainLowTilemap;
            case TileType.MountainHigh: return mountainHighTilemap;
            default: return null;
        }
    }

    TileBase GetTileFromType(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.WaterDeep: return waterDeepTile;
            case TileType.Water: return waterTile;
            case TileType.WaterUndeep: return waterUndeepTile;
            case TileType.Sand: return sandTile;
            case TileType.Grass: return grassTile;
            case TileType.ForestGrass: return forestGrassTile;
            case TileType.MountainLow: return mountainLowTile;
            case TileType.MountainHigh: return mountainHighTile;
            default: return null;
        }
    }

    TileType AssignTileBasedOnHeight(float height, int x, int y)
    {
        if (height < 0.2f)
            return TileType.WaterDeep;
        else if (height < 0.4f)
            return TileType.Water;
        else if (height < 0.5f)
            return TileType.WaterUndeep;
        else if (height < 0.56f)
            return TileType.Sand;
        else if (height < 0.725f)
            return TileType.Grass;
        else if (height < 0.85f)
            return TileType.ForestGrass;
        else if (height < 0.925f)
            return TileType.MountainLow;
        else
            return TileType.MountainHigh;
    }

    public enum TileType
    {
        WaterDeep,
        Water,
        WaterUndeep,
        Sand,
        Grass,
        ForestGrass,
        MountainLow,
        MountainHigh
    }

    public struct TileData
    {
        public Vector3Int position;
        public TileType tileType;

        public TileData(Vector3Int position, TileType tileType)
        {
            this.position = position;
            this.tileType = tileType;
        }
    }
}
