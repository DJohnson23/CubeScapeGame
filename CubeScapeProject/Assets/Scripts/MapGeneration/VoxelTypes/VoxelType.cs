using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewVoxelType", menuName = "VoxelTerrain/VoxelType")]
public class VoxelType : ScriptableObject
{
    public Gradient colorGradient;
    public int minHeight;
    public Noise3DSettings noiseSettings;

    public Texture2D CreateGradientTex(int size)
    {
        Texture2D tex = new Texture2D(size, 1);
        Color[] colors = new Color[size];

        float step = 1f / (size - 1);

        for(int i = 0; i < size; i++)
        {
            colors[i] = colorGradient.Evaluate(step * i);
        }

        tex.SetPixels(colors);
        return tex;
    }
}
