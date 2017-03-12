using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(GameNode))]
    public class NodeUI : MonoBehaviour {
        public Material activeValueMaterial;
        public Material passiveValueMaterial;

        [Range(0.1f, 100f)]
        public float radius = 1.0f;

        private bool shouldUpdate = true;
        private GameObject uiRoot;
        private List<GameObject> lineRenderers = new List<GameObject>();
        private GameNode gameNode;
        private List<Vector3> tempPositions = new List<Vector3>();

        private void Awake() {
            uiRoot = new GameObject();
            uiRoot.transform.SetParent(gameObject.transform);
            gameNode = GetComponent<GameNode>();
        }

        void Start() {
            updateRenderer();
        }

        private void Update() {
            updateRenderer();
        }

        public void hasUpdate() {
            shouldUpdate = true;
        }

        public void updateRenderer() {
            uiRoot.transform.LookAt(Camera.main.transform);
            if (!shouldUpdate) {
                return;
            }
            foreach (GameObject oldRenderer in lineRenderers) {
                GameObject.Destroy(oldRenderer);
            }
            lineRenderers.Clear();

            int totalSegments = gameNode.maxValue * 10;
            int currentSegments = gameNode.currentValue * 10;
            float deltaTheta = (float)(2.0 * Mathf.PI) / totalSegments;

            // draw current value
            if (currentSegments > 0) {
                tempPositions.Clear();
                GameObject currentValueIndicator = new GameObject();
                lineRenderers.Add(currentValueIndicator);

                currentValueIndicator.transform.SetParent(uiRoot.transform);
                LineRenderer lineRenderer = currentValueIndicator.AddComponent<LineRenderer>();
                lineRenderer.material = activeValueMaterial;
                lineRenderer.startWidth = 0.25f;
                lineRenderer.endWidth = 0.25f;
                lineRenderer.numPositions = currentSegments + 1;
                lineRenderer.numCornerVertices = 12;
                lineRenderer.useWorldSpace = false;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderer.receiveShadows = false;

                float theta = 0f;
                for (int i = 0; i < currentSegments + 1; i++) {
                    float x = radius * Mathf.Cos(theta);
                    float z = radius * Mathf.Sin(theta);
                    Vector3 pos = new Vector3(x, z, 0);
                    tempPositions.Add(pos);
                    theta += deltaTheta;
                }
                lineRenderer.SetPositions(tempPositions.ToArray());
            }

            // draw maxValue
            if (currentSegments < totalSegments) {
                tempPositions.Clear();
                GameObject maxValueIndicator = new GameObject();
                lineRenderers.Add(maxValueIndicator);
                maxValueIndicator.transform.SetParent(uiRoot.transform);
                LineRenderer lineRenderer = maxValueIndicator.AddComponent<LineRenderer>();
                lineRenderer.material = passiveValueMaterial;
                lineRenderer.startWidth = 0.25f;
                lineRenderer.endWidth = 0.25f;
                lineRenderer.numCornerVertices = 12;
                lineRenderer.numPositions = totalSegments - currentSegments + 1;
                lineRenderer.useWorldSpace = false;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderer.receiveShadows = false;

                float theta = deltaTheta * currentSegments;
                for (int i = currentSegments; i < totalSegments + 1; i++) {
                    float x = radius * Mathf.Cos(theta);
                    float z = radius * Mathf.Sin(theta);
                    Vector3 pos = new Vector3(x, z, 0);
                    tempPositions.Add(pos);
                    theta += deltaTheta;
                }
                lineRenderer.SetPositions(tempPositions.ToArray());
            }
            tempPositions.Clear();
            shouldUpdate = false;
        }
    }
}
