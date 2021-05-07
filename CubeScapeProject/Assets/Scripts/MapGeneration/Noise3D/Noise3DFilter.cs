using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise3DFilter
{
    Noise3DSettings settings;
    Noise3D noise = new Noise3D();

    public Noise3DFilter(Noise3DSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = (noise.Evaluate(point * (settings.scale / 100) + settings.center) + 1) * 0.5f;
        return noiseValue;
    }
}
