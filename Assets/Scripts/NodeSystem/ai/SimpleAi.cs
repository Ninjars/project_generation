using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(Player))]
    public class SimpleAi : BaseAi {

        private List<AiNode> ownedNodes = new List<AiNode>();

        private bool targetsInRange;
        private Player player;

        public override void Awake() {
            base.Awake();
            player = gameObject.GetComponent<Player>();
        }

        // Use this for initialization
        void Start() {
            updateOwnedNodes();
        }

        // Update is called once per frame
        void Update() {

        }

        private void updateNodeConnections() {
            foreach (AiNode node in ownedNodes) {
                GameNode connectionTarget = node.getBestConnection();
                if (connectionTarget != null && !node.node.isConnected(connectionTarget)) {
                    node.node.connectToNode(connectionTarget);
                }
            }
        }

        private void updateOwnedNodes() {
            List<GameNode> nodes = gameManager.getGameNodesForPlayer(player);
            ownedNodes.Clear();
            foreach (GameNode node in nodes) {
                ownedNodes.Add(new AiNode(gameManager.getNodeGraph(), node));
            }
            updateNodeConnections();
        }

        public override void onGameNodeOwnerChange(GameNode node) {
            updateOwnedNodes();
        }

        public override void onDecisionTick() {
            updateNodeConnections();
        }
    }
}
