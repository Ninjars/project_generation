using System;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(NodeConnectionIndicator))]
    public class GameNode : MonoBehaviour {
        public Material connectionLineMaterial;

        public int initialValue = 0;
        public int maxValue = 20;
        [Range(0f, 1f)]
        [Tooltip("% of maxValue below which nothing will be emitted")]
        public float reserveThreshold = 0.2f;
        [Tooltip("Number of packets that are consumed to upgrade. Negative number indicates cannot upgrade.")]
        public int upgradeCost = 15;

        [Tooltip("once owned, this node will generate this many packets per slow tick")]
        public int packetsPerTick = 1;

        public GameObject packet;

        private PathGen.PathwayGenerator pathGenerator;
        private GameObject pathMeshContainer;

        protected NodeUI nodeUi;

        protected GameManager gameManager;
        private NodeValue currentValue;
        private NodeConnection connection;
        private List<GameNode> pathToHome;
        private Player owningPlayer;

        private void Awake() {
            nodeUi = GetComponent<NodeUI>();
            gameManager = FindObjectOfType<GameManager>();
            owningPlayer = GetComponent<Player>();
            if (owningPlayer == null) {
                owningPlayer = gameManager.getNeutralPlayer();
            }
            currentValue = new NodeValue(gameManager, owningPlayer, maxValue, initialValue, newOwnerId => onOwnerChange(newOwnerId));
            pathGenerator = gameObject.GetComponent<PathGen.PathwayGenerator>();
        }

        private void Start() {
            onOwnerChange(owningPlayer);
            nodeUi.onUpdate(getViewModel());
        }

        #region ownership handling
        private void onOwnerChange(Player newOwner) {
            Debug.Log("onOwnerChange: " + gameObject.name + " to player " + newOwner);
            gameObject.GetComponentInChildren<MeshRenderer>().material = newOwner.getNodeMaterial();
            gameManager.onGameNodeOwnerChange(this);
            clearConnection();
        }

        public bool isOwnedBySamePlayer(GameNode otherNode) {
            return otherNode.getOwningPlayer() == getOwningPlayer();
        }

        public Player getOwningPlayer() {
            return currentValue.getOwningPlayer();
        }

        protected bool isNeutral() {
            return getOwningPlayer().isNeutralPlayer();
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }
        #endregion


        #region packet handling
        public void onPacket(Packet packet) {
            changeValue(packet.getOwner(), 1);
            nodeUi.onUpdate(getViewModel());
        }

        public bool canReceivePacket() {
            return currentValue.getValueForPlayer(getOwningPlayer()) < currentValue.getMaxValue();
        }

        public void onSlowBeat() {
            if (!isNeutral()) {
                changeValue(getOwningPlayer(), packetsPerTick);
                nodeUi.onUpdate(getViewModel());
            }
        }

        public void onMediumBeat() {
            // nothing to see here
        }

        public void onFastBeat() {
            if (isEmittingFast()) {
                onEmit();
            }
        }

        private bool isEmittingFast() {
            return getOwnerValue() / (float)maxValue >= reserveThreshold;
        }

        private void onEmit() {
            GameNode connectedNode = getConnectedNodeIfPresent();
            if (connectedNode != null && sendPacketToNode(connectedNode)) {
                changeValue(getOwningPlayer(), -1);
                nodeUi.onUpdate(getViewModel());
            }
        }

        private bool sendPacketToNode(GameNode gameNode) {
            bool differentTeam = !isOwnedBySamePlayer(gameNode);
            if (differentTeam || gameNode.canReceivePacket()) {
                GameObject packetObj = Instantiate(packet, transform.position, transform.rotation);
                Packet packetScript = packetObj.GetComponent<Packet>();
                packetScript.target = gameNode;
                packetScript.setOwner(getOwningPlayer());
                return true;
            } else {
                return false;
            }
        }
        #endregion


        #region value handling
        public virtual void changeValue(Player player, int change) {
            currentValue.changePlayerValue(player, change);
        }

        public int getOwnerValue() {
            return currentValue.getValueForPlayer(getOwningPlayer());
        }
        #endregion


        #region upgrade handling
        public void onSelfInteraction() {
            attemptUpgrade();
        }

        private void attemptUpgrade() {
            if (upgradeCost > 0 && currentValue.isUncontested() && currentValue.isOwned() && currentValue.getTotalValue() >= upgradeCost) {
                currentValue.changePlayerValue(getOwningPlayer(), -upgradeCost);
                currentValue.setMaxValue(currentValue.getMaxValue() * 2);
                packetsPerTick *= 2;
                upgradeCost = -1;
                nodeUi.onUpdate(getViewModel());
            }
        }
        #endregion


        #region connection handling
        public void connectToNode(GameNode node) {
            connection = new NodeConnection(this, node);
            GetComponent<NodeConnectionIndicator>().update();

            if (pathGenerator != null) {
                if (pathMeshContainer != null) {
                    GameObject.Destroy(pathMeshContainer);
                }
                pathMeshContainer = pathGenerator.createConnection(gameObject, node.transform.position);
            }
        }

        public NodeConnection getConnection() {
            return connection;
        }

        public void clearConnection() {
            connection = null;
            GetComponent<NodeConnectionIndicator>().update();

            if (pathMeshContainer != null) {
                GameObject.Destroy(pathMeshContainer);
            }
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
        #endregion


        #region ui handling
        public Material getConnectionLineMaterial() {
            return connectionLineMaterial;
        }

        protected NodeViewModel getViewModel() {
            int max = currentValue.getMaxValue();
            Dictionary<Player, int> playerStakes = currentValue.getPlayerStakes();
            List<PlayerMaterialViewModel> playerModels = new List<PlayerMaterialViewModel>();
            foreach (KeyValuePair<Player, int> entry in playerStakes) {
                int value = entry.Value;
                if (value > 0) {
                    Material material = entry.Key.getNodeMaterial();
                    playerModels.Add(new PlayerMaterialViewModel(material, value));
                }
            }
            return new NodeViewModel(currentValue.isOwned(), max, playerModels);
        }
        #endregion

        public override string ToString() {
            return "<GameNode " + gameObject.name + " @ " + getPosition() + ">";
        }
    }
}
