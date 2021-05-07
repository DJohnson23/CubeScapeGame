static float3 _CubeVertices[8] =
{
    float3(0.5f, 0.5f, 0.5f),
    float3(-0.5f, 0.5f, 0.5f),
    float3(-0.5f,-0.5f, 0.5f),
    float3(0.5f,-0.5f, 0.5f),
    float3(-0.5f, 0.5f,-0.5f),
    float3(0.5f, 0.5f,-0.5f),
    float3(0.5f,-0.5f,-0.5f),
    float3(-0.5f,-0.5f,-0.5f)
};

static int _CubeFaceTriangles[6][4] =
{
    { 0, 1, 2, 3 },
    { 5, 0, 3, 6 },
    { 4, 5, 6, 7 },
    { 1, 4, 7, 2 },
    { 5, 4, 1, 0 },
    { 3, 2, 7, 6 },
};


static float3 _CubeNormals[6] =
{
    float3(0, 0, 1),
    float3(1, 0, 0),
    float3(0, 0,-1),
    float3(-1, 0, 0),
    float3(0, 1, 0),
    float3(0,-1, 0),
};

void CubeFaceVertices(inout float3 fv[4], int dir)
{
    for (int i = 0; i < 4; i++)
    {
        fv[i] = _CubeVertices[_CubeFaceTriangles[dir][i]];
    }
}