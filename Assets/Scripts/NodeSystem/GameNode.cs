using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(Node))]
    public abstract class GameNode : MonoBehaviour {

        public int currentValue = 0;
        public bool isActive = true;
        public int maxValue = 10;
        public int ownerId = 0;
        public int maxOutboundConnections = -1;
        public bool allowsInboundConnections = true;

        public GameObject packet;

        protected NodeUI nodeUi;
        protected Node nodeComponent;

        private GameManager gameManager;
        private Globals globals;
        private List<GameNode> gameNodesInRange;

        private void Awake() {
            nodeUi = GetComponent<NodeUI>();
            nodeComponent = GetComponent<Node>();
            gameManager = FindObjectOfType<GameManager>();
            globals = FindObjectOfType<Globals>();
            gameNodesInRange = createGameNodesInRangeList();
        }

        private List<GameNode> createGameNodesInRangeList() {
            float maxRange = GameManager.nodeConnectionRange;
            int mask = 1 << LayerMask.NameToLayer("nodes");
            Collider[] colliders = Physics.OverlapSphere(getPosition(), maxRange, mask);
            List<GameNode> nodes = new List<GameNode>();
            foreach (Collider collider in colliders) {
                GameNode node = collider.transform.gameObject.GetComponentInParent<GameNode>();
                if (node != null && node != this) {
                    nodes.Add(node);
                }
            }
            return nodes;
        }

        void Start() {
            setOwnerId(ownerId);
        }

        public void setOwnerId(int activePlayerId) {
            ownerId = activePlayerId;
            gameObject.GetComponentInChildren<MeshRenderer>().material = globals.playerMaterials[activePlayerId];
            gameManager.onGameNodeOwnerChange(this);
            isActive = ownerId != GameManager.NEUTRAL_PLAYER_ID;
        }

        public bool isOwnedBySamePlayer(GameNode otherNode) {
            return otherNode.ownerId == ownerId;
        }

        protected int getOwnerId() {
            return ownerId;
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }

        public abstract void onPacket(Packet packet);

        public abstract void onSlowBeat();

        public abstract void onMediumBeat();

        public abstract void onFastBeat();

        public bool hasConnection(GameNode otherNode) {
            return nodeComponent.hasConnection(otherNode.nodeComponent);
        }

        public void removeConnection(GameNode otherNode) {
            nodeComponent.removeConnection(otherNode.nodeComponent);
        }

        public void addConnection(GameNode otherNode) {
            if (!otherNode.allowsInboundConnections) {
                Debug.Log("GameNode: ERROR: attempted to connect to node that doesn't allow inbound connections");
                return;
            }
            if (maxOutboundConnections < 0 || nodeComponent.getConnections().Count < maxOutboundConnections) {
                nodeComponent.addConnection(otherNode.nodeComponent);
            } else if (maxOutboundConnections == 1) {
                nodeComponent.removeAllConnections();
                nodeComponent.addConnection(otherNode.nodeComponent);
            }
        }

        public void onSelfInteraction() {
            nodeComponent.removeAllConnections();
        }

        public bool canReceivePacket() {
            return currentValue < maxValue;
        }

        public virtual void changeValue(int change) {
            currentValue = Mathf.Max(0, Mathf.Min(maxValue, currentValue + change));
        }

        public List<GameNode> getGameNodesInRange() {
            return gameNodesInRange;
        }
    }
}
