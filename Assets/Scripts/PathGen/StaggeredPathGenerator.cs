using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathGen {
    public class StaggeredPathGenerator : MonoBehaviour, IPathMeshBuilder {

        public float width = 0.5f;

        [Range(0.5f, 10f)]
        [Tooltip("Multiplier applied to width for max distance to step laterally by")]
        public float stepFactor = 2.5f;

        [Range(2, 20)]
        [Tooltip("Number of lateral steps on path")]
        public int stepCount = 3;

        private int[] baseTriangles = {
            0, 1, 2,
            2, 3, 0
        };
        private Vector3 scale;

        private class Quad {
            public readonly Vector3 a;
            public readonly Vector3 b;
            public readonly Vector3 c;
            public readonly Vector3 d;

            public Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
                this.a = a;
                this.b = b;
                this.c = c;
                this.d = d;
            }
        }

        private class Line {
            public readonly Vector3 start;
            public readonly Vector3 end;

            public Line(Vector3 start, Vector3 end) {
                this.start = start;
                this.end = end;
            }

            public static Line offset(Line line, float distance) {
                Vector3 vector = line.end - line.start;
                Vector3 tangent = getTangent(vector);
                Vector3 offset = Vector3.Scale(tangent, new Vector3(distance, 1f, distance));
                return new Line(line.start + offset, line.end + offset);
            }

            private static Vector3 getTangent(Vector3 normal) {
                Vector3 t1 = Vector3.Cross(normal, Vector3.up);
                Vector3 t2 = Vector3.Cross(normal, Vector3.forward);
                if (t1.sqrMagnitude > t2.sqrMagnitude) {
                    return t1.normalized;
                } else {
                    return t1.normalized;
                }
            }

            public override string ToString() {
                return "Line{" + start + " -> " + end + "}";
            }
        }

        private void Awake() {
            scale = new Vector3(width, 1f, width);
        }

        public Mesh generateMesh(Vector3 start, Vector3 end) {
            // separate path into segments
            List<Vector3> pathKeyPoints = new List<Vector3>();
            int pathPointCount = stepCount + 2;
            Debug.Log("key points: ");
            for (int i = 0; i <= pathPointCount; i++) {
                float fraction = i / (float) pathPointCount;
                Vector3 point = getPointOnPath(fraction, start, end);
                pathKeyPoints.Add(point);
                Debug.Log("> " + point);
            }
            List<Line> sections = new List<Line>();
            for (int i = 0; i < pathKeyPoints.Count - 1; i++) {
                Line line = new Line(pathKeyPoints[i], pathKeyPoints[i + 1]);
                Debug.Log("creating " + line);
                if (i != 0 && i != pathKeyPoints.Count - 2) {
                    // offset inner line sections
                    line = Line.offset(line, UnityEngine.Random.value * stepFactor * width * 2 - (stepFactor * width));
                    Debug.Log("offset to " + line);
                }
                sections.Add(line);
            }
            
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            foreach (Line line in sections) {
                Debug.Log("creating quad from " + line);
                addQuad(vertices, triangles, makeQuadFromLine(line.start, line.end));
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }

        private Vector3 getPointOnPath(float fraction, Vector3 start, Vector3 end) {
            float x = (end.x - start.x) * fraction + start.x;
            float y = (end.y - start.y) * fraction + start.y;
            float z = (end.z - start.z) * fraction + start.z;
            Debug.Log("getPointOnPath at fraction " + fraction + ": " + x + ", " + y + ", " + z);
            return new Vector3(x, y, z);
        }

        private Quad makeQuadFromLine(Vector3 start, Vector3 end) {
            Vector3 vector = end - start;
            Vector3 tangent = getTangent(vector);
            Vector3 offset1 = Vector3.Scale(tangent, scale);
            Vector3 offset2 = Vector3.Scale(-tangent, scale);
            return new Quad(start + offset1, start + offset2, end + offset1, end + offset2);
        }

        private void addQuad(List<Vector3> currentVerts, List<int> currentTris, Quad quad) {
            int triOffset = currentVerts.Count;
            currentVerts.Add(quad.a);
            currentVerts.Add(quad.c);
            currentVerts.Add(quad.d);
            currentVerts.Add(quad.b);

            foreach (int i in baseTriangles) {
                currentTris.Add(i + triOffset);
            }
        }

        private static Vector3 getTangent(Vector3 normal) {
            Vector3 t1 = Vector3.Cross(normal, Vector3.up);
            Vector3 t2 = Vector3.Cross(normal, Vector3.forward);
            if (t1.sqrMagnitude > t2.sqrMagnitude) {
                return t1.normalized;
            } else {
                return t1.normalized;
            }
        }
    }
}