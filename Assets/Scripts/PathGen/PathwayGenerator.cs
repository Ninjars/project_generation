using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathGen {
    public class PathwayGenerator : MonoBehaviour {
        public GameObject startingNodeObj;
        public IPathMeshBuilder pathMeshGenerator;
        private HashSet<Node> nodes;
        private HashSet<NodeConnection> connections;

        void Start() {
            Node staringNode = startingNodeObj.GetComponent<Node>();
            Debug.Assert(staringNode != null, "PathwayGenerator given a starting object that wasn't a node");
            nodes = new HashSet<Node>();
            connections = new HashSet<NodeConnection>();
            traverseNodes(nodes, connections, staringNode);

            IPathMeshBuilder pathMeshGenerator = GetComponent<IPathMeshBuilder>();
            Debug.Assert(pathMeshGenerator != null, "requires a pathMeshBuilder implementation attached");
        }

        private void traverseNodes(HashSet<Node> knownNodes, HashSet<NodeConnection> connections, Node rootNode) {
            knownNodes.Add(rootNode);
            HashSet<NodeConnection> iterConnections = new HashSet<NodeConnection>(rootNode.getConnections());
            foreach (NodeConnection nodeConnection in iterConnections) {
                Node nextNode = nodeConnection.getOther(rootNode);
                Debug.Assert(nextNode != null, "connections should always have the current node in them");
                nextNode.addConnection(nodeConnection);
                if (!connections.Contains(nodeConnection)) {
                    connections.Add(nodeConnection);
                    traverseNodes(knownNodes, connections, nextNode);
                }
            }
        }

        void OnDrawGizmos() {
            if (!Application.isPlaying) {
                return;
            }
            Gizmos.color = Color.blue;
            foreach (Node node in nodes) {
                Gizmos.DrawCube(node.getPosition(), Vector3.one);
            }
            foreach (NodeConnection connection in connections) {
                Vector3 a = connection.getA().getPosition();
                Vector3 b = connection.getB().getPosition();
                Vector3 halfway = (b - a) / 2 + a;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(a, halfway);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(halfway, b);
            }
        }

        void Update() {

        }
    }
}