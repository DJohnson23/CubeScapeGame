int to1D(int x, int y, int z, int chunkSize)
{
    int xMax = chunkSize;
    int yMax = chunkSize;

    return (z * xMax * yMax) + (y * xMax) + x;
}

int3 to3D(int idx, int chunkSize)
{
    int xMax = chunkSize;
    int yMax = chunkSize;

    int z = idx / (xMax * yMax);
    idx -= (z * xMax * yMax);
    int y = idx / xMax;
    int x = idx % xMax;

    return int3(x, y, z);
}