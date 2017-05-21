using System;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(GameNode))]
    public class NodeUI : MonoBehaviour {
        public Material activeValueMaterial;

        [Range(0.1f, 3f)]
        public float radius = 1.0f;
        public float width = 0.25f;
        public float segmentSeparationRadians = 0.05f;

        private Globals globals;

        private bool shouldUpdate = true;
        private NodeViewModel viewModel;

        private GameObject uiRoot;
        private List<MeshRenderer> segmentRenderers;

        void Awake() {
            globals = FindObjectOfType<Globals>();
            uiRoot = new GameObject();
            uiRoot.transform.SetParent(gameObject.transform);
            uiRoot.transform.position = gameObject.transform.position;
            uiRoot.name = "uiRoot";
            uiRoot.transform.Rotate(Vector3.right, 90);
        }

        void Start() {
            updateRenderer();
        }

        private void Update() {
            updateRenderer();
        }

        public void onUpdate(NodeViewModel model) {
            shouldUpdate = true;
            viewModel = model;
        }

        private void updateRenderer() {
            if (!shouldUpdate) {
                return;
            }
            if (segmentRenderers == null || segmentRenderers.Count != viewModel.maxValue) {
                segmentRenderers = createSegments(radius, width, viewModel.maxValue);
            }
            int i = 0;
            foreach (PlayerMaterialViewModel model in viewModel.valueModel) {
                Material segmentMaterial = model.material;
                for (int j = i; j < i + model.count; j++) {
                    segmentRenderers[j].GetComponent<MeshRenderer>().material = segmentMaterial;
                }
                i += model.count;
            }
            Material passiveMaterial = globals.passiveValueMaterial;
            for (int j = i; j < segmentRenderers.Count; j++) {
                segmentRenderers[j].GetComponent<MeshRenderer>().material = passiveMaterial;
            }
            shouldUpdate = false;
        }

        private List<MeshRenderer> createSegments(float radius, float width, int count) {
            deleteExistingSegments();
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

        private void deleteExistingSegments() {
            if (segmentRenderers == null) {
                return;
            }
            foreach (MeshRenderer renderer in segmentRenderers) {
                Destroy(renderer.gameObject);
            }
            segmentRenderers.Clear();
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
