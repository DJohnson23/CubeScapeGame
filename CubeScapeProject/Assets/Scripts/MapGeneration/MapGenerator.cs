using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    struct DataCoordinate
    {
        public int x;
        public int y;
        public int z;

        public DataCoordinate(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    struct TerrainVoxel
    {
        public bool active;
        public Vector3 position;
        public DataCoordinate coord;
    }

    static DataCoordinate[] neighborOffsets =
    {
        new DataCoordinate( 0, 0, 1),
        new DataCoordinate( 1, 0, 0),
        new DataCoordinate( 0, 0,-1),
        new DataCoordinate(-1, 0, 0),
        new DataCoordinate( 0, 1, 0),
        new DataCoordinate( 0,-1, 0)
    };

    public Vector3Int mapSize = new Vector3Int(5, 5, 5);
    public Material mapMat;
    public float radius = 5;

    int numCubes;
    TerrainVoxel[] terrainVoxels;

    private void Start()
    {
        RefreshMap();
    }

    public void RefreshMap()
    {
        foreach(Transform t in transform)
        {
            DestroyImmediate(t.gameObject);
        }

        numCubes = mapSize.x * mapSize.y * mapSize.z;
        InitVoxels();

        GameObject newMap = new GameObject("Voxel Map");
        newMap.transform.SetParent(transform);

        MeshFilter meshFilter = newMap.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newMap.AddComponent<MeshRenderer>();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        for(int i = 0; i < terrainVoxels.Length; i++)
        {
            CreateVoxel(vertices, triangles, normals, terrainVoxels[i]);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = mapMat;
    }

    void InitVoxels()
    {
        terrainVoxels = new TerrainVoxel[numCubes];
        int cubeIndex = 0;

        Vector3 halfMapSize = new Vector3(mapSize.x / 2.0f, mapSize.y / 2.0f, mapSize.z / 2.0f);
        float sqrRadius = radius * radius;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    TerrainVoxel newVoxel;
                    newVoxel.position = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) - halfMapSize;
                    newVoxel.active = newVoxel.position.sqrMagnitude <= sqrRadius;
                    newVoxel.coord = new DataCoordinate(x, y, z);

                    terrainVoxels[cubeIndex++] = newVoxel;
                }
            }
        }
    }

    void CreateVoxel(List<Vector3> vertices, List<int> triangles, List<Vector3> normals, TerrainVoxel voxel)
    {
        if(!voxel.active)
        {
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            Vector3[] faceVerts = CubeMeshData.FaceVertices(i);
            Vector3 n = CubeMeshData.normals[i];

            DataCoordinate offsetCoord = neighborOffsets[i];
            DataCoordinate neighborCoord = new DataCoordinate(voxel.coord.x + offsetCoord.x, voxel.coord.y + offsetCoord.y, voxel.coord.z + offsetCoord.z);

            if(IsValidCoord(neighborCoord))
            {
                TerrainVoxel adjacent = terrainVoxels[to1D(neighborCoord.x, neighborCoord.y, neighborCoord.z)];

                if (adjacent.active)
                {
                    continue;
                }
            }

            vertices.Add(faceVerts[0] + voxel.position);
            vertices.Add(faceVerts[1] + voxel.position);
            vertices.Add(faceVerts[2] + voxel.position);
            vertices.Add(faceVerts[3] + voxel.position);

            normals.Add(n);
            normals.Add(n);
            normals.Add(n);
            normals.Add(n);

            triangles.Add(vertices.Count - 4);
            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 2);

            triangles.Add(vertices.Count - 4);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);
        }

    }

    bool IsValidCoord(DataCoordinate index)
    {
        return index.x < mapSize.x && index.y < mapSize.y && index.z < mapSize.z
            && index.x >= 0 && index.y >= 0 && index.z >= 0;
    }

    public int to1D(int x, int y, int z)
    {
        int zMax = mapSize.z;
        int yMax = mapSize.y;

        return (x * zMax * yMax) + (y * zMax) + z;
    }

    public int[] to3D(int idx)
    {
        int x = idx / (mapSize.z * mapSize.y);
        idx -= (x * mapSize.z * mapSize.y);
        int y = idx / mapSize.z;
        int z = idx % mapSize.z;
        return new int[] { x, y, z };
    }
}
