using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(GameNode))]
    public abstract class NodeUI : MonoBehaviour {
        public Material activeValueMaterial;
        public Material passiveValueMaterial;

        [Range(0.1f, 3f)]
        public float radius = 1.0f;

        protected bool shouldUpdate = true;
        protected GameObject uiRoot;
        protected GameNode gameNode;

        void Awake() {
            gameNode = GetComponent<GameNode>();
            uiRoot = new GameObject();
            uiRoot.transform.SetParent(gameObject.transform);
            uiRoot.transform.position = gameObject.transform.position;
            uiRoot.name = "uiRoot";
            init();
        }

        protected virtual void init() { }

        void Start() {
            updateRenderer();
        }

        private void Update() {
            updateRenderer();
        }

        public void hasUpdate() {
            shouldUpdate = true;
        }

        public abstract void updateRenderer();
    }
}
