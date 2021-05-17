using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VoxelMap
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {
        MapGenerator mapGen;
        Editor editor;

        bool showShapeSettings = false;
        bool showVoxelTypes = false;

        Dictionary<string, bool> showType = new Dictionary<string, bool>();

        public override void OnInspectorGUI()
        {
            mapGen = (MapGenerator)target;
            if (DrawDefaultInspector() && mapGen.autoRefresh)
            {
                mapGen.RefreshMap();
            }

            ShapeSettings shapeSettings = mapGen.shapeSettings;

            if (shapeSettings != null)
            {
                DrawShapeSettings(shapeSettings);
            }

            VoxelType[] voxelTypes = mapGen.voxelTypes;

            DrawVoxelTypes(voxelTypes);

            if (GUILayout.Button("Generate"))
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

                if (editor.DrawDefaultInspector() && mapGen.autoRefresh)
                {
                    mapGen.RefreshMap();
                }

                EditorGUI.indentLevel--;
            }
        }

        void DrawVoxelTypes(VoxelType[] voxelTypes)
        {
            showVoxelTypes = EditorGUILayout.Foldout(showVoxelTypes, "Voxel Types");

            if (showVoxelTypes)
            {
                EditorGUI.indentLevel++;

                foreach (VoxelType type in voxelTypes)
                {
                    if (!showType.ContainsKey(type.name))
                    {
                        showType.Add(type.name, false);
                    }

                    bool show = showType[type.name];

                    show = EditorGUILayout.Foldout(show, type.name);

                    showType[type.name] = show;

                    if (show)
                    {
                        EditorGUI.indentLevel++;

                        CreateCachedEditor(type, null, ref editor);

                        if (editor.DrawDefaultInspector() && mapGen.autoRefresh)
                        {
                            mapGen.RefreshMap();
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
    }
}