using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    public int mapWidth = 512;
    public int mapHeight = 512;

    [Header("Noise Settings")]
    public float noiseScale = 80f;
    [Range(1, 10)]
    public int octaves = 3;
    [Range(0, 1)]
    public float persistance = 0.3f;
    public float lacunarity = 1.8f;

    public int seed;
    public Vector2 offset;

    [Header("Height Control")]
    public float heightMultiplier = 0.05f;
    public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Terrain")]
    public Terrain terrain;
    public NormalizeMode normalizeMode;

    public bool autoRefresh;

    private void OnValidate()
    {
        if (autoRefresh)
            GenerateTerrain();

        if (mapHeight < 1) mapHeight = 1;
        if (mapWidth < 1) mapWidth = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }

    public void GenerateTerrain()
    {
        float[,] noiseMap = GenerateNoiseMap(
            mapWidth,
            mapHeight,
            seed,
            noiseScale,
            octaves,
            persistance,
            lacunarity,
            offset,
            normalizeMode
        );

        // Apply height multiplier and smoothing curve
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = heightCurve.Evaluate(noiseMap[x, y]);
                noiseMap[x, y] *= heightMultiplier;
            }
        }

        TerrainData td = terrain.terrainData;
        td.heightmapResolution = Mathf.Max(mapWidth, mapHeight);
        td.SetHeights(0, 0, noiseMap);
    }


    public static float[,] GenerateNoiseMap(
        int mapWidth,
        int mapHeight,
        int seed,
        float scale,
        int octaves,
        float persistance,
        float lacunarity,
        Vector2 offset,
        NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0)
            scale = 0.0001f;

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight) maxLocalNoiseHeight = noiseHeight;
                if (noiseHeight < minLocalNoiseHeight) minLocalNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        // Normalize
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(
                        minLocalNoiseHeight,
                        maxLocalNoiseHeight,
                        noiseMap[x, y]
                    );
                }
                else
                {
                    float normalizedHeight = Mathf.InverseLerp(
                        -maxPossibleHeight,
                        maxPossibleHeight,
                        noiseMap[x, y]
                    );

                    noiseMap[x, y] = normalizedHeight;
                }
            }
        }

        return noiseMap;
    }
}

public enum NormalizeMode { Local, Global }
