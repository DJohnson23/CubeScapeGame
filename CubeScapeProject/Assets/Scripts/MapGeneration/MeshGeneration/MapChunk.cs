using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class MapChunk
{

    struct TerrainVoxel
    {
        public bool active;
        public Vector3 localPosition;
        public Vector3Int coord;
        public int typeIndex;
    }

    static Vector3Int[] neighborOffsets =
    {
        new Vector3Int( 0, 0, 1),
        new Vector3Int( 1, 0, 0),
        new Vector3Int( 0, 0,-1),
        new Vector3Int(-1, 0, 0),
        new Vector3Int( 0, 1, 0),
        new Vector3Int( 0,-1, 0)
    };

    MapGenerator mapGen;

    int chunkSize;
    int[,] heightMap;
    Material material;
    Vector3 position;

    int numCubes;
    TerrainVoxel[] terrainVoxels;

    GameObject chunkObj;
    MeshFilter mFilter;
    MeshRenderer mRenderer;

    List<Vector3> vertices;

    List<int> triangles;
    List<Vector3> normals;
    List<Color> colors;

    public MapChunk(MapGenerator mapGen, Vector3 position, int[,] heightMap)
    {
        this.mapGen = mapGen;
        this.position = position;
        this.chunkSize = mapGen.shapeSettings.chunkSize;
        this.material = mapGen.mapMaterial;
        this.heightMap = heightMap;
        chunkObj = new GameObject("MapChunk");
        chunkObj.transform.SetParent(mapGen.transform);
        chunkObj.transform.position = position;

        mFilter = chunkObj.AddComponent<MeshFilter>();
        mRenderer = chunkObj.AddComponent<MeshRenderer>();

        RefreshChunk();
    }

    public void RefreshChunk()
    {
        numCubes = chunkSize * chunkSize * chunkSize;
        InitVoxels();

        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
        colors = new List<Color>();

        Stopwatch sw = new Stopwatch();

        sw.Start();

        for(int i = 0; i < terrainVoxels.Length; i++)
        {
            CreateCube(terrainVoxels[i]);
        }

        sw.Stop();

        UnityEngine.Debug.Log("CreateCubes: " + sw.Elapsed);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = mapGen.voxelTypes.Length;
        mesh.triangles = triangles.ToArray();

        mesh.normals = normals.ToArray();
        mesh.colors = colors.ToArray();

        mFilter.sharedMesh = mesh;
        mRenderer.sharedMaterial = material;
    }

    void InitVoxels()
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();

        terrainVoxels = new TerrainVoxel[numCubes];
        int cubeIndex = 0;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float worldY = y + position.y;

                    TerrainVoxel newVoxel;
                    newVoxel.localPosition = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                    newVoxel.active = worldY < heightMap[x, z];
                    newVoxel.coord = new Vector3Int(x, y, z);

                    int typeIndex = 0;

                    while(typeIndex < mapGen.voxelTypes.Length - 1 && mapGen.voxelTypes[typeIndex + 1].minHeight < worldY)
                    {
                        typeIndex++;
                    }

                    newVoxel.typeIndex = typeIndex;

                    terrainVoxels[cubeIndex++] = newVoxel;
                }
            }
        }

        sw.Stop();

        UnityEngine.Debug.Log("InitVoxels: " + sw.Elapsed);
    }

    void CreateCube(TerrainVoxel voxel)
    {
        if(!voxel.active)
        {
            return;
        }

        float colorNoiseVal = mapGen.voxColNoiseFilters[voxel.typeIndex].Evaluate(voxel.localPosition + position);
        Color voxCol = mapGen.voxelTypes[voxel.typeIndex].colorGradient.Evaluate(colorNoiseVal);

        for (int i = 0; i < 6; i++)
        {
            Vector3[] faceVerts = CubeMeshData.FaceVertices(i);
            Vector3 n = CubeMeshData.normals[i];

            Vector3Int offsetCoord = neighborOffsets[i];
            Vector3Int neighborCoord = new Vector3Int(voxel.coord.x + offsetCoord.x, voxel.coord.y + offsetCoord.y, voxel.coord.z + offsetCoord.z);

            if(IsValidCoord(neighborCoord))
            {
                TerrainVoxel adjacent = terrainVoxels[to1D(neighborCoord.x, neighborCoord.y, neighborCoord.z)];

                if (adjacent.active)
                {
                    continue;
                }
            }

            vertices.Add(faceVerts[0] + voxel.localPosition);
            vertices.Add(faceVerts[1] + voxel.localPosition);
            vertices.Add(faceVerts[2] + voxel.localPosition);
            vertices.Add(faceVerts[3] + voxel.localPosition);

            normals.Add(n);
            normals.Add(n);
            normals.Add(n);
            normals.Add(n);

            colors.Add(voxCol);
            colors.Add(voxCol);
            colors.Add(voxCol);
            colors.Add(voxCol);

            triangles.Add(vertices.Count - 4);
            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 2);

            triangles.Add(vertices.Count - 4);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);

        }

    }

    bool IsValidCoord(Vector3Int index)
    {
        return index.x < chunkSize && index.y < chunkSize && index.z < chunkSize
            && index.x >= 0 && index.y >= 0 && index.z >= 0;
    }

    public int to1D(int x, int y, int z)
    {
        int yMax = chunkSize;
        int zMax = chunkSize;

        return (x * zMax * yMax) + (y * zMax) + z;
    }

    public int[] to3D(int idx)
    {
        int yMax = chunkSize;
        int zMax = chunkSize;

        int x = idx / (zMax * yMax);
        idx -= (x * zMax * yMax);
        int y = idx / zMax;
        int z = idx % zMax;
        return new int[] { x, y, z };
    }
}
