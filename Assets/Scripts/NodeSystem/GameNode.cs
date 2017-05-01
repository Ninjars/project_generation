using System.Collections;
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

        private void Awake() {
            nodeUi = GetComponent<NodeUI>();
            nodeComponent = GetComponent<Node>();
        }

        void Start() {
            setOwnerId(ownerId);
        }

        public void setOwnerId(int activePlayerId) {
            ownerId = activePlayerId;
            gameObject.GetComponentInChildren<MeshRenderer>().material = 
                GameObject.FindWithTag("GameController").GetComponent<Globals>().playerMaterials[activePlayerId];
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
            Debug.Log("GameNode: onSelfInteraction()");
            nodeComponent.removeAllConnections();
        }

        public bool canReceivePacket() {
            return currentValue < maxValue;
        }

        public virtual void changeValue(int change) {
            currentValue = Mathf.Max(0, Mathf.Min(maxValue, currentValue + change));
        }
    }
}
