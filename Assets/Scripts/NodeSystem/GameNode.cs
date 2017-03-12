using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(Node))]
    [RequireComponent(typeof(NodeUI))]
    public class GameNode : MonoBehaviour {

        public bool isActive = true;
        public int currentValue = 0;
        public int maxValue = 10;
        public float secondsPerIncrease = 1f;

        private float elapsedIncreaseSeconds;
        private NodeUI nodeUi;

        private void Awake() {
            nodeUi = GetComponent<NodeUI>();
        }

        void Start() {

        }

        void Update() {
            if (currentValue < maxValue) {
                elapsedIncreaseSeconds += Time.deltaTime;
                if (elapsedIncreaseSeconds >= secondsPerIncrease) {
                    currentValue++;
                    elapsedIncreaseSeconds -= secondsPerIncrease;
                    nodeUi.hasUpdate();
                }
            } else {
                elapsedIncreaseSeconds = 0;
            }
        }
    }
}
