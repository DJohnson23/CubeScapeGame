using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CubeMeshData
{
    public enum CubeNormal
    {
        North   = 0,
        East    = 1,
        South   = 2,
        West    = 3,
        Up      = 4,
        Down    = 5
    }

    public static readonly Vector3[] vertices =
    {
        new Vector3( 0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f,-0.5f, 0.5f),
        new Vector3( 0.5f,-0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f,-0.5f),
        new Vector3( 0.5f, 0.5f,-0.5f),
        new Vector3( 0.5f,-0.5f,-0.5f),
        new Vector3(-0.5f,-0.5f,-0.5f)
    };

    public static readonly int[][] faceTriangles =
    {
        new int[] { 0, 1, 2, 3 },
        new int[] { 5, 0, 3, 6 },
        new int[] { 4, 5, 6, 7 },
        new int[] { 1, 4, 7, 2 },
        new int[] { 5, 4, 1, 0 },
        new int[] { 3, 2, 7, 6 },
    };

    public static readonly Vector3[] normals =
    {
        new Vector3( 0, 0, 1),
        new Vector3( 1, 0, 0),
        new Vector3( 0, 0,-1),
        new Vector3(-1, 0, 0),
        new Vector3( 0, 1, 0),
        new Vector3( 0,-1, 0),
    };

    public static Vector3[] FaceVertices(int dir)
    {
        Vector3[] fv = new Vector3[4];

        for(int i = 0; i < fv.Length; i++)
        {
            fv[i] = vertices[faceTriangles[dir][i]];
        }

        return fv;
    }
}
