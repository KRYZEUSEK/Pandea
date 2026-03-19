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
    [Tooltip("Dodaj tutaj unikalne obiekty (np. bazę główną), które mają zespawnować się tylko raz w określonym dystansie.")]
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

        // Losowanie i kalkulowanie ukrytych lokacji dla obiektów fabularnych
        CalculateStoryElementPositions();

        UpdateVisibleChunks();
    }

    void CalculateStoryElementPositions()
    {
        System.Random prng = new System.Random(mapGenerator.seed);

        for (int i = 0; i < storyElements.Length; i++)
        {
            // Losujemy kąt (0-360 stopni) i odległość w zadanym przedziale
            float angle = (float)(prng.NextDouble() * Mathf.PI * 2);
            float distance = Mathf.Lerp(storyElements[i].minDistance, storyElements[i].maxDistance, (float)prng.NextDouble());

            // Obliczamy wektor 2D (X, Z w świecie gry)
            float spawnX = Mathf.Cos(angle) * distance;
            float spawnY = Mathf.Sin(angle) * distance;

            storyElements[i].targetPosition = new Vector2(spawnX, spawnY);
            storyElements[i].isSpawned = false;
        }
    }

    void Update()
    {
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
                    // Przekazujemy referencję do 'this' (EndlessTerrain), aby Chunk miał dostęp do fabuły
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
            // Bounds w 2D (X, Y) odpowiadają globalnym kordom X i Z
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
                                    SpawnStoryElements(); // Najpierw sprawdzamy elementy bazy/fabuły
                                    SpawnObjects();       // Potem rośliny
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

                // ZMIANA: Sprawdzamy czy wysokość tego obiektu została już skorygowana na tym chunku
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

                    // ZMIANA: Jeśli obiekt był zespawnowany od razu, po prostu korygujemy jego pozycję
                    if (el.spawnImmediately && el.spawnedInstance != null)
                    {
                        el.spawnedInstance.transform.position = correctPos;
                        el.spawnedInstance.layer = meshObject.layer;
                    }
                    // W przeciwnym razie spawniemy go standardowo po dojściu na miejsce
                    else if (!el.isSpawned)
                    {
                        GameObject go = Object.Instantiate(el.prefab, correctPos, Quaternion.identity);
                        go.layer = meshObject.layer;
                        go.transform.parent = parentTerrain.transform;

                        el.spawnedInstance = go;
                        el.isSpawned = true;
                    }

                    el.isHeightAdjusted = true; // Zaznaczamy, że w tej sesji obiekt otrzymał już poprawną wysokość
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

            System.Random prng = new System.Random(mapGenerator.seed + position.GetHashCode());

            float chunkWorldX = position.x;
            float chunkWorldY = position.y;

            // ZMIANA: Zwiększony 'step' (z 1 na 3) mocno rozrzedzi roślinność
            // Nie będą już generować się na każdej kratce terenu, a będą bardziej porozrzucane.
            int step = 3;

            for (int y = 0; y < height; y += step)
            {
                for (int x = 0; x < width; x += step)
                {
                    float currentHeight = mapData.heightMap[x, y];
                    float currentSlope = CalculateSlope(x, y, width, height, mapData.heightMap);

                    float globalX = (chunkWorldX + x) / MapGenerator.mapChunkSize;
                    float globalY = (chunkWorldY - y) / MapGenerator.mapChunkSize;

                    for (int i = 0; i < spawnSettings.Length; i++)
                    {
                        PlantScriptable plant = spawnSettings[i];

                        if (plant == null || plant.prefabs == null || plant.prefabs.Length == 0) continue;
                        if (currentHeight < plant.minHeight || currentHeight > plant.maxHeight) continue;
                        if (currentSlope > plant.maxSlope) continue;

                        float noiseValue = Mathf.PerlinNoise(
                            globalX * plant.noiseScale + plant.noiseOffset.x,
                            globalY * plant.noiseScale + plant.noiseOffset.y
                        );

                        if (noiseValue < plant.noiseThreshold) continue;
                        if (prng.NextDouble() > plant.density) continue;

                        float heightMultiplier = mapGenerator.meshHeightMultiplier;
                        AnimationCurve heightCurve = new AnimationCurve(mapGenerator.meshHeightCurve.keys);
                        float localY = heightCurve.Evaluate(currentHeight) * heightMultiplier;

                        Vector3 localPos = new Vector3(topLeftX + x, localY, topLeftZ - y);

                        // Wzmocniony jitter zapewnia organiczne rozrzucenie (łamanie siatki)
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
                            go.transform.localRotation = Quaternion.Euler(0, (float)prng.Next(0, 360), 0);

                            float randomScale = Mathf.Lerp(plant.minScale, plant.maxScale, (float)prng.NextDouble());
                            go.transform.localScale = Vector3.one * randomScale;

                            if (plant.isInteractable) go.tag = "Interactable";
                            else go.isStatic = true;
                        }

                        goto NextGridPoint;
                    }
                NextGridPoint:;
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
        public float minDistance = 50f;
        public float maxDistance = 150f;

        [Tooltip("Zaznacz to, jeśli obiekt ma istnieć na scenie od razu po odpaleniu gry (np. do systemu questów). Zostanie przyciągnięty do ziemi, gdy gracz się do niego zbliży.")]
        public bool spawnImmediately = false;

        [HideInInspector] public Vector2 targetPosition;
        [HideInInspector] public bool isSpawned;
        [HideInInspector] public bool isHeightAdjusted;
        [HideInInspector] public GameObject spawnedInstance;
    }
}