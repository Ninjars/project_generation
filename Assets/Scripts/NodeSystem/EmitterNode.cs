using UnityEngine;

namespace Node {
    public class EmitterNode : GameNodeEmitter {

        [Tooltip("Each hostile packet received will add one lock that is cleared every medium beat, up to this many locks")]
        public int maxMediumBeatLockDuration = 3;
        [Range(0f, 1f)]
        [Tooltip("% below which nothing will be emitted")]
        public float reserveThreshold = 0.2f;

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
            if (!isNeutral()) {
                changeValue(1);
                nodeUi.hasUpdate();
            }
        }

        public override void onMediumBeat() {
            // nothing to see here
        }

        public override void onFastBeat() {
            if (isEmittingFast()) {
                base.onEmit();
            }
        }

        private bool isEmittingFast() {
            return currentValue / (float)maxValue >= reserveThreshold;
        }
    }
}
