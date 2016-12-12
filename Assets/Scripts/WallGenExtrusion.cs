using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenExtrusion : MonoBehaviour, Interfaces.IWallGenerator {
    private static System.Random random = new System.Random();

    public float wallHeight = 5;
    public float wallDepthVariance = 0.25f;
    public float minWallDepth = 0.5f;

    public Mesh generate(List<List<int>> outlines, List<Vector3> vertices) {
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count - 1; i++) {
                int previousVertex = i == 0 ? outline.Count - 1 : i - 1;
                int nextVertex = i == outline.Count - 2 ? 0 : i + 2;
                List<Vector3[]> outlineBox = makeBoxFromLine(vertices[outline[i]], vertices[outline[i + 1]], vertices[outline[previousVertex]], vertices[outline[nextVertex]]);

                foreach (Vector3[] surface in outlineBox) {
                    int startIndex = wallVertices.Count;
                    wallVertices.AddRange(surface);
                    wallTriangles.AddRange(triangulateSurface(startIndex, 0, 1, 2, 3));
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = wallVertices.ToArray();
        mesh.triangles = wallTriangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    private int[] triangulateSurface(int offset, int a, int b, int c, int d) {
        return new int[] { offset+a, offset+b, offset+d, offset+a, offset+d, offset+c };
    }

    private List<Vector3[]> makeBoxFromLine(Vector3 parentTopLeft, Vector3 parentTopRight, Vector3 neighbourLeft, Vector3 neighbourRight) {
        Vector3 parentBottomLeft = parentTopLeft - Vector3.up * wallHeight;
        Vector3 parentBottomRight = parentTopRight - Vector3.up * wallHeight;

        // find normal of adjacent line
        Vector3 normalPrevious = getNormal(neighbourLeft, parentTopLeft, parentBottomLeft);
        Vector3 normalNext = getNormal(parentTopRight, neighbourRight, parentBottomRight);
        Vector3 normal = getNormal(parentTopLeft, parentTopRight, parentBottomLeft);

        float offset = minWallDepth + (float)random.NextDouble() * wallDepthVariance;
        float wallHeightVal = wallHeight / 2f + (wallHeight / 2f * (float) random.NextDouble());
        Vector3[] vertices = createVerticesForExtrusion(parentTopLeft, parentTopRight, Vector3.Normalize(normalPrevious + normal), Vector3.Normalize(normalNext + normal), wallHeightVal, offset);

        List<Vector3[]> surfaces = getSurfacesFromVertices(vertices[0], vertices[1], vertices[2], vertices[3], vertices[4], vertices[5], vertices[6], vertices[7]);

        return surfaces;
    }

    private Vector3[] createVerticesForExtrusion(Vector3 topLeft, Vector3 topRight, Vector3 normalLeft, Vector3 normalRight, float height, float offset) {
        Vector3 offsetLeft = normalLeft * offset;
        Vector3 offsetRight = normalRight * offset;

        Vector3 bottomLeft = topLeft - Vector3.up * height;
        Vector3 bottomRight = topRight - Vector3.up * height;
        
        Vector3 topLeftOffset = topLeft + offsetLeft;
        Vector3 topRightOffset = topRight + offsetRight;
        Vector3 bottomLeftOffset = bottomLeft + offsetLeft;
        Vector3 bottomRightOffset = bottomRight + offsetRight;

        return new Vector3[] {
                topLeft, topRight, bottomRight, bottomLeft, 
                topLeftOffset, topRightOffset, bottomRightOffset, bottomLeftOffset
            };
    }

    private List<Vector3[]> getSurfacesFromVertices(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, 
                                                    Vector3 topLeftOffset, Vector3 topRightOffset, Vector3 bottomRightOffset, Vector3 bottomLeftOffset) {
        List<Vector3[]> surfaces = new List<Vector3[]>();
        // back
        surfaces.Add(copySurfaceVectors(topRight, topLeft, bottomRight, bottomLeft));

        // front
        surfaces.Add(copySurfaceVectors(topLeftOffset, topRightOffset, bottomLeftOffset, bottomRightOffset));

        // top
        surfaces.Add(copySurfaceVectors(topLeft, topRight, topLeftOffset, topRightOffset));

        // bottom
        surfaces.Add(copySurfaceVectors(bottomRight, bottomLeft, bottomRightOffset, bottomLeftOffset));

        // left
        surfaces.Add(copySurfaceVectors(topLeft, topLeftOffset, bottomLeft, bottomLeftOffset));

        // right
        surfaces.Add(copySurfaceVectors(topRightOffset, topRight, bottomRightOffset, bottomRight));
        return surfaces;
    }

    private Vector3[] copySurfaceVectors(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
        return new Vector3[] {
            new Vector3(a.x, a.y, a.z),
            new Vector3(b.x, b.y, b.z),
            new Vector3(c.x, c.y, c.z),
            new Vector3(d.x, d.y, d.z)
        };
    }

    private Vector3 getNormal(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 dir = Vector3.Cross(b - a, c - a);
        return Vector3.Normalize(dir);
    }
}
