using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetViewer))]
public class PlanetViewerEditor : Editor
{
    PlanetViewer planet;

    public override void OnInspectorGUI()
    {
        planet = (PlanetViewer)target;

        if(DrawDefaultInspector())
        {
            planet.RefreshPlanet();
        }

        if(GUILayout.Button("Refresh"))
        {
            planet.RefreshPlanet();
        }
    }
}
