using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMap
{
    public struct MapRaycastHit
    {
        public Vector3 hitPosition;
        public MapChunk hitChunk;
        public int voxelIndex;
        public bool hit;
    }

    public static class MapUtils
    {
        static readonly Vector3 halfVec = new Vector3(0.5f, 0.5f, 0.5f);

        public static int to1D(int x, int y, int z)
        {
            int chunkSize = MapGenerator.chunkSize;
            int xMax = chunkSize;
            int yMax = chunkSize;

            return (z * xMax * yMax) + (y * xMax) + x;
        }

        public static Vector3Int to3D(int idx)
        {
            int chunkSize = MapGenerator.chunkSize;
            int xMax = chunkSize;
            int yMax = chunkSize;

            int z = idx / (xMax * yMax);
            idx -= (z * xMax * yMax);
            int y = idx / xMax;
            int x = idx % xMax;

            return new Vector3Int(x, y, z);
        }

        public static Vector3 FloorVec(Vector3 v)
        {
            return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
        }

        public static Vector3Int FloorVecToInt(Vector3 v)
        {
            return new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        }

        public static Vector3 DivideVec(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x / (v2.x + 0.001f), v1.y / (v2.y + 0.001f), v1.z / (v2.z + 0.001f));
        }

        const int maxIterations = 50;

        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out MapRaycastHit hit)
        {
            float dst = 0;
            Vector3 start = origin;
            hit = new MapRaycastHit();

            for(int i = 0; i < maxIterations; i++)
            {
                Vector3 min = FloorVec(start);
                Vector3 max = min + Vector3.one;

                Vector3 t0 = DivideVec(min - start, direction);
                Vector3 t1 = DivideVec(max - start, direction);

                Vector3 tmax = Vector3.Max(t0, t1);

                float dstB = Mathf.Min(tmax.x, tmax.y, tmax.z);

                float dstInsideBox = Mathf.Max(0, dstB);

                if(dst + dstInsideBox > maxDistance)
                {
                    break;
                }

                dst += dstInsideBox;
                start += direction * (dstInsideBox + 0.01f);

                MapChunk chunk = GetChunk(start);
                int voxIndex = chunk.GetVoxelIndex(start);

                if(chunk[voxIndex].active)
                {
                    hit.hit = true;
                    hit.hitChunk = chunk;
                    hit.voxelIndex = voxIndex;
                    hit.hitPosition = start;
                    return true;
                }
            }

            hit.hit = false;
            return false;
        }

        public static MapChunk GetChunk(Vector3 position)
        {
            Vector3 chunkPos = FloorVec(position / MapGenerator.chunkSize) * MapGenerator.chunkSize;

            return MapGenerator.instance[chunkPos];
        }
    }
}
