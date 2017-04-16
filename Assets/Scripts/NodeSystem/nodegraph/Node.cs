using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(NodeConnectionIndicator))]
    public class Node : MonoBehaviour {

        public List<GameObject> connectedNodes;
        public Material connectionLineMaterial;
        private HashSet<NodeConnection> connections = new HashSet<NodeConnection>();

        void Awake() {
            foreach (GameObject node in connectedNodes) {
                Node nodeComponent = node.GetComponent<Node>();
                if (nodeComponent != null) {
                    addConnection(nodeComponent);
                    nodeComponent.addConnection(this);
                } else {
                    Debug.LogWarning("Node.linkToNodesObjs contained a gameobject with no Node component");
                }
            }
        }

        public HashSet<NodeConnection> getConnections() {
            return connections;
        }

        public void addConnection(Node node) {
            connections.Add(new NodeConnection(this, node));
            GetComponent<NodeConnectionIndicator>().update();
        }

        public void removeConnection(Node node) {
            connections.Remove(new NodeConnection(this, node));
            GetComponent<NodeConnectionIndicator>().update();
        }

        public void removeAllConnections() {
            connections.Clear();
            GetComponent<NodeConnectionIndicator>().update();
        }

        public bool hasConnection(Node node) {
            NodeConnection connection = new NodeConnection(this, node);
            return connections.Contains(connection);
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }

        public override string ToString() {
            return "<Node @ " + getPosition() + ">";
        }

        public void addConnection(NodeConnection connection) {
            Debug.Assert(connection.getA().Equals(this) || connection.getB().Equals(this), "must contain this node to connect");
            connections.Add(connection);
        }

        public Material getConnectionLineMaterial() {
            return connectionLineMaterial;
        }

        public List<Node> getConnectedNodes() {
            List<Node> nodes = new List<Node>(connections.Count);
            foreach (NodeConnection connection in connections) {
                nodes.Add(connection.getOther(this));
            }
            return nodes;
        }
    }
}
