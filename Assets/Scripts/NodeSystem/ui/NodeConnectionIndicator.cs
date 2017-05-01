using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Node {
    public class NodeConnectionIndicator : MonoBehaviour {
        private List<GameObject> lineRenderers;

        void Start() {
            update();
        }

        public void update() {
            if (lineRenderers == null) {
                lineRenderers = new List<GameObject>();
            } else {
                foreach (GameObject oldRenderer in lineRenderers) {
                    GameObject.Destroy(oldRenderer);
                }
                lineRenderers.Clear();
            }
            Material lineMaterial = gameObject.GetComponent<Node>().getConnectionLineMaterial();

            HashSet<NodeConnection> connections = gameObject.GetComponent<Node>().getConnections();
            foreach (NodeConnection connection in connections) {
                GameObject container = new GameObject();
                container.transform.SetParent(gameObject.transform);
                LineRenderer connectionRenderer = container.AddComponent<LineRenderer>();
                connectionRenderer.positionCount = 2;
                connectionRenderer.material = lineMaterial;
                connectionRenderer.startWidth = 0.25f;
                connectionRenderer.endWidth = 0.25f;
                connectionRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                connectionRenderer.receiveShadows = false;
                connectionRenderer.SetPosition(0, connection.getA().getPosition());
                connectionRenderer.SetPosition(1, connection.getB().getPosition());
                lineRenderers.Add(container);
            }
        }
    }
}
