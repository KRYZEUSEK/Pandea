using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{
    const float scale = 1f;

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDst;

    public Transform viewer;
    public Material mapMaterial;

    public string terrainLayerName = "Terrain";

    [Header("Spawning Configuration")]
    public PlantScriptable[] spawnableObjects;

    [Header("Story & Base Elements")]
    public StoryElement[] storyElements;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        CalculateStoryElementPositions();
        UpdateVisibleChunks();
    }

    void CalculateStoryElementPositions()
    {
        System.Random prng = new System.Random(mapGenerator.seed);

        for (int i = 0; i < storyElements.Length; i++)
        {
            float angle = (float)(prng.NextDouble() * Mathf.PI * 2);
            float distance = Mathf.Lerp(storyElements[i].minDistance, storyElements[i].maxDistance, (float)prng.NextDouble());

            float spawnX = Mathf.Cos(angle) * distance;
            float spawnY = Mathf.Sin(angle) * distance;

            storyElements[i].targetPosition = new Vector2(spawnX, spawnY);
            storyElements[i].isSpawned = false;
            storyElements[i].isHeightAdjusted = false;

            if (storyElements[i].spawnImmediately)
            {
                Vector3 tempPos = new Vector3(spawnX, 0, spawnY);
                GameObject spawnedGo = Instantiate(storyElements[i].prefab, tempPos, Quaternion.identity);
                spawnedGo.transform.parent = transform;

                storyElements[i].spawnedInstance = spawnedGo;
                storyElements[i].isSpawned = true;

                if (!string.IsNullOrEmpty(storyElements[i].layerName))
                {
                    int layerIndex = LayerMask.NameToLayer(storyElements[i].layerName);
                    if (layerIndex != -1)
                    {
                        spawnedGo.layer = layerIndex;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (viewer == null) return;

        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        int layerIndex = LayerMask.NameToLayer(terrainLayerName);
        if (layerIndex == -1) layerIndex = 0;

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, layerIndex, spawnableObjects, this));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        PlantScriptable[] spawnSettings;
        EndlessTerrain parentTerrain;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        Transform objectsParent;
        bool objectsSpawned = false;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material, int layerIndex, PlantScriptable[] spawnSettings, EndlessTerrain parentTerrain)
        {
            this.detailLevels = detailLevels;
            this.spawnSettings = spawnSettings;
            this.parentTerrain = parentTerrain;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshObject.layer = layerIndex;

            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;

            objectsParent = new GameObject("Objects").transform;
            objectsParent.SetParent(meshObject.transform);
            objectsParent.localPosition = Vector3.zero;
            objectsParent.localRotation = Quaternion.identity;
            objectsParent.localScale = Vector3.one;

            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDst;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;

                            if (lodIndex == 0)
                            {
                                meshCollider.sharedMesh = lodMesh.mesh;

                                if (!objectsSpawned)
                                {
                                    SpawnStoryElements();
                                    SpawnObjects();
                                }

                                objectsParent.gameObject.SetActive(true);
                            }
                            else
                            {
                                meshCollider.sharedMesh = null;
                                objectsParent.gameObject.SetActive(false);
                            }
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
        }

        void SpawnStoryElements()
        {
            if (parentTerrain.storyElements == null) return;

            int width = mapData.heightMap.GetLength(0);
            int height = mapData.heightMap.GetLength(1);
            float chunkWorldX = position.x;
            float chunkWorldY = position.y;

            for (int i = 0; i < parentTerrain.storyElements.Length; i++)
            {
                StoryElement el = parentTerrain.storyElements[i];

                if (!el.isHeightAdjusted && bounds.Contains(new Vector3(el.targetPosition.x, el.targetPosition.y, 0)))
                {
                    int gridX = Mathf.RoundToInt(el.targetPosition.x - (chunkWorldX - (width - 1) / 2f));
                    int gridY = Mathf.RoundToInt((chunkWorldY + (height - 1) / 2f) - el.targetPosition.y);

                    gridX = Mathf.Clamp(gridX, 0, width - 1);
                    gridY = Mathf.Clamp(gridY, 0, height - 1);

                    float terrainHeight = mapData.heightMap[gridX, gridY];
                    float heightMultiplier = mapGenerator.meshHeightMultiplier;
                    float finalY = new AnimationCurve(mapGenerator.meshHeightCurve.keys).Evaluate(terrainHeight) * heightMultiplier;

                    Vector3 correctPos = new Vector3(el.targetPosition.x, finalY, el.targetPosition.y);

                    if (el.spawnImmediately && el.spawnedInstance != null)
                    {
                        el.spawnedInstance.transform.position = correctPos;
                    }
                    else if (!el.isSpawned)
                    {
                        GameObject go = Object.Instantiate(el.prefab, correctPos, Quaternion.identity);
                        go.transform.parent = parentTerrain.transform;

                        if (!string.IsNullOrEmpty(el.layerName))
                        {
                            int layerIndex = LayerMask.NameToLayer(el.layerName);
                            if (layerIndex != -1) go.layer = layerIndex;
                        }

                        el.spawnedInstance = go;
                        el.isSpawned = true;
                    }

                    el.isHeightAdjusted = true;
                }
            }
        }

        void SpawnObjects()
        {
            objectsSpawned = true;

            if (spawnSettings == null || spawnSettings.Length == 0) return;

            int width = mapData.heightMap.GetLength(0);
            int height = mapData.heightMap.GetLength(1);

            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;

            int chunkHash = Mathf.RoundToInt(position.x * 100f) + Mathf.RoundToInt(position.y);
            System.Random prng = new System.Random(mapGenerator.seed + chunkHash);

            float chunkWorldX = position.x;
            float chunkWorldY = position.y;

            float seedOffsetX = mapGenerator.seed * 100f;
            float seedOffsetY = mapGenerator.seed * 100f;

            int step = 8; // Zwiększony krok dla lepszej wydajności

            float safeZoneRadius = 20f;
            float sqrSafeZoneRadius = safeZoneRadius * safeZoneRadius;

            for (int y = 0; y < height; y += step)
            {
                for (int x = 0; x < width; x += step)
                {
                    float currentHeight = mapData.heightMap[x, y];
                    float currentSlope = CalculateSlope(x, y, width, height, mapData.heightMap);

                    // NEW: Pobieramy wektor nachylenia terenu
                    Vector3 groundNormal = CalculateNormal(x, y, width, height, mapData.heightMap);

                    float globalX = (chunkWorldX + x) / MapGenerator.mapChunkSize;
                    float globalY = (chunkWorldY - y) / MapGenerator.mapChunkSize;

                    float pointWorldX = chunkWorldX + (topLeftX + x);
                    float pointWorldZ = chunkWorldY - (topLeftZ - y);
                    float sqrDistanceToCenter = (pointWorldX * pointWorldX) + (pointWorldZ * pointWorldZ);

                    if (sqrDistanceToCenter < sqrSafeZoneRadius) continue;

                    bool obstacleSpawned = false;

                    for (int i = 0; i < spawnSettings.Length; i++)
                    {
                        PlantScriptable plant = spawnSettings[i];

                        if (plant == null || plant.prefabs == null || plant.prefabs.Length == 0) continue;

                        // Smart spawning: Tylko jeden duży obiekt na kratkę, ale trawa może się nakładać
                        if (!plant.isGroundCover && obstacleSpawned) continue;

                        if (currentHeight < plant.minHeight || currentHeight > plant.maxHeight) continue;
                        if (currentSlope > plant.maxSlope) continue;

                        float noiseValue = Mathf.PerlinNoise(
                            (globalX + seedOffsetX) * plant.noiseScale + plant.noiseOffset.x,
                            (globalY + seedOffsetY) * plant.noiseScale + plant.noiseOffset.y
                        );

                        if (noiseValue < plant.noiseThreshold) continue;

                        // Rzadsze spawnowanie obiektów interaktywnych
                        float finalSpawnChance = plant.isInteractable ? (plant.density * 0.05f) : plant.density;
                        if (prng.NextDouble() > finalSpawnChance) continue;

                        float heightMultiplier = mapGenerator.meshHeightMultiplier;
                        AnimationCurve heightCurve = new AnimationCurve(mapGenerator.meshHeightCurve.keys);
                        float localY = heightCurve.Evaluate(currentHeight) * heightMultiplier;

                        // Lekkie zagłębienie trawy, żeby nie latała w powietrzu
                        if (plant.isGroundCover) localY -= 0.15f;

                        Vector3 localPos = new Vector3(topLeftX + x, localY, topLeftZ - y);

                        float randomOffsetX = (float)prng.NextDouble() * step - (step / 2f);
                        float randomOffsetZ = (float)prng.NextDouble() * step - (step / 2f);
                        localPos.x += randomOffsetX;
                        localPos.z += randomOffsetZ;

                        int prefabIndex = prng.Next(0, plant.prefabs.Length);
                        GameObject selectedPrefab = plant.prefabs[prefabIndex];

                        if (selectedPrefab != null)
                        {
                            GameObject go = Object.Instantiate(selectedPrefab, objectsParent);
                            go.transform.localPosition = localPos;

                            // Obrót dopasowany do zbocza góry + losowy obrót wokół własnej osi
                            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
                            Quaternion randomSpin = Quaternion.Euler(0, (float)prng.Next(0, 360), 0);
                            go.transform.localRotation = slopeRotation * randomSpin;

                            float randomScale = Mathf.Lerp(plant.minScale, plant.maxScale, (float)prng.NextDouble());
                            go.transform.localScale = Vector3.one * randomScale;

                            if (plant.isInteractable) go.tag = "Interactable";
                            else go.isStatic = true;
                        }

                        if (!plant.isGroundCover)
                        {
                            obstacleSpawned = true;
                        }
                    }
                }
            }
        }

        float CalculateSlope(int x, int y, int width, int height, float[,] heightMap)
        {
            float hL = heightMap[Mathf.Clamp(x - 1, 0, width - 1), y];
            float hR = heightMap[Mathf.Clamp(x + 1, 0, width - 1), y];
            float hD = heightMap[x, Mathf.Clamp(y - 1, 0, height - 1)];
            float hU = heightMap[x, Mathf.Clamp(y + 1, 0, height - 1)];

            float dX = Mathf.Abs(hL - hR);
            float dZ = Mathf.Abs(hD - hU);

            return Mathf.Max(dX, dZ) * 100f;
        }

        Vector3 CalculateNormal(int x, int y, int width, int height, float[,] heightMap)
        {
            float hL = heightMap[Mathf.Clamp(x - 1, 0, width - 1), y];
            float hR = heightMap[Mathf.Clamp(x + 1, 0, width - 1), y];
            float hD = heightMap[x, Mathf.Clamp(y - 1, 0, height - 1)];
            float hU = heightMap[x, Mathf.Clamp(y + 1, 0, height - 1)];

            float dX = hL - hR;
            float dZ = hD - hU;

            return new Vector3(dX, 2f / MapGenerator.mapChunkSize, dZ).normalized;
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
    }

    [System.Serializable]
    public class StoryElement
    {
        public string name = "Cel Misji";
        public GameObject prefab;
        public string layerName = "";
        public float minDistance = 50f;
        public float maxDistance = 150f;
        public bool spawnImmediately = false;

        [HideInInspector] public Vector2 targetPosition;
        [HideInInspector] public bool isSpawned;
        [HideInInspector] public bool isHeightAdjusted;
        [HideInInspector] public GameObject spawnedInstance;
    }
}