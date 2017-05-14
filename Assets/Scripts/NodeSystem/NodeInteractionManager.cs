using UnityEngine;

namespace Node {
    public class NodeInteractionManager : MonoBehaviour {

        public GameObject rangeIndicatorObject;

        private NodeRangeIndicator rangeIndicatorScript;
        private GameObject rangeIndicatorInstance;
        private GameNode selectedNode = null;
        private Plane baseCollisionPlane;

        private int activePlayerId = 1;

        void Awake() {
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
            if (node.ownerId == activePlayerId) {
                selectedNode = node;
                showRangeIndicator(node.getPosition());
            }
        }

        private void endInteraction(GameNode node) {
            Debug.Log("NodeInteractionManager: endInteraction()");
            Debug.Assert(selectedNode != null);
            if (node.Equals(selectedNode)) {
                selectedNode.onSelfInteraction();
            } else {
                connectionInteraction(node);
            }
            clearInteraction();
        }

        private void connectionInteraction(GameNode node) {
            Debug.Log("NodeInteractionManager: connectionInteraction()");
            // limit node connection range
            float distanceBetweenNodes = Vector3.Distance(node.getPosition(), selectedNode.getPosition());
            if (distanceBetweenNodes > GameManager.nodeConnectionRange) {
                Debug.Log("NodeInteractionManager: distance too far " + distanceBetweenNodes);
            } else if (!node.allowsInboundConnections) {
                Debug.Log("NodeInteractionManager: not allowed to connect to " + node);
            } else if (selectedNode.hasConnection(node)) {
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
