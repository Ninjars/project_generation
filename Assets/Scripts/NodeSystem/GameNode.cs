using System;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(NodeConnectionIndicator))]
    public abstract class GameNode : MonoBehaviour {

        public GameObject connectedNode;
        public Material connectionLineMaterial;

        public int initialValue = 0;
        public int maxValue = 10;
        public int initialOwnerId = 0;
        public int maxOutboundConnections = -1;
        public bool allowsInboundConnections = true;

        public GameObject packet;

        protected NodeUI nodeUi;

        protected GameManager gameManager;
        private Globals globals;
        private List<GameNode> gameNodesInRange;
        private NodeValue currentValue;
        private NodeConnection connection;

        private void Awake() {
            nodeUi = GetComponent<NodeUI>();
            gameManager = FindObjectOfType<GameManager>();
            globals = FindObjectOfType<Globals>();

            if (connectedNode != null) {
                GameNode nodeComponent = connectedNode.GetComponent<GameNode>();
                if (nodeComponent != null) {
                    connectToNode(this);
                } else {
                    Debug.LogWarning("GameNode " + gameObject.name + " connectedNodes contained a gameobject with no GameNode component: " + connectedNode.name);
                }
            }

            gameNodesInRange = createGameNodesInRangeList();
            currentValue = new NodeValue(initialOwnerId, maxValue, initialValue, newOwnerId => onOwnerChange(newOwnerId));
        }

        private void Start() {
            onOwnerChange(initialOwnerId);
            nodeUi.onUpdate(getViewModel());
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
            Debug.Log("onOwnerChange: " + gameObject.name + " to player " + ownerId);
            gameObject.GetComponentInChildren<MeshRenderer>().material = globals.playerMaterials[ownerId];
            gameManager.onGameNodeOwnerChange(this);
            clearConnection();
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
            changeValue(packet.getOwnerId(), 1);
            nodeUi.onUpdate(getViewModel());
        }

        public abstract void onSlowBeat();

        public abstract void onMediumBeat();

        public abstract void onFastBeat();

        public void connectToNode(GameNode node) {
            if (!node.allowsInboundConnections) {
                Debug.Log("GameNode: ERROR: attempted to connect to node that doesn't allow inbound connections");
                return;
            }
            clearConnection();
            connection = new NodeConnection(this, node);
            GetComponent<NodeConnectionIndicator>().update();
        }

        public void onSelfInteraction() {
            clearConnection();
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

        protected NodeViewModel getViewModel() {
            int max = currentValue.getMaxValue();
            Dictionary<int, int> playerStakes = currentValue.getPlayerStakes();
            List<PlayerMaterialViewModel> playerModels = new List<PlayerMaterialViewModel>();
            foreach (KeyValuePair<int, int> entry in playerStakes) {
                int value = entry.Value;
                if (value > 0) {
                    Material material = globals.playerMaterials[entry.Key];
                    playerModels.Add(new PlayerMaterialViewModel(material, value));
                }
            }
            return new NodeViewModel(currentValue.isOwned(), max, playerModels);
        }

        public NodeConnection getConnection() {
            return connection;
        }

        public void clearConnection() {
            connection = null;
            connectedNode = null;
            GetComponent<NodeConnectionIndicator>().update();
        }

        public bool isConnected(GameNode node) {
            return connection != null && connection.getOther(this) == node;
        }

        public bool isConnected() {
            return connection != null;
        }

        public GameNode getConnectedNodeIfPresent() {
            return isConnected() ? connection.getOther(this) : null;
        }

        public override string ToString() {
            return "<GameNode " + gameObject.name + " @ " + getPosition() + ">";
        }

        public Material getConnectionLineMaterial() {
            return connectionLineMaterial;
        }
    }
}
