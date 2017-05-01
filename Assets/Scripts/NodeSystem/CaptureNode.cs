using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class CaptureNode : GameNodeCompoundValue {


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
    }
}
