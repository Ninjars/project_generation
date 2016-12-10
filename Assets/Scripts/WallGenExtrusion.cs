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
                wallTriangles.Add(tl);
                wallTriangles.Add(tr);
                wallTriangles.Add(tlo);
                wallTriangles.Add(tlo);
                wallTriangles.Add(tr);
                wallTriangles.Add(tro);

                // front
                wallTriangles.Add(tro);
                wallTriangles.Add(bro);
                wallTriangles.Add(tlo);
                wallTriangles.Add(tlo);
                wallTriangles.Add(bro);
                wallTriangles.Add(blo);

                // bottom
                wallTriangles.Add(blo);
                wallTriangles.Add(bro);
                wallTriangles.Add(br);
                wallTriangles.Add(br);
                wallTriangles.Add(bl);
                wallTriangles.Add(blo);

                // left
                wallTriangles.Add(blo);
                wallTriangles.Add(bl);
                wallTriangles.Add(tlo);
                wallTriangles.Add(tlo);
                wallTriangles.Add(bl);
                wallTriangles.Add(tl);

                // back
                wallTriangles.Add(tl);
                wallTriangles.Add(bl);
                wallTriangles.Add(tr);
                wallTriangles.Add(tr);
                wallTriangles.Add(bl);
                wallTriangles.Add(br);

                // right
                wallTriangles.Add(br);
                wallTriangles.Add(bro);
                wallTriangles.Add(tr);
                wallTriangles.Add(tr);
                wallTriangles.Add(bro);
                wallTriangles.Add(tro);

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
}
