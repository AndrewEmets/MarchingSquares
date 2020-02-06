using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class MarchingSquares : MonoBehaviour
{
    [SerializeField] private int fieldSize;
    [SerializeField, Range(0,1)] private float step;

    public float scale;
    public Vector2 Offset;

    private void OnValidate()
    {
        MakeMesh();
    }

    private readonly List<int> allTriangles = new List<int>();
    private readonly List<Vector3> allVertices = new List<Vector3>();

    [ContextMenu("make mesh")]
    public void MakeMesh()
    {
        ResetSDFCache();

        Profiler.BeginSample("Making mesh");
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            return;
        }

        if (meshFilter.sharedMesh == null)
        {
            meshFilter.sharedMesh = new Mesh();
        }

        var sharedMesh = meshFilter.sharedMesh;

        sharedMesh.Clear();

        allTriangles.Clear();
        allVertices.Clear();

        var vertCache = new Vector3[8];
        var trsCache = new List<Vector3>();

        for (var i = 0; i < fieldSize; i++)
        for (var j = 0; j < fieldSize; j++)
        {
            var ind = GetSquareIndex(i, j);
            var (localTris, vertMask) = squares[ind];

            if (localTris.Length == 0)
            {
                continue;
            }

            FillVertices(i, j, vertCache, vertMask);

            trsCache.Clear();
            for (int k = 0; k < localTris.Length; k++)
            {
                var p = vertCache[localTris[k]];
                p.x /= fieldSize;
                p.y /= fieldSize;
                trsCache.Add(p);
            }

            allVertices.AddRange(trsCache);
            for (int k = 0; k < trsCache.Count; k++)
            {
                allTriangles.Add(allTriangles.Count);
            }
        }

        sharedMesh.SetVertices(allVertices);
        sharedMesh.SetTriangles(allTriangles, 0);
        //sharedMesh.SetColors(allVertices.Select(v => UnityEngine.Random.ColorHSV()).ToList());

        //Debug.Log("Verts: " + vertices.Count);
        //Debug.Log("Tris : " + triangles.Count);
        Profiler.EndSample();
        Profiler.enabled = false;
    }

    private void FillVertices(int i, int j, Vector3[] vertCache, uint vertMask)
    {
        var offset = new Vector3(i, j);

        if ((vertMask & 1) == 1)
            vertCache[0] = offset;

        if ((vertMask & 2) == 2)
            vertCache[1] = offset + Vector3.right * f(SDF(i, j), SDF(i + 1, j), step);

        if ((vertMask & 4) == 4)
            vertCache[2] = offset + Vector3.right;

        if ((vertMask & 8) == 8)
            vertCache[3] = offset + Vector3.right + Vector3.up * f(SDF(i + 1, j), SDF(i + 1, j + 1), step);

        if ((vertMask & 16) == 16)
            vertCache[4] = offset + Vector3.right + Vector3.up;

        if ((vertMask & 32) == 32)
            vertCache[5] = offset + Vector3.right * f(SDF(i, j + 1), SDF(i + 1, j + 1), step) + Vector3.up;

        if ((vertMask & 64) == 64)
            vertCache[6] = offset + Vector3.up;

        if ((vertMask & 128) == 128)
            vertCache[7] = offset + Vector3.up * f(SDF(i, j), SDF(i, j + 1), step);

        float f(float a, float b, float s)
        {
            var f1 = (s - a) / (b - a);
            f1 = Mathf.Clamp01(f1);
            return f1;
        }
    }

    private float[,] sdfCache;
    float SDF(int x, int y)
    {
        if (!float.IsNaN(sdfCache[x, y]))
        {
            return sdfCache[x, y];
        }

        var uv = new Vector2(x * scale / fieldSize + Offset.x, y * scale / fieldSize + Offset.y);

        var result = Mathf.PerlinNoise(uv.x, uv.y);
        sdfCache[x, y] = result;
        return result;
    }

    private void ResetSDFCache()
    {
        if (sdfCache == null || sdfCache.GetLength(0) != fieldSize + 1 || sdfCache.GetLength(1) != fieldSize)
        {
            sdfCache = new float[fieldSize + 1, fieldSize + 1];
        }

        for (var i = 0; i < fieldSize + 1; i++)
        for (var j = 0; j < fieldSize + 1; j++)
        {
            sdfCache[i, j] = float.NaN;
        }
    }

    private int GetSquareIndex(int i, int j)
    {
        float
            p0 = SDF(i, j),
            p1 = SDF(i + 1, j),
            p2 = SDF(i, j + 1),
            p3 = SDF(i + 1, j + 1);

        var r0 = p0 > step ? 1 : 0;
        var r1 = p1 > step ? 1 : 0;
        var r2 = p2 > step ? 1 : 0;
        var r3 = p3 > step ? 1 : 0;

        var result = r3 << 3 | r2 << 2 | r1 << 1 | r0;

        return result;
    }

    private readonly (int[], uint)[] squares =
    {
        /*00*/ (new int[] { }, 0),
        /*01*/ (new[] {7, 1, 0}, 131),
        /*02*/ (new[] {1, 3, 2}, 14),
        /*03*/ (new[] {0, 7, 3, 0, 3, 2}, 143),
        /*04*/ (new[] {6, 5, 7}, 224),
        /*05*/ (new[] {0, 6, 5, 0, 5, 1}, 227),
        /*06*/ (new[] {7, 6, 5, 1, 3, 2}, 238),
        /*07*/ (new[] {0, 6, 5, 0, 5, 3, 0, 3, 2}, 239),
        /*08*/ (new[] {5, 4, 3}, 56),
        /*09*/ (new[] {0, 7, 1, 5, 4, 3}, 187),
        /*10*/ (new[] {1, 5, 4, 1, 4, 2}, 62),
        /*11*/ (new[] {2, 0, 7, 2, 7, 5, 2, 5, 4}, 191),
        /*12*/ (new[] {7, 6, 4, 7, 4, 3}, 248),
        /*13*/ (new[] {6, 4, 3, 6, 3, 1, 6, 1, 0}, 251),
        /*14*/ (new[] {4, 2, 1, 4, 1, 7, 4, 7, 6}, 254),
        /*15*/ (new[] {0, 6, 4, 0, 4, 2}, 255)
    };
}
