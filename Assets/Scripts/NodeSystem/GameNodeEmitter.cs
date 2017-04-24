using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public abstract class GameNodeEmitter : GameNode {
        public float secondsPerEmission = 0.5f;
        private float elapsedEmissionSeconds;
        private int emissionIndex;

        protected virtual void updateEmission() {
            if (currentValue > 0) {
                elapsedEmissionSeconds += Time.deltaTime;
                if (elapsedEmissionSeconds >= secondsPerEmission) {
                    List<Node> connectedNodes = nodeComponent.getConnectedNodes();
                    if (connectedNodes.Count == 0) {
                        elapsedEmissionSeconds = secondsPerEmission;
                    } else {
                        int index = emissionIndex % connectedNodes.Count;
                        emissionIndex++;
                        if (sendPacketToNode(connectedNodes[index])) {
                            elapsedEmissionSeconds -= secondsPerEmission;
                            changeValue(-1);
                            nodeUi.hasUpdate();
                        }
                    }
                }
            }
        }

        private bool sendPacketToNode(Node node) {
            GameNode gameNode = node.gameObject.GetComponent<GameNode>();
            bool differentTeam = !this.isOwnedBySamePlayer(gameNode);
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
    }
}
