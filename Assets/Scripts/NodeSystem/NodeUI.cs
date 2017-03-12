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
            gameNode = GetComponent<GameNode>();
        }

        void Start() {
            uiRoot = new GameObject();
            uiRoot.transform.SetParent(gameObject.transform);
            uiRoot.transform.position = gameObject.transform.position;
            uiRoot.name = "uiRoot";
            updateRenderer();
        }

        private void Update() {
            updateRenderer();
        }

        public void hasUpdate() {
            shouldUpdate = true;
        }

        public void updateRenderer() {
            //uiRoot.transform.position = gameObject.transform.position;
            //uiRoot.transform.LookAt(Camera.main.transform);
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
            float initialAngle = 0.5f * Mathf.PI;

            // draw current value
            if (currentSegments > 0) {
                tempPositions.Clear();
                GameObject lineContainer = new GameObject();
                lineContainer.transform.SetParent(uiRoot.transform);
                lineContainer.transform.localPosition = Vector3.zero;
                lineRenderers.Add(lineContainer);

                LineRenderer lineRenderer = lineContainer.AddComponent<LineRenderer>();
                lineRenderer.material = activeValueMaterial;
                lineRenderer.startWidth = 0.25f;
                lineRenderer.endWidth = 0.25f;
                lineRenderer.numPositions = currentSegments + 1;
                lineRenderer.numCornerVertices = 12;
                lineRenderer.useWorldSpace = false;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderer.receiveShadows = false;

                float theta = initialAngle;
                for (int i = 0; i < currentSegments + 1; i++) {
                    float x = radius * Mathf.Cos(theta);
                    float z = radius * Mathf.Sin(theta);
                    Vector3 pos = new Vector3(x, z, 0);
                    tempPositions.Add(pos);
                    theta -= deltaTheta;
                }
                lineRenderer.SetPositions(tempPositions.ToArray());
            }

            // draw maxValue
            if (currentSegments < totalSegments) {
                tempPositions.Clear();
                GameObject lineContainer = new GameObject();
                lineContainer.transform.SetParent(uiRoot.transform);
                lineContainer.transform.localPosition = Vector3.zero;
                lineRenderers.Add(lineContainer);

                LineRenderer lineRenderer = lineContainer.AddComponent<LineRenderer>();
                lineRenderer.material = passiveValueMaterial;
                lineRenderer.startWidth = 0.25f;
                lineRenderer.endWidth = 0.25f;
                lineRenderer.numCornerVertices = 12;
                lineRenderer.numPositions = totalSegments - currentSegments + 1;
                lineRenderer.useWorldSpace = false;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderer.receiveShadows = false;

                float theta = initialAngle - (deltaTheta * currentSegments);
                for (int i = currentSegments; i < totalSegments + 1; i++) {
                    float x = radius * Mathf.Cos(theta);
                    float z = radius * Mathf.Sin(theta);
                    Vector3 pos = new Vector3(x, z, 0);
                    tempPositions.Add(pos);
                    theta -= deltaTheta;
                }
                lineRenderer.SetPositions(tempPositions.ToArray());
            }
            tempPositions.Clear();
            shouldUpdate = false;
        }
    }
}
