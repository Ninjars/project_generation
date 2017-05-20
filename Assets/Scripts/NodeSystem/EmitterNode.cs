using UnityEngine;

namespace Node {
    public class EmitterNode : GameNodeEmitter {
        [Range(0f, 1f)]
        [Tooltip("% below which nothing will be emitted")]
        public float reserveThreshold = 0.2f;

        public override void onSlowBeat() {
            if (!isNeutral()) {
                changeValue(getOwnerId(), 1);
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
            return getOwnerValue() / (float)maxValue >= reserveThreshold;
        }
    }
}
