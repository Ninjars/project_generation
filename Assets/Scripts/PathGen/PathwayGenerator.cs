using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathGen {
    public class PathwayGenerator : MonoBehaviour {
        public GameObject startingNodeObj;
        public IPathMeshBuilder pathMeshGenerator;
        private HashSet<Node> nodes;

        void Start() {
            Node nodeComponent = startingNodeObj.GetComponent<Node>();
            Debug.Assert(nodeComponent != null, "PathwayGenerator given a starting object that wasn't a node");
            nodes = new HashSet<Node>();
            addConnectedNodes(nodes, nodeComponent);

        }

        private void addConnectedNodes(HashSet<Node> knownNodes, Node rootNode) {
            knownNodes.Add(rootNode);
            foreach (Node node in rootNode.getConnectedNodes().Keys) {
                if (!knownNodes.Contains(node)) {
                    addConnectedNodes(knownNodes, node);
                }
            }
        }

        void OnDrawGizmos() {
            foreach (Node node in nodes) {
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(node.getPosition(), Vector3.one);
                Gizmos.color = Random.ColorHSV();
                foreach (Node connectedNode in node.getConnectedNodes().Keys) {
                    Gizmos.DrawLine(node.getPosition(), connectedNode.getPosition());
                }
            }
        }

        void Update() {

        }
    }
}