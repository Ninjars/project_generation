using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class SimpleAi : BaseAi {
        public int playerId;

        private List<AiNode> ownedNodes;

        private bool targetsInRange;

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
                if (connectionTarget != null) {
                    node.node.addConnection(connectionTarget);
                }
            }
        }

        private void updateOwnedNodes() {
            List<GameNode> nodes = gameManager.getGameNodesForPlayer(playerId);
            ownedNodes = new List<AiNode>();
            foreach (GameNode node in nodes) {
                ownedNodes.Add(new AiNode(node));
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
