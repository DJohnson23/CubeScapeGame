using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapChunk
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

    int chunkSize;
    int chunkHeight;
    int[,] heightMap;
    Material material;
    Vector3 position;

    int numCubes;
    TerrainVoxel[] terrainVoxels;

    GameObject chunkObj;
    MeshFilter mFilter;
    MeshRenderer mRenderer;

    public MapChunk(Transform parent, int chunkSize, int chunkHeight, Material material, Vector3 position, int[,] heightMap)
    {
        this.position = position;
        this.chunkSize = chunkSize;
        this.chunkHeight = chunkHeight;
        this.material = material;
        this.heightMap = heightMap;
        chunkObj = new GameObject("Map Chunk");
        chunkObj.transform.SetParent(parent);
        chunkObj.transform.position = position;

        mFilter = chunkObj.AddComponent<MeshFilter>();
        mRenderer = chunkObj.AddComponent<MeshRenderer>();

        RefreshChunk();
    }

    public void RefreshChunk()
    {
        numCubes = chunkSize * chunkSize * chunkHeight;
        InitVoxels();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        for(int i = 0; i < terrainVoxels.Length; i++)
        {
            CreateCube(vertices, triangles, normals, terrainVoxels[i]);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        mFilter.sharedMesh = mesh;
        mRenderer.sharedMaterial = material;
    }

    void InitVoxels()
    {
        terrainVoxels = new TerrainVoxel[numCubes];
        int cubeIndex = 0;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    TerrainVoxel newVoxel;
                    newVoxel.position = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                    newVoxel.active = y <= heightMap[x, z];
                    newVoxel.coord = new DataCoordinate(x, y, z);

                    terrainVoxels[cubeIndex++] = newVoxel;
                }
            }
        }
    }

    void CreateCube(List<Vector3> vertices, List<int> triangles, List<Vector3> normals, TerrainVoxel voxel)
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
        return index.x < chunkSize && index.y < chunkHeight && index.z < chunkSize
            && index.x >= 0 && index.y >= 0 && index.z >= 0;
    }

    public int to1D(int x, int y, int z)
    {
        int yMax = chunkHeight;
        int zMax = chunkSize;

        return (x * zMax * yMax) + (y * zMax) + z;
    }

    public int[] to3D(int idx)
    {
        int yMax = chunkHeight;
        int zMax = chunkSize;

        int x = idx / (zMax * yMax);
        idx -= (x * zMax * yMax);
        int y = idx / zMax;
        int z = idx % zMax;
        return new int[] { x, y, z };
    }
}
