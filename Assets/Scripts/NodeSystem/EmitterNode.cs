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
                    incrementValue();
                    elapsedIncreaseSeconds -= secondsPerIncrease;
                    nodeUi.hasUpdate();
                }
            } else {
                elapsedIncreaseSeconds = 0;
            }
            base.updateEmission();
        }

        private void incrementValue() {
            currentValue = Mathf.Min(maxValue, currentValue + 1);
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
