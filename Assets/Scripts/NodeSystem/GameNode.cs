﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(Node))]
    [RequireComponent(typeof(NodeUI))]
    public class GameNode : MonoBehaviour {

        public bool isActive = true;
        public int currentValue = 0;
        public int maxValue = 10;
        public int ownerId = 0;
        public float secondsPerIncrease = 1f;
        public float secondsPerEmission = 1f;

        public GameObject packet;

        private float elapsedIncreaseSeconds;
        private float elapsedEmissionSeconds;
        private int emissionIndex;
        private NodeUI nodeUi;
        private Node nodeComponent;

        private void Awake() {
            nodeUi = GetComponent<NodeUI>();
            nodeComponent = GetComponent<Node>();
        }

        void Start() {
            setOwnerId(ownerId);
        }

        void Update() {
            if (currentValue < maxValue) {
                elapsedIncreaseSeconds += Time.deltaTime;
                if (elapsedIncreaseSeconds >= secondsPerIncrease) {
                    currentValue++;
                    elapsedIncreaseSeconds -= secondsPerIncrease;
                    nodeUi.hasUpdate();
                }
            } else {
                elapsedIncreaseSeconds = 0;
            }
            if (currentValue > 0) {
                elapsedEmissionSeconds += Time.deltaTime;
                if (elapsedEmissionSeconds >= secondsPerEmission) {
                    List<Node> connectedNodes = nodeComponent.getConnectedNodes();
                    if (connectedNodes.Count == 0) {
                        elapsedEmissionSeconds = secondsPerEmission;
                    } else {
                        int index = emissionIndex % connectedNodes.Count;
                        Debug.Log("GameNode: emitting packet to connection index " + index);
                        emissionIndex++;
                        sendPacketToNode(connectedNodes[index]);
                        elapsedEmissionSeconds -= secondsPerEmission;
                        currentValue--;
                        nodeUi.hasUpdate();
                    }
                }
            }
        }

        public void setOwnerId(int activePlayerId) {
            ownerId = activePlayerId;
            gameObject.GetComponentInChildren<MeshRenderer>().material = GameObject.FindWithTag("GameController").GetComponent<Globals>().playerMaterials[activePlayerId];
        }

        public bool isOwnedBySamePlayer(GameNode otherNode) {
            return otherNode.ownerId == ownerId;
        }

        private void sendPacketToNode(Node node) {
            GameNode gameNode = node.gameObject.GetComponent<GameNode>();
            GameObject packetObj = Instantiate(packet, transform.position, transform.rotation);
            Packet packetScript = packetObj.GetComponent<Packet>();
            packetScript.target = gameNode;
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }

        public void onPacket(Packet packet) {
            currentValue = Mathf.Max(currentValue--, 0);
            nodeUi.hasUpdate();
        }

        public bool hasConnection(GameNode otherNode) {
            return nodeComponent.hasConnection(otherNode.nodeComponent);
        }

        public void removeConnection(GameNode otherNode) {
            nodeComponent.removeConnection(otherNode.nodeComponent);
        }

        public void addConnection(GameNode otherNode) {
            nodeComponent.addConnection(otherNode.nodeComponent);
        }
    }
}
