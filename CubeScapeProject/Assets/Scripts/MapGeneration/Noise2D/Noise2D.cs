using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// src: https://www.youtube.com/watch?v=WP-Bm65Q-1Y&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=2
// Based off of Procedural Landmass Generation by Sebastian Lague

public static class Noise2D
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, Noise2DSettings settings, Vector2 additionalOffset)
    {
        return GenerateNoiseMap(mapWidth, mapHeight, settings.seed, settings.scale, settings.octaves, settings.persistance, settings.lacunarity, settings.offset + additionalOffset);
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, Noise2DSettings settings)
    {
        return GenerateNoiseMap(mapWidth, mapHeight, settings.seed, settings.scale, settings.octaves, settings.persistance, settings.lacunarity, settings.offset);
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        System.Random prng = new System.Random(seed);

        Vector2[] octaveOffsets = new Vector2[octaves];

        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        /*
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        */

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + offset.x) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight + offset.y) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                /*

                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                */

                noiseMap[x, y] = (noiseHeight + 1) / 2;
            }
        }

        /*
        Debug.Log(minNoiseHeight + " " + maxNoiseHeight);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        */

        return noiseMap;
    }

}
