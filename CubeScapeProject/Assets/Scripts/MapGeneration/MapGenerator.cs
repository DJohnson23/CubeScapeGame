using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public ShapeSettings shapeSettings;

    public Material mapMaterial;
    public int numChunks = 5;

    MapChunk[] chunks;

    NoiseFilter[] noiseFilters;

    int worldSize;

    private void Start()
    {
        RefreshMap();
    }
    public void RefreshMap()
    {
        worldSize = shapeSettings.chunkSize * shapeSettings.worldChunks;
        noiseFilters = new NoiseFilter[shapeSettings.noiseLayers.Length];

        for(int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = new NoiseFilter(shapeSettings.noiseLayers[i].noiseSettings);
        }

        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        chunks = new MapChunk[numChunks * numChunks];

        int index = 0;

        for(int x = 0; x < numChunks; x++)
        {
            for(int z = 0; z < numChunks; z++)
            {
                Vector3 position = new Vector3(x * shapeSettings.chunkSize, 0, z * shapeSettings.chunkSize);
                int[,] heightMap = GenerateHeightMap(shapeSettings.chunkSize, shapeSettings.chunkSize, x * shapeSettings.chunkSize, z * shapeSettings.chunkSize);

                chunks[index++] = new MapChunk(transform, shapeSettings.chunkSize, shapeSettings.chunkHeight, mapMaterial, position, heightMap);
            }
        }

    }

    int[,] GenerateHeightMap(int mapWidth, int mapHeight, int xOffset, int yOffset)
    {
        int[,] heightMap = new int[mapWidth, mapHeight];

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                heightMap[x, y] = GetHeight(x + xOffset, y + yOffset);
            }
        }

        return heightMap;
    }

    int GetHeight(int x, int y)
    {
        float firstLayerValue = 0;
        Vector3 simPoint = Simulate3D(x, y);
        float height = 0;

        if(noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].Evaluate(simPoint);

            if(shapeSettings.noiseLayers[0].enabled)
            {
                height = firstLayerValue;
            }
        }

        for(int i = 1; i < noiseFilters.Length; i++)
        {
            if(shapeSettings.noiseLayers[i].enabled)
            {
                float mask = (shapeSettings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                height += noiseFilters[i].Evaluate(simPoint) * mask;
            }

        }

        return (int)height;
    }

    Vector3 Simulate3D(int x, int z)
    {

        float latitude = (float)x / worldSize * 2 * Mathf.PI;
        float longitude = (float)z / worldSize * 2 * Mathf.PI;
        float radius = worldSize / (2 * Mathf.PI);

        Vector3 point = new Vector3();
        point.x = radius * Mathf.Cos(latitude) * Mathf.Cos(longitude);
        point.y = radius * Mathf.Sin(latitude);
        point.z = radius * Mathf.Cos(latitude) * Mathf.Sin(longitude);

        return point;
    }
}
