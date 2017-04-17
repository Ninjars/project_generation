using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class EmitterNode : GameNode {
        public float secondsPerIncrease = 1f;
        public float secondsPerEmission = 0.5f;
        private float elapsedIncreaseSeconds;
        private float elapsedEmissionSeconds;
        private int emissionIndex;

        void Update() {
            if (currentValue < maxValue) {
                elapsedIncreaseSeconds += Time.deltaTime;
                if (elapsedIncreaseSeconds >= secondsPerIncrease) {
                    incrementValue();
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
                        emissionIndex++;
                        sendPacketToNode(connectedNodes[index]);
                        elapsedEmissionSeconds -= secondsPerEmission;
                        currentValue--;
                        nodeUi.hasUpdate();
                    }
                }
            }
        }

        private void incrementValue() {
            currentValue = Mathf.Min(maxValue, currentValue + 1);
        }

        private void sendPacketToNode(Node node) {
            GameNode gameNode = node.gameObject.GetComponent<GameNode>();
            GameObject packetObj = Instantiate(packet, transform.position, transform.rotation);
            Packet packetScript = packetObj.GetComponent<Packet>();
            packetScript.target = gameNode;
            packetScript.setOwnerId(getOwnerId());
        }

        public override void onPacket(Packet packet) {
            bool ownerMatches = packet.getOwnerId() == getOwnerId();
            if (ownerMatches) {
                incrementValue();
            } else {
                if (currentValue == 0) {
                    elapsedIncreaseSeconds = 0;
                    nodeComponent.removeAllConnections();
                    setOwnerId(packet.getOwnerId());
                } else {
                    currentValue = Mathf.Max(currentValue - 1, 0);
                }
            }
            nodeUi.hasUpdate();
        }
    }
}
