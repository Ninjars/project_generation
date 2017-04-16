using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(GameNode))]
    public class SegmentUI : NodeUI {

        public float width = 0.25f;
        public float segmentSeparationRadians = 0.05f;

        private List<MeshRenderer> segmentRenderers;

        protected override void init() {
            segmentRenderers = createSegments(radius, width, gameNode.maxValue);
        }

        public override void updateRenderer() {
            uiRoot.transform.LookAt(Camera.main.transform);
            if (!shouldUpdate) {
                return;
            }
            for (int i = 0; i < segmentRenderers.Count; i++) {
                Material segmentMaterial;
                if (i < gameNode.currentValue) {
                    segmentMaterial = activeValueMaterial;
                } else {
                    segmentMaterial = passiveValueMaterial;
                }
                segmentRenderers[i].GetComponent<MeshRenderer>().material = segmentMaterial;
            }
            shouldUpdate = false;
        }

        private List<MeshRenderer> createSegments(float radius, float width, int count) {
            List<int> triangles = new List<int>() {
                    0, 1, 2,
                    1, 3, 2
                };
            float segmentArc = (2 * Mathf.PI - (segmentSeparationRadians * count)) / count;
            float angle = 0;
            List<MeshRenderer> segments = new List<MeshRenderer>();
            for (int i = 0; i < count; i++) {
                List<Vector3> vertices = createSegmentVertices(radius, angle, angle + segmentArc, width);
                Mesh segmentMesh = new Mesh();
                segmentMesh.vertices = vertices.ToArray();
                segmentMesh.triangles = triangles.ToArray();
                segmentMesh.RecalculateNormals();

                GameObject segmentObject = new GameObject();
                segmentObject.transform.SetParent(uiRoot.transform);
                segmentObject.transform.localPosition = Vector3.zero;
                segmentObject.transform.rotation = uiRoot.transform.rotation;
                MeshFilter meshFilter = segmentObject.AddComponent<MeshFilter>();
                meshFilter.mesh = segmentMesh;
                MeshRenderer meshRenderer = segmentObject.AddComponent<MeshRenderer>();

                segments.Add(meshRenderer);
                angle += segmentArc + segmentSeparationRadians;
            }
            return segments;
        }

        private List<Vector3> createSegmentVertices(float radius, float startAngle, float endAngle, float width) {
            Vector3 a = calculateVector(startAngle, radius);
            Vector3 b = calculateVector(startAngle, radius + width);
            Vector3 c = calculateVector(endAngle, radius);
            Vector3 d = calculateVector(endAngle, radius + width);
            return new List<Vector3>() { a, b, c, d };
        }

        private Vector3 calculateVector(float angle, float radius) {
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;
            return new Vector3(x, y, 0);
        }
    }
}
