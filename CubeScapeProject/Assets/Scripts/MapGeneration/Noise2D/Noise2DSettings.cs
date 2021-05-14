using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Noise2DSettings
{
    public float scale = 5;
    public int octaves = 5;
    public float persistance = 0.5f;
    public float lacunarity = 2f;
    public int seed = 1234;
    public Vector2 offset = new Vector2();
}