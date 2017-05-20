using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public abstract class GameNodeEmitter : GameNode {
        private int emissionIndex;

        protected virtual void onEmit() {
            if (getOwnerValue() > 0) {
                List<Node> connectedNodes = nodeComponent.getConnectedNodes();
                if (connectedNodes.Count > 0) {
                    int index = emissionIndex % connectedNodes.Count;
                    emissionIndex++;
                    if (sendPacketToNode(connectedNodes[index])) {
                        changeValue(getOwnerId(), -1);
                        nodeUi.hasUpdate();
                    }
                }
            }
        }

        private bool sendPacketToNode(Node node) {
            GameNode gameNode = node.gameObject.GetComponent<GameNode>();
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
    }
}
