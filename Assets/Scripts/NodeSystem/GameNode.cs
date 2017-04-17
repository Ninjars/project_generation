﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(Node))]
    [RequireComponent(typeof(NodeUI))]
    public abstract class GameNode : MonoBehaviour {

        public int currentValue = 0;
        public bool isActive = true;
        public int maxValue = 10;
        public int ownerId = -1;
        public int maxOutboundConnections = -1;

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
    }
}
