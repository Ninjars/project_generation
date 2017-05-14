using UnityEngine;

namespace Node {
    public abstract class BaseAi : MonoBehaviour {

        protected GameManager gameManager;

        void Awake() {
            gameManager = FindObjectOfType<GameManager>();
        }

        public abstract void onGameNodeOwnerChange(GameNode node);

        public abstract void onDecisionTick();
    }
}
