using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{

    const float scale = 5f;

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDst;

    public Transform viewer;
    public Material mapMaterial;

    public string terrainLayerName = "Terrain";

    [Header("Spawning Configuration")]
    public PlantScriptable[] spawnableObjects;

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

        UpdateVisibleChunks();
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
        if (layerIndex == -1)
        {
            // Debug.LogError("Warstwa '" + terrainLayerName + "' nie istnieje! Używam Default.");
            layerIndex = 0;
        }

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
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, layerIndex, spawnableObjects));
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

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        Transform objectsParent;
        bool objectsSpawned = false;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material, int layerIndex, PlantScriptable[] spawnSettings)
        {
            this.detailLevels = detailLevels;
            this.spawnSettings = spawnSettings;

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

        void SpawnObjects()
        {
            objectsSpawned = true;


            if (spawnSettings == null || spawnSettings.Length == 0) return;

            int width = mapData.heightMap.GetLength(0);
            int height = mapData.heightMap.GetLength(1);


            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;

            System.Random prng = new System.Random(position.GetHashCode());


            int step = 2;

            for (int y = 0; y < height; y += step)
            {
                for (int x = 0; x < width; x += step)
                {

                    float currentHeight = mapData.heightMap[x, y];

                    for (int i = 0; i < spawnSettings.Length; i++)
                    {
                        PlantScriptable plant = spawnSettings[i];

                        if (plant == null || plant.prefab == null) continue;

                        if (currentHeight >= plant.minHeight && currentHeight <= plant.maxHeight)
                        {

                            if (prng.NextDouble() < plant.density)
                            {

                                float heightMultiplier = mapGenerator.meshHeightMultiplier;
                                AnimationCurve heightCurve = new AnimationCurve(mapGenerator.meshHeightCurve.keys);
                                float localY = heightCurve.Evaluate(currentHeight) * heightMultiplier;

                                // Wyznaczamy pozycję
                                Vector3 localPos = new Vector3(topLeftX + x, localY, topLeftZ - y);

                                // Dodajemy małe losowe przesunięcie (Jitter), żeby zbić efekt siatki
                                float randomOffsetX = (float)prng.NextDouble() * step - (step / 2f);
                                float randomOffsetZ = (float)prng.NextDouble() * step - (step / 2f);
                                localPos.x += randomOffsetX;
                                localPos.z += randomOffsetZ;

                                // Tworzymy obiekt
                                GameObject go = Object.Instantiate(plant.prefab, objectsParent);
                                go.transform.localPosition = localPos;

                                // Losowa rotacja wokół osi Y
                                go.transform.localRotation = Quaternion.Euler(0, (float)prng.Next(0, 360), 0);

                                // Losowa skala z zakresu zdefiniowanego w ScriptableObject
                                float randomScale = Mathf.Lerp(plant.minScale, plant.maxScale, (float)prng.NextDouble());
                                go.transform.localScale = Vector3.one * randomScale;

                                // Jeśli coś zaspawnowaliśmy w tym punkcie, przerywamy pętlę roślin 
                                // (żeby w tym samym miejscu nie powstały dwa obiekty na raz)
                                goto NextPosition;
                            }
                        }
                    }

                NextPosition:;
                }
            }
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
}