using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class NodeInteractionManager : MonoBehaviour {

        public enum InteractionMode {
            CONNECT,
            MOVE
        }

        private GameNode selectedNode = null;
        private Plane baseCollisionPlane;
        private InteractionMode mode = InteractionMode.CONNECT;

        private int activePlayerId = 1;

        void Awake() {
            baseCollisionPlane = new Plane(Vector3.up, Vector3.zero);
        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                checkForNodeInteraction(ray);
            }

            if (Input.GetButton("ChoosePlayer1")) {
                activePlayerId = 1;
            } else if (Input.GetButton("ChoosePlayer2")) {
                activePlayerId = 2;
            } else if (Input.GetButton("ChoosePlayerNeutral")) {
                activePlayerId = -1;
            } 
        }

        public void setInteractionMode(InteractionMode mode) {
            Debug.Log("NodeInteractionManager: setInteractionMode() " + mode);
            clearInteraction();
            this.mode = mode;
        }

        private void clearInteraction() {
            Debug.Log("NodeInteractionManager: clearInteraction()");
            selectedNode = null;
        }

        private bool checkForNodeInteraction(Ray ray) {
            float distance;
            int mask = 1 << LayerMask.NameToLayer("nodes");
            RaycastHit hit;
            Physics.Raycast(ray, out hit, Mathf.Infinity, mask);
            if (hit.collider != null) {
                Debug.Log("NodeInteractionManager: click on gameobject: " + hit.collider.gameObject);
                GameNode selectedNode = hit.collider.transform.parent.gameObject.GetComponent<GameNode>();
                return onInteraction(selectedNode);

            } else {
                clearInteraction();
                if (baseCollisionPlane.Raycast(ray, out distance)) {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    Debug.Log("NodeInteractionManager: click at position: " + hitPoint);
                }
                return false;
            }
        }

        private bool onInteraction(GameNode node) {
            Debug.Log("NodeInteractionManager: onInteraction() " + node);
            if (node == null) {
                clearInteraction();
                return false;
            } else if (selectedNode == null) {
                beginInteraction(node);
            } else {
                endInteraction(node);
            }
            return true;
        }

        private void beginInteraction(GameNode node) {
            Debug.Log("NodeInteractionManager: beginInteraction()");
            selectedNode = node;
            node.setOwnerId(activePlayerId);
        }

        private void endInteraction(GameNode node) {
            Debug.Log("NodeInteractionManager: endInteraction()");
            Debug.Assert(selectedNode != null);
            if (node.Equals(selectedNode)) {
                selectedNode.onSelfInteraction();
            } else {
                switch (mode) {
                case InteractionMode.CONNECT:
                    {
                        connectionInteraction(node);
                        break;
                    }
                case InteractionMode.MOVE:
                    {
                        moveInteraction(node);
                        break;
                    }
                }
            }
            clearInteraction();
        }

        private void connectionInteraction(GameNode node) {
            Debug.Log("NodeInteractionManager: connectionInteraction()");
            if (selectedNode.hasConnection(node)) {
                Debug.Log("NodeInteractionManager: removing connection " + selectedNode + ", " + node);
                selectedNode.removeConnection(node);
            } else {
                Debug.Log("NodeInteractionManager: adding connection " + selectedNode + ", " + node);
                selectedNode.addConnection(node);
                if (selectedNode.isOwnedBySamePlayer(node)) {
                    node.removeConnection(selectedNode);
                }
            }
        }

        private void moveInteraction(GameNode node) {
            Debug.Log("NodeInteractionManager: moveInteraction()");
            if (selectedNode.hasConnection(node)) {
                Debug.Log("NodeInteractionManager: moving " + selectedNode + " to " + node);
            } else {
                Debug.Log("NodeInteractionManager: invalid move; no connection from " + selectedNode + " to " + node);
            }
        }
    }
}
