using System.Collections.Generic;
using UnityEngine;

namespace PathGen {
    public class Node : MonoBehaviour {

        public List<GameObject> childNodeObjects;
        private Dictionary<Node, double> connectedNodes = new Dictionary<Node, double>();

        void Awake() {
            Vector3 thisPosition = getPosition();
            foreach (GameObject node in childNodeObjects) {
                Node nodeComponent = node.GetComponent<Node>();
                if (nodeComponent != null) {
                    connectedNodes.Add(nodeComponent, Vector3.Distance(thisPosition, node.transform.position));
                } else {
                    Debug.LogWarning("Node.linkToNodesObjs contained a gameobject with no Node component");
                }
            }
        }

        public Dictionary<Node, double> getConnectedNodes() {
            return connectedNodes;
        }

        public void addNodeConnection(Node node) {
            connectedNodes.Add(node, Vector3.Distance(getPosition(), node.transform.position));
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }
    }
}
