using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathGen {
    public class BasicPathMeshBuilder : MonoBehaviour, IPathMeshBuilder {

        public float width = 0.5f;

        public Mesh generateMesh(Vector3 start, Vector3 end) {
            Vector3 scale = new Vector3(width, 1f, width);
            Vector3 vector = end - start;
            Debug.Log("generateMesh " + start + " to " + end + "vector " + vector);
            Vector3 tangent = getTangent(vector);
            Vector3 offset1 = Vector3.Scale(tangent, scale);
            Vector3 offset2 = Vector3.Scale(-tangent, scale);
            Vector3 s1 = start + offset1;
            Vector3 s2 = start + offset2;
            Vector3 e1 = end + offset1;
            Vector3 e2 = end + offset2;

            List<Vector3> vertices = new List<Vector3>();
            vertices.Add(s1);
            vertices.Add(e1);
            vertices.Add(e2);
            vertices.Add(s2);
            int[] triangles = {
                0, 1, 2,
                2, 3, 0
            };
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        private Vector3 getTangent(Vector3 normal) {
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