using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class NodeInteractionManager : MonoBehaviour {

        public GameObject rangeIndicatorObject;

        private GameManager gameManager;
        private NodeRangeIndicator rangeIndicatorScript;
        private GameObject rangeIndicatorInstance;
        private GameNode selectedNode = null;
        private Plane baseCollisionPlane;

        private int activePlayerId = 1;
        private float interactionStartTime;
        private const float selfInteractionTimeThreshold = 0.5f;

        void Awake() {
            gameManager = FindObjectOfType<GameManager>();
            baseCollisionPlane = new Plane(Vector3.up, Vector3.zero);
        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                checkForNodeInteraction(ray);
            }

            if (rangeIndicatorInstance != null && rangeIndicatorInstance.activeSelf) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                updateRangeIndicator(ray);
            }

            if (Input.GetButton("ChoosePlayer1")) {
                activePlayerId = 1;
            } else if (Input.GetButton("ChoosePlayer2")) {
                activePlayerId = 2;
            } else if (Input.GetButton("ChoosePlayerNeutral")) {
                activePlayerId = 0;
            } 
        }

        private void clearInteraction() {
            Debug.Log("NodeInteractionManager: clearInteraction()");
            selectedNode = null;
            hideRangeIndicator();
            interactionStartTime = -1f;
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
            if (node.getOwnerId() == activePlayerId) {
                selectedNode = node;
                showRangeIndicator(node.getPosition());
                interactionStartTime = Time.time;
            }
        }

        private void endInteraction(GameNode node) {
            Debug.Log("NodeInteractionManager: endInteraction()");
            Debug.Assert(selectedNode != null);
            bool isSameNode = node.Equals(selectedNode);
            if (isSameNode && isDoubleTap()) {
                selectedNode.onSelfInteraction();
            } else if (!isSameNode) {
                connectionInteraction(node);
            } else {
                selectedNode.clearConnection();
            }
            clearInteraction();
        }

        private bool isDoubleTap() {
            return interactionStartTime > 0 && Time.time - interactionStartTime <= selfInteractionTimeThreshold;
        }

        private void connectionInteraction(GameNode node) {
            Debug.Log("NodeInteractionManager: connectionInteraction " + selectedNode.name + " to " + node.name);
            IList<GameNode> nodesInRange = gameManager.getNodeGraph().getConnectedNodes(selectedNode);
            if (!nodesInRange.Contains(node)) {
                Debug.Log("NodeInteractionManager: no connection available");
            } else if (selectedNode.isConnected(node)) {
                Debug.Log("NodeInteractionManager: already connected; ignoring");
            } else {
                Debug.Log("NodeInteractionManager: adding connection " + selectedNode + ", " + node);
                selectedNode.connectToNode(node);
                if (selectedNode.isOwnedBySamePlayer(node) && node.isConnected(selectedNode)) {
                    Debug.Log("NodeInteractionManager: >> clearing reciprocal connection");
                    node.clearConnection();
                }
            }
        }

        private void showRangeIndicator(Vector3 position) {
            if (rangeIndicatorInstance == null) {
                rangeIndicatorInstance = GameObject.Instantiate(rangeIndicatorObject);
                rangeIndicatorScript = rangeIndicatorInstance.GetComponent<NodeRangeIndicator>();
            } else {
                rangeIndicatorInstance.SetActive(true);
            }
            rangeIndicatorInstance.transform.position = position;
        }

        private void hideRangeIndicator() {
            if (rangeIndicatorInstance != null) {
                rangeIndicatorInstance.SetActive(false);
            }
        }

        private void updateRangeIndicator(Ray ray) {
            if (rangeIndicatorScript == null || rangeIndicatorInstance == null) {
                return;
            }
            float distanceToBaseCollision;
            if (baseCollisionPlane.Raycast(ray, out distanceToBaseCollision)) {
                Vector3 hitPoint = ray.GetPoint(distanceToBaseCollision);
                Vector3 indicatorPosition = rangeIndicatorInstance.transform.position;
                float distanceBetween = Vector3.Distance(
                    new Vector3(hitPoint.x, 0, hitPoint.z), 
                    new Vector3(indicatorPosition.x, 0, indicatorPosition.z));
                rangeIndicatorScript.setRadius(Mathf.Min(distanceBetween, GameManager.nodeConnectionRange));
            }
        }
    }
}
