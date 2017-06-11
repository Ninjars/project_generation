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

        public int initialOwnerId = 0;

        [Tooltip("once owned, this node will generate this many packets per slow tick")]
        public int packetsPerTick = 1;

        public GameObject packet;

        private PathGen.PathwayGenerator pathGenerator;
        private GameObject pathMeshContainer;

        protected NodeUI nodeUi;

        protected GameManager gameManager;
        private Globals globals;
        private NodeValue currentValue;
        private NodeConnection connection;

        private void Awake() {
            nodeUi = GetComponent<NodeUI>();
            gameManager = FindObjectOfType<GameManager>();
            globals = FindObjectOfType<Globals>();
            currentValue = new NodeValue(initialOwnerId, maxValue, initialValue, newOwnerId => onOwnerChange(newOwnerId));
            pathGenerator = gameObject.GetComponent<PathGen.PathwayGenerator>();
        }

        private void Start() {
            onOwnerChange(initialOwnerId);
            nodeUi.onUpdate(getViewModel());
        }

        #region ownership handling
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
        #endregion


        #region packet handling
        public void onPacket(Packet packet) {
            changeValue(packet.getOwnerId(), 1);
            nodeUi.onUpdate(getViewModel());
        }

        public bool canReceivePacket() {
            return currentValue.getValueForPlayer(getOwnerId()) < currentValue.getMaxValue();
        }

        public void onSlowBeat() {
            if (!isNeutral()) {
                changeValue(getOwnerId(), packetsPerTick);
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
                changeValue(getOwnerId(), -1);
                nodeUi.onUpdate(getViewModel());
            }
        }

        private bool sendPacketToNode(GameNode gameNode) {
            bool differentTeam = !isOwnedBySamePlayer(gameNode);
            if (differentTeam || gameNode.canReceivePacket()) {
                GameObject packetObj = Instantiate(packet, transform.position, transform.rotation);
                Packet packetScript = packetObj.GetComponent<Packet>();
                packetScript.target = gameNode;
                packetScript.setOwnerId(getOwnerId());
                return true;
            } else {
                return false;
            }
        }
        #endregion


        #region value handling
        public virtual void changeValue(int playerId, int change) {
            currentValue.changePlayerValue(playerId, change);
        }

        public int getOwnerValue() {
            return currentValue.getValueForPlayer(getOwnerId());
        }
        #endregion


        #region upgrade handling
        public void onSelfInteraction() {
            attemptUpgrade();
        }

        private void attemptUpgrade() {
            if (upgradeCost > 0 && currentValue.isUncontested() && currentValue.isOwned() && currentValue.getTotalValue() >= upgradeCost) {
                currentValue.changePlayerValue(getOwnerId(), -upgradeCost);
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
        #endregion

        public override string ToString() {
            return "<GameNode " + gameObject.name + " @ " + getPosition() + ">";
        }
    }
}
