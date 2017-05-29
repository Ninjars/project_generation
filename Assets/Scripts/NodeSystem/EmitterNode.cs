using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class EmitterNode : GameNode {
        [Range(0f, 1f)]
        [Tooltip("% below which nothing will be emitted")]
        public float reserveThreshold = 0.2f;

        private int emissionIndex;

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

        public override void onSlowBeat() {
            if (!isNeutral()) {
                changeValue(getOwnerId(), 1);
                nodeUi.onUpdate(getViewModel());
            }
        }

        public override void onMediumBeat() {
            // nothing to see here
        }

        public override void onFastBeat() {
            if (isEmittingFast()) {
                onEmit();
            }
        }

        private bool isEmittingFast() {
            return getOwnerValue() / (float)maxValue >= reserveThreshold;
        }
    }
}
