namespace Node {
    public class EmitterNode : GameNodeEmitter {

        public int maxMediumBeatLockDuration = 3;

        private int currentLocks = 0;

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
            if (currentLocks <= 0) {
                changeValue(1);
                nodeUi.hasUpdate();
            }
        }

        public override void onMediumBeat() {
            if (currentLocks > 0) {
                currentLocks--;
            }
        }

        public override void onFastBeat() {
            base.onEmit();
        }
    }
}
