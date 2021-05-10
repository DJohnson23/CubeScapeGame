using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public struct ShaderVoxelType
{
    public int minHeight;
    public float scale;
    public Vector3 center;
    public Vector4 lightCol;
    public Vector4 darkCol;
}

public class MapGenerator : MonoBehaviour
{
    public ComputeShader meshGenerationShader;
    public ComputeShader initVoxelsShader;
    public bool autoRefresh = true;
    public bool generateDisplay;
    public ShapeSettings shapeSettings;

    public Material mapMaterial;
    public int numChunks = 5;

    public VoxelType[] voxelTypes;
    public ShaderVoxelType[] shaderVoxelTypes { get; private set; }

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

        shaderVoxelTypes = new ShaderVoxelType[voxelTypes.Length];

        for (int i = 0; i < voxelTypes.Length; i++)
        {
            VoxelType voxType = voxelTypes[i];

            ShaderVoxelType newType = new ShaderVoxelType();
            newType.minHeight = voxType.minHeight;
            newType.scale = voxType.noiseSettings.scale;
            newType.center = voxType.noiseSettings.center;
            newType.lightCol = voxType.lightCol;
            newType.darkCol = voxType.darkCol;

            shaderVoxelTypes[i] = newType;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
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

        Stopwatch sw = new Stopwatch();

        sw.Start();

        for(int x = -halfNum; x < numChunks - halfNum; x++)
        {
            for(int z = -halfNum; z < numChunks - halfNum; z++)
            {
                int[] heightMap = GenerateHeightMap(shapeSettings.chunkSize, shapeSettings.chunkSize, new Vector2(x * shapeSettings.chunkSize, z * shapeSettings.chunkSize));

                for(int y = 0; y < numChunks; y++)
                {
                    Vector3 position = new Vector3(x * shapeSettings.chunkSize, y * shapeSettings.chunkSize, z * shapeSettings.chunkSize);
                    chunks[index++] = new MapChunk(this, position, heightMap, meshGenerationShader, initVoxelsShader);
                }
            }
        }

        sw.Stop();

        UnityEngine.Debug.Log("Generation Loop: " + sw.Elapsed);

    }

    int[] GenerateHeightMap(float[,] noiseMap, int width, int height, Vector2 offset)
    {
        int[] heightMap = new int[width * height];

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                int sampleX = x + (int)offset.x;
                int sampleY = y + (int)offset.y;
                heightMap[y * width + x] = Mathf.RoundToInt(noiseMap[sampleX, sampleY] * shapeSettings.maxSurfaceHeight);
            }
        }

        return heightMap;
    }

    int[] GenerateHeightMap(int width, int height, Vector2 offset)
    {
        float[,] noiseMap = Noise2D.GenerateNoiseMap(width, height, shapeSettings.noiseSettings, offset);
        int[] heightMap = new int[width * height];

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                int val = Mathf.RoundToInt(Mathf.Lerp(shapeSettings.minSurfaceHeight, shapeSettings.maxSurfaceHeight, noiseMap[x, y]));
                heightMap[y * width + x] = val;
            }
        }

        return heightMap;
    }
}
