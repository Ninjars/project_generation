using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class Player : MonoBehaviour {

        public int id;
        private GameNode homeNode;
        private Globals globals;
        private GameManager gameManager;

        void Awake() {
            gameManager = FindObjectOfType<GameManager>();
            homeNode = GetComponent<GameNode>();
            globals = FindObjectOfType<Globals>();
        }

        public Material getNodeMaterial() {
            return globals.playerMaterials[id];
        }

        public bool isNeutralPlayer() {
            return this == gameManager.getNeutralPlayer();
        }

        public GameNode getHomeNode() {
            return homeNode;
        }
    }
}
