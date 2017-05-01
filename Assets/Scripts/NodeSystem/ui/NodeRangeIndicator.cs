using UnityEngine;
using System.Collections;

namespace Node {
    [RequireComponent(typeof(LineRenderer))]
    public class NodeRangeIndicator : MonoBehaviour {
        [Range(0.1f, 100f)]
        public float radius = 1.0f;

        [Range(3, 256)]
        public int numSegments = 128;

        void Start() {
            render();
        }

        public void setRadius(float radius) {
            this.radius = radius;
            render();
        }

        private void render() {
            LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = numSegments + 1;
            lineRenderer.useWorldSpace = false;

            float deltaTheta = (float) (2.0 * Mathf.PI) / numSegments;
            float theta = 0f;

            for (int i = 0 ; i < numSegments + 1 ; i++) {
                float x = radius * Mathf.Cos(theta);
                float z = radius * Mathf.Sin(theta);
                Vector3 pos = new Vector3(x, 0, z);
                lineRenderer.SetPosition(i, pos);
                theta += deltaTheta;
            }
        }
    }
}
