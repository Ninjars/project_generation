using UnityEngine;

namespace Node {
    public class GameManager : MonoBehaviour {

        public static float nodeConnectionRange = 10f;

        public float slowBeatSeconds = 4f;
        public float mediumBeatSeconds = 2f;
        public float fastBeatSeconds = 1f;

        private GameNode[] gameNodes;

        private float elapsedSlow = 0;
        private float elapsedMedium = 0;
        private float elapsedFast = 0;

        // Use this for initialization
        void Start() {
            gameNodes = FindObjectsOfType<GameNode>();
        }

        // Update is called once per frame
        void Update() {
            elapsedSlow += Time.deltaTime;
            elapsedMedium += Time.deltaTime;
            elapsedFast += Time.deltaTime;

            if (elapsedSlow >= slowBeatSeconds) {
                updateGameNodesSlowBeat();
                elapsedSlow -= slowBeatSeconds;
            }
            if (elapsedMedium >= mediumBeatSeconds) {
                updateGameNodesMediumBeat();
                elapsedMedium -= mediumBeatSeconds;
            }
            if (elapsedFast >= fastBeatSeconds) {
                updateGameNodesFastBeat();
                elapsedFast -= fastBeatSeconds;
            }
        }

        private void updateGameNodesSlowBeat() {
            foreach (GameNode node in gameNodes) {
                node.onSlowBeat();
            }
        }

        private void updateGameNodesMediumBeat() {
            foreach (GameNode node in gameNodes) {
                node.onMediumBeat();
            }
        }

        private void updateGameNodesFastBeat() {
            foreach (GameNode node in gameNodes) {
                node.onFastBeat();
            }
        }
    }
}
