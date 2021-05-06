using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    MapGenerator mapGen;
    Editor editor;

    bool showShapeSettings = false;

    public override void OnInspectorGUI()
    {
        mapGen = (MapGenerator)target;

        if(DrawDefaultInspector())
        {
            mapGen.RefreshMap();
        }

        ShapeSettings shapeSettings = mapGen.shapeSettings;

        if(shapeSettings != null)
        {
            DrawShapeSettings(shapeSettings);
        }

        if(GUILayout.Button("Generate"))
        {
            mapGen.RefreshMap();
        }
    }

    void DrawShapeSettings(ShapeSettings shapeSettings)
    {
        CreateCachedEditor(shapeSettings, null, ref editor);

        showShapeSettings = EditorGUILayout.Foldout(showShapeSettings, "Shape Settings");

        if (showShapeSettings)
        {
            EditorGUI.indentLevel++;

            if (editor.DrawDefaultInspector())
            {
                mapGen.RefreshMap();
            }

            EditorGUI.indentLevel--;
        }
    }
}
