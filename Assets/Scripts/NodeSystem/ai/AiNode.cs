using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class AiNode {
        public readonly GameNode node;
        private List<GameNode> targets = new List<GameNode>();

        public AiNode(GameNode node) {
            this.node = node;
            foreach (GameNode target in node.getGameNodesInRange()) {
                if (target.getOwnerId() != node.getOwnerId()) {
                    targets.Add(target);
                }
            }
        }

        public bool hasTargets() {
            return targets.Count > 0;
        }

        public GameNode getBestConnection() {
            return findBestGameNodeToTarget(node);
        }

        private GameNode findBestGameNodeToTarget(GameNode queryNode) {
            List<GameNode> nodesInRange = queryNode.getGameNodesInRange();
            // find closest unowned node with lowest value as initial target
            GameNode bestTarget = null;
            float bestWeighting = -1;
            foreach (GameNode node in nodesInRange) {
                float weighting = getWeightingForNodeTarget(queryNode, node);
                if (weighting > bestWeighting) {
                    bestTarget = node;
                    bestWeighting = weighting;
                }
            }
            // if we have an unowned node in range, attempt to take it!
            if (bestTarget != null) {
                return bestTarget;
            }
            return null;
        }

        /**
         * get normalised fraction balancing distance and current value difference:
         * prefer nodes that are weaker
         * prefer close hostile nodes
         * prefer distant friendly nodes
         * prefer neutral nodes over enemy nodes over friendly nodes
         * 
         * avoid friendly nodes that are maxed out
         * avoid friendly nodes that are connected to this node
         * avoid hostile nodes that have greater current value than this node
         */
        private float getWeightingForNodeTarget(GameNode queryNode, GameNode targetNode) {
            bool isFriendly = queryNode.getOwnerId() == targetNode.getOwnerId();
            if (isFriendly && targetNode.isConnected(queryNode)) {
                return -0.5f;
            }
            if (isFriendly && targetNode.getOwnerValue() == targetNode.maxValue) {
                return -0.5f;
            }
            bool isNeutral = targetNode.getOwnerId() == GameManager.NEUTRAL_PLAYER_ID;
            if (!isNeutral && !isFriendly && targetNode.getOwnerValue() > queryNode.getOwnerValue()) {
                return 0;
            }

            float distanceFactor = Mathf.Min(1, Mathf.Max(0, Vector3.Distance(targetNode.getPosition(), queryNode.getPosition()) / GameManager.nodeConnectionRange));
            if (!isFriendly) {
                distanceFactor = 1f - distanceFactor;
            }

            float weaknessFactor = Mathf.Min(1, Mathf.Max(0, 1f - (targetNode.getOwnerValue() / (float) targetNode.maxValue)));

            float alignmentFactor = isFriendly ? 0 : isNeutral ? 1 : 0.5f;
            float val = 0.15f * distanceFactor +  0.5f * weaknessFactor + 0.35f * alignmentFactor;
            return val;
        }
    }
}
