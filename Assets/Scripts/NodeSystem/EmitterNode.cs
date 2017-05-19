﻿using UnityEngine;

namespace Node {
    public class EmitterNode : GameNodeEmitter {

        [Tooltip("Each hostile packet received will add one lock that is cleared every medium beat, up to this many locks")]
        public int maxMediumBeatLockDuration = 3;
        [Range(0f, 1f)]
        [Tooltip("% above which emitter will emit at fastest speed")]
        public float fastThreshold = 0.66f;
        [Range(0f, 1f)]
        [Tooltip("% above which emitter will emit at medium speed")]
        public float mediumThreshold = 0.33f;
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
            if (isEmittingSlow()) {
                base.onEmit();
            }
        }

        private bool isEmittingSlow() {
            float currentWeight = currentValue / (float)maxValue;
            return currentWeight < mediumThreshold && currentWeight > reserveThreshold;
        }

        public override void onMediumBeat() {
            if (isEmittingMedium()) {
                base.onEmit();
            }
        }

        private bool isEmittingMedium() {
            float currentWeight = currentValue / (float)maxValue;
            return currentWeight < fastThreshold && currentWeight >= mediumThreshold;
        }

        public override void onFastBeat() {
            if (isEmittingFast()) {
                base.onEmit();
            }
        }

        private bool isEmittingFast() {
            return currentValue / (float)maxValue >= fastThreshold;
        }
    }
}
