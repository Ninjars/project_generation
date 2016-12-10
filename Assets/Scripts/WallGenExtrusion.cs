using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenExtrusion : MonoBehaviour, Interfaces.IWallGenerator {
    public float wallHeight = 5;
    public float wallDepth = 1;

    public Mesh generate(List<List<int>> outlines, List<Vector3> vertices) {
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count - 1; i++) {
                int startIndex = wallVertices.Count;
                List<Vector3> outlineBox = makeBoxFromLine(vertices[outline[i]], vertices[outline[i + 1]]);
                wallVertices.AddRange(outlineBox);

                // wind triangles around the new 6-sided polygon
                int tl = startIndex + 0;
                int tr = startIndex + 1;
                int bl = startIndex + 2;
                int br = startIndex + 3;
                int tlo = startIndex + 4;
                int tro = startIndex + 5;
                int blo = startIndex + 6;
                int bro = startIndex + 7;

                // top
                wallTriangles.AddRange(triangulateSurface(tl, tr, tro, tlo));

                // front
                wallTriangles.AddRange(triangulateSurface(tro, bro, blo, tlo));

                // bottom
                wallTriangles.AddRange(triangulateSurface(blo, bro, br, bl));

                // right
                wallTriangles.AddRange(triangulateSurface(br, bro, tro, tr));

                // back
                wallTriangles.AddRange(triangulateSurface(tr, tl, bl, br));

                // left
                wallTriangles.AddRange(triangulateSurface(tl, tlo, blo, bl));
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = wallVertices.ToArray();
        mesh.triangles = wallTriangles.ToArray();
        return mesh;
    }

    private List<Vector3> makeBoxFromLine(Vector3 topLeft, Vector3 topRight) {
        List<Vector3> box = new List<Vector3>();
        Vector3 bottomLeft = topLeft - Vector3.up * wallHeight;
        Vector3 bottomRight = topRight - Vector3.up * wallHeight;

        // surface is made of 2 triangles, but we are assuming that it is planar and so both share same normal
        Vector3 normal = getNormal(topLeft, topRight, bottomLeft);
        Vector3 offset = normal * wallDepth;
        Vector3 topLeftOffset = topLeft + offset;
        Vector3 topRightOffset = topRight + offset;
        Vector3 bottomLeftOffset = bottomLeft + offset;
        Vector3 bottomRightOffset = bottomRight + offset;

        box.Add(topLeft);
        box.Add(topRight);
        box.Add(bottomLeft);
        box.Add(bottomRight);
        box.Add(topLeftOffset);
        box.Add(topRightOffset);
        box.Add(bottomLeftOffset);
        box.Add(bottomRightOffset);
        return box;
    }

    private Vector3 getNormal(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 dir = Vector3.Cross(b - a, c - a);
        return Vector3.Normalize(dir);
    }

    private int[] triangulateSurface(int a, int b, int c, int d) {
        return new int[] { a, b, d, d, b, c };
    }
}
