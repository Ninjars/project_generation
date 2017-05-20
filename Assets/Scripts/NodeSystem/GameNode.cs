using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(Node))]
    public abstract class GameNode : MonoBehaviour {

        public int intialValue = 0;
        public int maxValue = 10;
        public int intialOwnerId = 0;
        public int maxOutboundConnections = -1;
        public bool allowsInboundConnections = true;

        public GameObject packet;

        protected NodeUI nodeUi;
        protected Node nodeComponent;

        private GameManager gameManager;
        private Globals globals;
        private List<GameNode> gameNodesInRange;
        private NodeValue currentValue;

        private void Awake() {
            nodeUi = GetComponent<NodeUI>();
            nodeComponent = GetComponent<Node>();
            gameManager = FindObjectOfType<GameManager>();
            globals = FindObjectOfType<Globals>();
            gameNodesInRange = createGameNodesInRangeList();
            currentValue = new NodeValue(intialOwnerId, maxValue, intialValue, newOwnerId => onOwnerChange(newOwnerId));
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

        private void onOwnerChange(int ownerId) {
            currentValue.setOwner(ownerId);
            gameObject.GetComponentInChildren<MeshRenderer>().material = globals.playerMaterials[currentValue.getOwnerId()];
            gameManager.onGameNodeOwnerChange(this);
            nodeComponent.removeAllConnections();
        }

        public bool isOwnedBySamePlayer(GameNode otherNode) {
            return otherNode.getOwnerId() == getOwnerId();
        }

        public int getOwnerId() {
            return currentValue.getOwnerId();
        }

        protected bool isNeutral() {
            return getOwnerId() == GameManager.NEUTRAL_PLAYER_ID;
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }

        public void onPacket(Packet packet) {
            Debug.Log("onPacket");
            changeValue(packet.getOwnerId(), 1);
            nodeUi.hasUpdate();
        }

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
            return currentValue.getValueForPlayer(getOwnerId()) < maxValue;
        }

        public virtual void changeValue(int playerId, int change) {
            currentValue.changePlayerValue(playerId, change);
        }

        public List<GameNode> getGameNodesInRange() {
            return gameNodesInRange;
        }

        public int getOwnerValue() {
            return currentValue.getValueForPlayer(getOwnerId());
        }
    }
}
