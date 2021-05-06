using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PlanetViewer : MonoBehaviour
{
    public float radius = 1;
    public Material planetMat;

    [Min(3)]
    public int heightSegments;
    [Min(3)]
    public int sections;

    public Transform target;

    float yRot = 0;
    float zRot = 0;

    public void RefreshPlanet()
    {
        CreateSphere();
    }

    private void Update()
    {
        FaceTarget();
    }

    void FaceTarget()
    {
        Vector3 offset = target.position - transform.position;
        Vector3 flat = offset;
        flat.y = 0;
        float worldDot = Vector3.Dot(offset, Vector3.left);

        yRot = -Mathf.Atan(offset.z / (offset.x + 0.001f)) * Mathf.Rad2Deg;

        zRot = -Mathf.Atan(offset.y / (flat.magnitude + 0.0001f)) * Mathf.Rad2Deg;

        if(worldDot <= 0)
        {
            yRot += 180;
        }

        Debug.Log(zRot);

        transform.rotation = Quaternion.Euler(0, yRot, zRot);

        float tx = (yRot + 90) / 360f;
        float ty = (zRot + 90) / 180f;
        planetMat.SetVector("_Offset", new Vector2(-tx, -ty));
    }

    void CreateSphere()
    {
        Vector3[] vertices = new Vector3[sections * heightSegments + heightSegments + sections - 1];
        int[] triangles = new int[6 * sections * (heightSegments - 1)];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];

        float sqrR = radius * radius;

        int v = 0;

        float sectionStep = 1.0f / sections;
        float halfStep = sectionStep / 2;

        for(int i = 0; i < sections; i++)
        {
            vertices[v] = new Vector3(0, -radius, 0);
            normals[v] = new Vector3(0, -1, 0);
            uvs[v] = new Vector2(sectionStep * i + halfStep, 0);
            v++;
        }

        int t = 0;

        for (int i = 0; i < sections; i++)
        {
            triangles[t++] = i;
            triangles[t++] = i + sections;
            triangles[t++] = i + sections + 1;
        }

        float pi2 = Mathf.PI * 2;
        float halfPi = Mathf.PI / 2;

        float yAngleIncr = Mathf.PI / heightSegments;
        float yAngle = - halfPi + yAngleIncr;
        float xzAngle;
        float xzAngleIncr = pi2 / sections;

        for (int i = 1; i < heightSegments; i++)
        {
            float y = radius * Mathf.Sin(yAngle);
            yAngle += yAngleIncr;

            float curRadius = Mathf.Sqrt(sqrR - y * y);
            xzAngle = 0;

            for(int j = 0; j < sections + 1; j++)
            {
                float x = curRadius * Mathf.Cos(xzAngle);
                float z = curRadius * Mathf.Sin(xzAngle);

                vertices[v] = new Vector3(x, y, z);
                normals[v] = vertices[v].normalized;
                uvs[v] = new Vector2((float)j / sections, (float)i / heightSegments / 2);

                xzAngle += xzAngleIncr;

                if(i != heightSegments - 1 && j != sections)
                {
                    int v1 = v + sections + 1;
                    int v2 = v1 + 1;
                    int v3 = v + 1;

                    triangles[t++] = v;
                    triangles[t++] = v1;
                    triangles[t++] = v2;

                    triangles[t++] = v;
                    triangles[t++] = v2;
                    triangles[t++] = v3;
                }

                v++;
            }
        }

        for(int i = 0; i < sections; i++)
        {
            vertices[v] = new Vector3(0, radius, 0);
            normals[v] = new Vector3(0, 1, 0);
            uvs[v] = new Vector2(sectionStep * i + halfStep, 0.5f);
            v++;
        }

        
        for(int i = sections; i > 0; i--)
        {
            int v0 = vertices.Length - i;
            int v1 = v0 - sections;
            int v2 = v1 - 1;

            triangles[t++] = v0;
            triangles[t++] = v1;
            triangles[t++] = v2;
        }
        

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = planetMat;
    }
}
