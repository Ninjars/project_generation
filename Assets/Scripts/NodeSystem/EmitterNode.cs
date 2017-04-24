using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class EmitterNode : GameNodeEmitter {
        public float secondsPerIncrease = 1f;
        private float elapsedIncreaseSeconds;

        void Update() {
            if (currentValue < maxValue) {
                elapsedIncreaseSeconds += Time.deltaTime;
                if (elapsedIncreaseSeconds >= secondsPerIncrease) {
                    changeValue(1);
                    elapsedIncreaseSeconds -= secondsPerIncrease;
                    nodeUi.hasUpdate();
                }
            } else {
                elapsedIncreaseSeconds = 0;
            }
            base.updateEmission();
        }

        public override void onPacket(Packet packet) {
            bool ownerMatches = packet.getOwnerId() == getOwnerId();
            if (ownerMatches) {
                changeValue(1);
            } else {
                if (currentValue == 0) {
                    elapsedIncreaseSeconds = 0;
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
