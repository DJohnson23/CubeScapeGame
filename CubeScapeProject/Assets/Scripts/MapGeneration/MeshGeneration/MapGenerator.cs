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
    public const int chunkSize = 16;

    public bool autoRefresh = true;
    public ComputeShader meshGenerationShader;
    public ComputeShader initVoxelsShader;
    public ShapeSettings shapeSettings;

    public Material mapMaterial;
    public int numChunks = 5;

    public VoxelType[] voxelTypes;
    public ShaderVoxelType[] shaderVoxelTypes { get; private set; }

    Dictionary<Vector3, MapChunk> mapChunks;

    int worldSize;

    private void Start()
    {
        RefreshMap();
    }

    public void RefreshMap()
    {
        worldSize = chunkSize * shapeSettings.worldChunks;

        shaderVoxelTypes = new ShaderVoxelType[voxelTypes.Length];

        for (int i = 0; i < voxelTypes.Length; i++)
        {
            VoxelType voxType = voxelTypes[i];

            ShaderVoxelType newType = new ShaderVoxelType();
            newType.minHeight = voxType.minHeight;
            newType.scale = voxType.noiseScale;
            newType.center = voxType.noiseCenter;
            newType.lightCol = voxType.lightCol;
            newType.darkCol = voxType.darkCol;

            shaderVoxelTypes[i] = newType;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        mapChunks = new Dictionary<Vector3, MapChunk>();

        int index = 0;

        int halfNum = numChunks / 2;

        Stopwatch sw = new Stopwatch();

        sw.Start();

        for(int x = -halfNum; x < numChunks - halfNum; x++)
        {
            for(int z = -halfNum; z < numChunks - halfNum; z++)
            {
                int[] heightMap = GenerateHeightMap(chunkSize, chunkSize, new Vector2(x * chunkSize, z * chunkSize));

                for(int y = 0; y < numChunks; y++)
                {
                    Vector3 position = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
                    MapChunk newChunk = new MapChunk(this, position, heightMap, meshGenerationShader, initVoxelsShader);
                    mapChunks.Add(position, newChunk);
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
