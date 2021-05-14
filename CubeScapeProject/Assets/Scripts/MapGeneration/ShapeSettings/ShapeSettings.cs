using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewShapeSettings", menuName = "VoxelTerrain/ShapeSettings")]
public class ShapeSettings : ScriptableObject
{
    public Noise2DSettings noiseSettings;
    public int maxSurfaceHeight = 2;
    public int minSurfaceHeight = 1;
    public int worldChunks = 10;
}
