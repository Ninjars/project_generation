using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class CacheNode : GameNodeEmitter {
        
        public override void onPacket(Packet packet) {
            bool ownerMatches = packet.getOwnerId() == getOwnerId();
            if (ownerMatches) {
                changeValue(1);
            } else {
                if (currentValue == 0) {
                    nodeComponent.removeAllConnections();
                    setOwnerId(packet.getOwnerId());
                } else {
                    changeValue(-1);
                }
            }
            nodeUi.hasUpdate();
        }

        public override void onSlowBeat() {
        }

        public override void onMediumBeat() {
        }

        public override void onFastBeat() {
            base.onEmit();
        }
    }
}
