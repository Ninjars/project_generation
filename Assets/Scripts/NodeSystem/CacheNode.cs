using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class CacheNode : GameNodeEmitter {

        void Update() {
            base.updateEmission();
        }

        public override void onPacket(Packet packet) {
            bool ownerMatches = packet.getOwnerId() == getOwnerId();
            if (ownerMatches) {
                currentValue = Mathf.Min(maxValue, currentValue + 1);
            } else {
                if (currentValue == 0) {
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
