using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class MapChunk
{

    struct TerrainVoxel
    {
        public int active;
        public Vector3 localPosition;
        public Vector3Int coord;
        public int typeIndex;
        public Vector4 color;
    }

    struct ShaderVoxelType
    {
        public int minHeight;
        public float scale;
        public Vector3 center;
    }

    struct Quad
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Vector3 d;

        public Vector3 normal;
        public Vector4 color;
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

    Vector3[] vertices;

    int[] triangles;
    Vector3[] normals;
    Color[] colors;

    int numActive;

    ComputeShader genMeshShader;

    public MapChunk(MapGenerator mapGen, Vector3 position, int[,] heightMap, ComputeShader genMeshShader)
    {
        this.mapGen = mapGen;
        this.position = position;
        this.chunkSize = mapGen.shapeSettings.chunkSize;
        this.material = mapGen.mapMaterial;
        this.heightMap = heightMap;
        this.genMeshShader = genMeshShader;

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

        CreateMesh();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.normals = normals;
        mesh.colors = colors;

        mFilter.sharedMesh = mesh;
        mRenderer.sharedMaterial = material;
    }

    void InitVoxels()
    {
        numActive = 0;

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
                    newVoxel.active = worldY < heightMap[x, z] ? 1 : 0;

                    if(newVoxel.active == 1)
                    {
                        numActive++;
                    }

                    newVoxel.coord = new Vector3Int(x, y, z);

                    int typeIndex = 0;

                    while(typeIndex < mapGen.voxelTypes.Length - 1 && mapGen.voxelTypes[typeIndex + 1].minHeight < worldY)
                    {
                        typeIndex++;
                    }

                    newVoxel.typeIndex = typeIndex;

                    float colorNoiseVal = mapGen.voxColNoiseFilters[newVoxel.typeIndex].Evaluate(newVoxel.localPosition + position);
                    newVoxel.color = mapGen.voxelTypes[newVoxel.typeIndex].colorGradient.Evaluate(colorNoiseVal);

                    terrainVoxels[cubeIndex++] = newVoxel;
                }
            }
        }
    }

    void CreateMesh()
    {
        if(numActive == 0)
        {
            return;
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();

        ComputeBuffer voxelsBuffer = new ComputeBuffer(terrainVoxels.Length, sizeof(int) + sizeof(float) * 7 + sizeof(int) * 4);
        ComputeBuffer voxelTypesBuffer = new ComputeBuffer(mapGen.voxelTypes.Length, sizeof(int) + sizeof(float) * 4);

        ComputeBuffer quadListBuffer = new ComputeBuffer(numActive * 6, sizeof(float) * 19, ComputeBufferType.Append);
        quadListBuffer.SetCounterValue(0);

        ShaderVoxelType[] voxelTypes = new ShaderVoxelType[mapGen.voxelTypes.Length];

        for(int i = 0; i < voxelTypes.Length; i++)
        {
            VoxelType voxType = mapGen.voxelTypes[i];
            
            ShaderVoxelType newType = new ShaderVoxelType();
            newType.minHeight = voxType.minHeight;
            newType.scale = voxType.noiseSettings.scale;
            newType.center = voxType.noiseSettings.center;

            voxelTypes[i] = newType;
        }

        voxelsBuffer.SetData(terrainVoxels);
        voxelTypesBuffer.SetData(voxelTypes);

        genMeshShader.SetBuffer(0, "voxels", voxelsBuffer);
        genMeshShader.SetBuffer(0, "voxelTypes", voxelTypesBuffer);
        genMeshShader.SetBuffer(0, "quadList", quadListBuffer);
        genMeshShader.SetInt("chunkSize", chunkSize);

        int numGroups = Mathf.CeilToInt(chunkSize / 8f);

        genMeshShader.Dispatch(0, numGroups, numGroups, numGroups);

        int[] args = { 0 };
        ComputeBuffer argsBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        ComputeBuffer.CopyCount(quadListBuffer, argsBuffer, 0);
        argsBuffer.GetData(args);

        Quad[] quadList = new Quad[args[0]];
        quadListBuffer.GetData(quadList);

        int v = 0;
        int t = 0;

        Stopwatch sw2 = new Stopwatch();

        sw2.Start();

        vertices = new Vector3[quadList.Length * 4];
        normals = new Vector3[vertices.Length];
        colors = new Color[vertices.Length];
        triangles = new int[quadList.Length * 6];

        foreach(Quad quad in quadList)
        {
            vertices[v    ] = quad.a;
            vertices[v + 1] = quad.b;
            vertices[v + 2] = quad.c;
            vertices[v + 3] = quad.d;

            normals[v    ] = quad.normal;
            normals[v + 1] = quad.normal;
            normals[v + 2] = quad.normal;
            normals[v + 3] = quad.normal;

            colors[v    ] = quad.color;
            colors[v + 1] = quad.color;
            colors[v + 2] = quad.color;
            colors[v + 3] = quad.color;

            triangles[t++] = v;
            triangles[t++] = v + 1;
            triangles[t++] = v + 2;

            triangles[t++] = v;
            triangles[t++] = v + 2;
            triangles[t++] = v + 3;

            v += 4;
        }

        sw2.Stop();

        UnityEngine.Debug.Log("sw2: " + sw2.Elapsed);

        argsBuffer.Release();
        voxelsBuffer.Release();
        voxelTypesBuffer.Release();
        quadListBuffer.Release();

        sw.Stop();

        UnityEngine.Debug.Log("sw: " + sw.Elapsed);
    }
}
