using System;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(NodeConnectionIndicator))]
    public abstract class GameNode : MonoBehaviour {
        public Material connectionLineMaterial;

        public int initialValue = 0;
        public int maxValue = 10;
        public int initialOwnerId = 0;

        public GameObject packet;

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
        }

        private void Start() {
            onOwnerChange(initialOwnerId);
            nodeUi.onUpdate(getViewModel());
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
