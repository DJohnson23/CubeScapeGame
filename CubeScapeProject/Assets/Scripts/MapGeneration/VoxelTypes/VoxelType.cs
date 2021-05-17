using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMap
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewVoxelType", menuName = "VoxelTerrain/VoxelType")]
    public class VoxelType : ScriptableObject
    {
        public Color lightCol;
        public Color darkCol;

        public int minHeight;

        public float noiseScale;
        public Vector3 noiseCenter;

        public Texture2D CreateGradientTex(int size)
        {
            Texture2D tex = new Texture2D(size, 1);
            Color[] colors = new Color[size];

            float step = 1f / (size - 1);

            for (int i = 0; i < size; i++)
            {
                colors[i] = EvaluateCol(step * i);
            }

            tex.SetPixels(colors);
            return tex;
        }

        public Color EvaluateCol(float t)
        {
            return lightCol + t * (darkCol - lightCol);
        }
    }
}
