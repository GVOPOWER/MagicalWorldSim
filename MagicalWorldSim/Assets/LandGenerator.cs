using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandGenerator : NetworkBehaviour
{
    // Tilemaps and TileBases
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
    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    private int chunkSize = 10;

    private bool mapGenerated = false;

    public override void OnStartLocalPlayer()
    {
        if (isServer)
        {
            Debug.Log("Host player, generating map locally.");
            CenterCameraOnMap();
            GenerateLandCenters();
            GenerateIslandCenters();
            StartCoroutine(GenerateMapInChunks());
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

    IEnumerator GenerateMapInChunks()
    {
        float adjustedLandSizeFactor = baseLandSizeFactor * (3.0f / numberOfLandmasses);
        float adjustedIslandSizeFactor = baseIslandSizeFactor * (3.0f / numberOfIslands);

        List<TileData> allTiles = new List<TileData>();

        for (int y = 0; y < mapHeight; y += chunkSize)
        {
            for (int x = 0; x < mapWidth; x += chunkSize)
            {
                Vector2Int chunkCoord = new Vector2Int(x / chunkSize, y / chunkSize);
                if (!generatedChunks.Contains(chunkCoord))
                {
                    generatedChunks.Add(chunkCoord);

                    List<TileData> chunkTiles = new List<TileData>();

                    GenerateChunk(x, y, adjustedLandSizeFactor, adjustedIslandSizeFactor, chunkTiles);
                    allTiles.AddRange(chunkTiles);

                    yield return null;
                }
            }
        }

        Debug.Log($"Map generation complete, sending {allTiles.Count} tiles to server.");
        CmdSendMapData(allTiles.ToArray());
    }

    [Command]
    void CmdSendMapData(TileData[] mapData)
    {
        Debug.Log("Server received map data from host.");
        RpcSyncMap(mapData);
    }

    [ClientRpc]
    void RpcSyncMap(TileData[] mapData)
    {
        Debug.Log($"Client received map data with {mapData.Length} tiles.");
        foreach (var tileData in mapData)
        {
            SetTileOnClient(tileData.position, tileData.tileType);
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
        {
            landCenters[i] = new Vector2(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
            Debug.Log($"Land center {i}: {landCenters[i]}");
        }
    }

    void GenerateIslandCenters()
    {
        islandCenters = new Vector2[numberOfIslands];
        for (int i = 0; i < numberOfIslands; i++)
        {
            islandCenters[i] = new Vector2(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
            Debug.Log($"Island center {i}: {islandCenters[i]}");
        }
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

    void GenerateChunk(int startX, int startY, float adjustedLandSizeFactor, float adjustedIslandSizeFactor, List<TileData> chunkTiles)
    {
        Debug.Log($"Generating chunk at {startX}, {startY}");
        for (int x = startX; x < startX + chunkSize && x < mapWidth; x++)
        {
            for (int y = startY; y < startY + chunkSize && y < mapHeight; y++)
            {
                float height = GetHeight(x, y, adjustedLandSizeFactor, adjustedIslandSizeFactor);
                TileType tileType = AssignTileBasedOnHeight(height, x, y);

                chunkTiles.Add(new TileData(new Vector3Int(x, y, 0), tileType));
            }
        }
    }

    void SetTileOnClient(Vector3Int position, TileType tileType)
    {
        TileBase tile = GetTileFromType(tileType);
        TileBase tileUnzoom = GetTileUnzoomFromType(tileType);

        if (tile == null || tileUnzoom == null)
        {
            Debug.LogError($"TileBase is null for TileType: {tileType}");
            return;
        }

        Tilemap targetTilemap = GetTilemapFromType(tileType);
        Tilemap targetTilemapUnzoom = GetTilemapUnzoomFromType(tileType);

        if (targetTilemap == null || targetTilemapUnzoom == null)
        {
            Debug.LogError($"Tilemap is null for TileType: {tileType}");
            return;
        }

        targetTilemap.SetTile(position, tile);
        targetTilemapUnzoom.SetTile(position, tileUnzoom);
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

    Tilemap GetTilemapUnzoomFromType(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.WaterDeep: return waterDeepTilemapUnzoom;
            case TileType.Water: return waterTilemapUnzoom;
            case TileType.WaterUndeep: return waterUndeepTilemapUnzoom;
            case TileType.Sand: return sandTilemapUnzoom;
            case TileType.Grass: return grassTilemapUnzoom;
            case TileType.ForestGrass: return forestTilemapUnzoom;
            case TileType.MountainLow: return mountainLowTilemapUnzoom;
            case TileType.MountainHigh: return mountainHighTilemapUnzoom;
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

    TileBase GetTileUnzoomFromType(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.WaterDeep: return waterDeepTileUnzoom;
            case TileType.Water: return waterTileUnzoom;
            case TileType.WaterUndeep: return waterUndeepTileUnzoom;
            case TileType.Sand: return sandTileUnzoom;
            case TileType.Grass: return grassTileUnzoom;
            case TileType.ForestGrass: return forestGrassTileUnzoom;
            case TileType.MountainLow: return mountainLowTileUnzoom;
            case TileType.MountainHigh: return mountainHighTileUnzoom;
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
