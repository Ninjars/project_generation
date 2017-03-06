using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class NodeInteractionManager : MonoBehaviour {

        private Node mFirstNode = null;
        private Plane mBaseCollisionPlane;

        public void onInteraction(Node node) {
            Debug.Log("NodeInteractionManager: onInteraction() " + node);
            if (node == null) {
                clearInteraction();
            } else if (mFirstNode == null) {
                beginInteraction(node);
            } else {
                endInteraction(node);
            }
        }

        private void beginInteraction(Node startNode) {
            mFirstNode = startNode;
        }

        private void clearInteraction() {
            mFirstNode = null;
        }

        private void endInteraction(Node endNode) {
            Debug.Assert(mFirstNode != null);
            if (mFirstNode.hasConnection(endNode)) {
                Debug.Log("NodeInteractionManager: removing connection " + mFirstNode + ", " + endNode);
                mFirstNode.removeConnection(endNode);
                endNode.removeConnection(mFirstNode);
            } else {
                Debug.Log("NodeInteractionManager: adding connection " + mFirstNode + ", " + endNode);
                mFirstNode.addConnection(endNode);
                endNode.addConnection(mFirstNode);
            }
            clearInteraction();
        }

        void Awake() {
            mBaseCollisionPlane = new Plane(Vector3.up, Vector3.zero);
        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float distance;
                int mask = 1 << LayerMask.NameToLayer("nodes");
                RaycastHit hit;
                Physics.Raycast(ray, out hit, Mathf.Infinity, mask);
                if (hit.collider != null) {
                    Debug.Log("NodeInteractionManager: click on gameobject: " + hit.collider.gameObject);
                    Node selectedNode = hit.collider.transform.parent.gameObject.GetComponent<Node>();
                    Debug.Assert(selectedNode != null);
                    if (mFirstNode == null) {
                        beginInteraction(selectedNode);
                    } else {
                        endInteraction(selectedNode);
                    }
                } else {
                    if (mBaseCollisionPlane.Raycast(ray, out distance)) {
                        Vector3 hitPoint = ray.GetPoint(distance);
                        Debug.Log("NodeInteractionManager: click at position: " + hitPoint);
                        clearInteraction();
                    }
                }
            }
        }
    }
}
