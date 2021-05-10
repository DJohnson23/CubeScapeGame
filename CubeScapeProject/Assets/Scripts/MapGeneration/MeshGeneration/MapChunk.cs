using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapChunk
{
    struct TerrainVoxel
    {
        public int active;
        public Vector3 localPosition;
        public int typeIndex;
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

    MapGenerator mapGen;

    int chunkSize;
    int[] heightMap;
    Material material;
    Vector3 position;

    int numCubes;
    TerrainVoxel[] terrainVoxels;

    GameObject chunkObj;
    MeshFilter mFilter;
    MeshRenderer mRenderer;

    int numActive;

    ComputeShader genMeshShader;
    ComputeShader initVoxelsShader;

    public MapChunk(MapGenerator mapGen, Vector3 position, int[] heightMap, ComputeShader genMeshShader, ComputeShader initVoxelsShader)
    {
        this.mapGen = mapGen;
        this.position = position;
        this.chunkSize = mapGen.shapeSettings.chunkSize;
        this.material = mapGen.mapMaterial;
        this.heightMap = heightMap;
        this.genMeshShader = genMeshShader;
        this.initVoxelsShader = initVoxelsShader;

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
    }
    
    
    void InitVoxels()
    {
        terrainVoxels = new TerrainVoxel[numCubes];

        ComputeBuffer voxelsBuffer = new ComputeBuffer(terrainVoxels.Length, sizeof(float) * 3 + sizeof(int) * 2);
        ComputeBuffer voxelTypesBuffer = new ComputeBuffer(mapGen.voxelTypes.Length, sizeof(int) + sizeof(float) * 12);
        ComputeBuffer heightMapBuffer = new ComputeBuffer(heightMap.Length, sizeof(int));
        ComputeBuffer numActiveBuffer = new ComputeBuffer(terrainVoxels.Length, sizeof(int), ComputeBufferType.Counter);


        voxelsBuffer.SetData(terrainVoxels);
        voxelTypesBuffer.SetData(mapGen.shaderVoxelTypes);
        heightMapBuffer.SetData(heightMap);
        numActiveBuffer.SetCounterValue(0);

        initVoxelsShader.SetBuffer(0, "terrainVoxels", voxelsBuffer);
        initVoxelsShader.SetBuffer(0, "voxelTypes", voxelTypesBuffer);
        initVoxelsShader.SetBuffer(0, "heightMap", heightMapBuffer);
        initVoxelsShader.SetBuffer(0, "numActive", numActiveBuffer);
        initVoxelsShader.SetInt("chunkSize", chunkSize);
        initVoxelsShader.SetVector("position", position);
        initVoxelsShader.SetInt("numVoxelTypes", mapGen.voxelTypes.Length);

        int numGroups = Mathf.CeilToInt(chunkSize / 10f);

        initVoxelsShader.Dispatch(0, numGroups, numGroups, numGroups);

        voxelsBuffer.GetData(terrainVoxels);

        int[] args = { 0 };
        ComputeBuffer argsBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        ComputeBuffer.CopyCount(numActiveBuffer, argsBuffer, 0);
        argsBuffer.GetData(args);

        numActive = args[0];

        voxelsBuffer.Release();
        voxelTypesBuffer.Release();
        heightMapBuffer.Release();
        numActiveBuffer.Release();
        argsBuffer.Release();
    }

    void CreateMesh()
    {
        if(numActive == 0)
        {
            return;
        }

        ComputeBuffer voxelsBuffer = new ComputeBuffer(terrainVoxels.Length, sizeof(float) * 3 + sizeof(int) * 2);
        ComputeBuffer voxelTypesBuffer = new ComputeBuffer(mapGen.voxelTypes.Length, sizeof(int) + sizeof(float) * 12);

        ComputeBuffer quadListBuffer = new ComputeBuffer(numActive * 6, sizeof(float) * 19, ComputeBufferType.Append);
        quadListBuffer.SetCounterValue(0);

        voxelsBuffer.SetData(terrainVoxels);
        voxelTypesBuffer.SetData(mapGen.shaderVoxelTypes);

        genMeshShader.SetBuffer(0, "voxels", voxelsBuffer);
        genMeshShader.SetBuffer(0, "voxelTypes", voxelTypesBuffer);
        genMeshShader.SetBuffer(0, "quadList", quadListBuffer);
        genMeshShader.SetInt("chunkSize", chunkSize);
        genMeshShader.SetVector("position", position);

        int numGroups = Mathf.CeilToInt(chunkSize / 10f);

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

        Vector3[] vertices = new Vector3[quadList.Length * 4];
        Vector3[] normals = new Vector3[vertices.Length];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[quadList.Length * 6];

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

        argsBuffer.Release();
        voxelsBuffer.Release();
        voxelTypesBuffer.Release();
        quadListBuffer.Release();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.normals = normals;
        mesh.colors = colors;

        mFilter.sharedMesh = mesh;
        mRenderer.sharedMaterial = material;
    }
}
