using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public bool autoRefresh = true;
    public bool generateDisplay;
    public ShapeSettings shapeSettings;

    public Material mapMaterial;
    public int numChunks = 5;

    public VoxelType[] voxelTypes;

    public Noise3DFilter[] voxColNoiseFilters { get; private set; }

    MapChunk[] chunks;

    int worldSize;

    private void Start()
    {
        RefreshMap();
    }

    public void RefreshMap()
    {
        worldSize = shapeSettings.chunkSize * shapeSettings.worldChunks;

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if(generateDisplay)
        {
            float[,] noiseMap = Noise2D.GenerateNoiseMap(worldSize, worldSize, shapeSettings.noiseSettings);
            display.textureRenderer.gameObject.SetActive(true);
            display.DrawNoiseMap(noiseMap);
        }
        else
        {
            display.textureRenderer.gameObject.SetActive(false);
        }

        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        chunks = new MapChunk[numChunks * numChunks * numChunks];

        voxColNoiseFilters = new Noise3DFilter[voxelTypes.Length];

        for(int i = 0; i < voxColNoiseFilters.Length; i++)
        {
            voxColNoiseFilters[i] = new Noise3DFilter(voxelTypes[i].noiseSettings);
        }

        int index = 0;

        int halfNum = numChunks / 2;

        for(int x = -halfNum; x < numChunks - halfNum; x++)
        {
            for(int z = -halfNum; z < numChunks - halfNum; z++)
            {
                int[,] heightMap = GenerateHeightMap(shapeSettings.chunkSize, shapeSettings.chunkSize, new Vector2(x * shapeSettings.chunkSize, z * shapeSettings.chunkSize));

                for(int y = 0; y < numChunks; y++)
                {
                    Vector3 position = new Vector3(x * shapeSettings.chunkSize, y * shapeSettings.chunkSize, z * shapeSettings.chunkSize);
                    chunks[index++] = new MapChunk(this, position, heightMap);
                }
            }
        }

    }

    int[,] GenerateHeightMap(float[,] noiseMap, int width, int height, Vector2 offset)
    {
        int[,] heightMap = new int[width, height];

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                int sampleX = x + (int)offset.x;
                int sampleY = y + (int)offset.y;
                heightMap[x, y] = Mathf.RoundToInt(noiseMap[sampleX, sampleY] * shapeSettings.maxSurfaceHeight);
            }
        }

        return heightMap;
    }

    int[,] GenerateHeightMap(int width, int height, Vector2 offset)
    {
        float[,] noiseMap = Noise2D.GenerateNoiseMap(width, height, shapeSettings.noiseSettings, offset);
        int[,] heightMap = new int[width, height];

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                int val = Mathf.RoundToInt(Mathf.Lerp(shapeSettings.minSurfaceHeight, shapeSettings.maxSurfaceHeight, noiseMap[x, y]));
                heightMap[x, y] = val;
            }
        }

        return heightMap;
    }
}
