using System.Collections.Generic;
using UnityEngine;

public class WallGenSimple : MonoBehaviour, IWallGenerator {
    public float wallHeight = 5;

    public Mesh generate(List<List<int>> outlines, List<Vector3> vertices) {
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count - 1; i++) {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left
                wallVertices.Add(vertices[outline[i + 1]]); // right
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                // winding triangles anticlockwise to view from within
                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 0);
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = wallVertices.ToArray();
        mesh.triangles = wallTriangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
}
