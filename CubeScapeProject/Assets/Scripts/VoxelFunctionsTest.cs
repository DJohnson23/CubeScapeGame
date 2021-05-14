using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelFunctionsTest : MonoBehaviour
{
    Vector3 FloorVec(Vector3 v)
    {
        return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
    }

    Vector3 DivideVec(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x / (v2.x + 0.001f), v1.y / (v2.y + 0.001f), v1.z / (v2.z + 0.001f));
    }

    Vector3 RaycastTest(Vector3 origin, Vector3 dir)
    {
        Vector3 min = FloorVec(origin);
        Vector3 max = min + Vector3.one;

        Gizmos.DrawWireCube(min + Vector3.one * 0.5f, Vector3.one);

        Vector3 t0 = DivideVec(min - origin, dir);
        Vector3 t1 = DivideVec(max - origin, dir);

        Vector3 tmax = Vector3.Max(t0, t1);

        float dstB = Mathf.Min(tmax.x, tmax.y, tmax.z);

        float dstInsideBox = Mathf.Max(0, dstB);

        return origin + dir * (dstInsideBox + 0.01f);
    }

    private void OnDrawGizmos()
    {
        Vector3 mapChunkPos = transform.position / MapGenerator.chunkSize;
        mapChunkPos.x = (Mathf.Floor(mapChunkPos.x) + 0.5f) * MapGenerator.chunkSize;
        mapChunkPos.y = (Mathf.Floor(mapChunkPos.y) + 0.5f) * MapGenerator.chunkSize;
        mapChunkPos.z = (Mathf.Floor(mapChunkPos.z) + 0.5f) * MapGenerator.chunkSize;

        Gizmos.DrawWireCube(mapChunkPos, Vector3.one * MapGenerator.chunkSize);

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        int boxCount = 10;

        for(int i = 0; i < boxCount; i++)
        {
            Vector3 castPos = RaycastTest(origin, direction);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, castPos);
            Gizmos.DrawWireSphere(castPos, 0.1f);
            Gizmos.color = Color.white;

            origin = castPos;
        }
    }
}
