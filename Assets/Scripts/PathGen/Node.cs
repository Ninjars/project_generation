﻿using System.Collections.Generic;
using UnityEngine;

namespace PathGen {
    public class Node : MonoBehaviour, IClickListener {

        public List<GameObject> childNodeObjects;
        private HashSet<NodeConnection> connections = new HashSet<NodeConnection>();

        void Awake() {
            foreach (GameObject node in childNodeObjects) {
                Node nodeComponent = node.GetComponent<Node>();
                if (nodeComponent != null) {
                    connections.Add(new NodeConnection(this, nodeComponent));
                } else {
                    Debug.LogWarning("Node.linkToNodesObjs contained a gameobject with no Node component");
                }
            }

            for (int i = 0; i < transform.childCount; i++) {
                GameObject childGameObject = transform.GetChild(i).gameObject;
                if(childGameObject.GetComponent<ParentNotifier>() == null) {
                    childGameObject.AddComponent<ParentNotifier>().listener = this;
                }
            }
        }

        #region IClickListener implementation

        public void onClick() {
            Debug.Log("Node.onClick()");
            NodeInteractionManager.onInteraction(this);
        }

        #endregion

        public HashSet<NodeConnection> getConnections() {
            return connections;
        }

        public void addConnection(Node node) {
            connections.Add(new NodeConnection(this, node));
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

        void OnDrawGizmos() {
            if (!Application.isPlaying) {
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(getPosition(), Vector3.one);
                foreach (GameObject node in childNodeObjects) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(getPosition(), node.transform.position);
                }
            }
        }
    }
}
