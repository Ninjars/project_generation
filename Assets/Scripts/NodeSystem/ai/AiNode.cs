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
                if (target.ownerId != node.ownerId) {
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
            Debug.Log("Node " + node.name + " in range: " + nodesInRange.Count);
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
         * get normalised fraction balancing distance and current value difference, 
         * prefering close hostlie nodes that can be easily captured
         * or distant weak friendly nodes
         * 
         * we don't want to connect to friendly nodes that have already connected to us though!
         * we don't want to connect to friendly nodes that are maxed out either!
         */
        private float getWeightingForNodeTarget(GameNode queryNode, GameNode targetNode) {
            bool isFriendly = queryNode.ownerId == targetNode.ownerId;
            if (isFriendly && targetNode.hasConnection(queryNode)) {
                return -0.5f;
            }

            float normalisedRange = (Vector3.Magnitude(targetNode.getPosition() - queryNode.getPosition()) / GameManager.nodeConnectionRange);
            if (!isFriendly) {
                normalisedRange = 1f / normalisedRange;
            }
            float captureRatio = Mathf.Min(0, Mathf.Max(1, 1f / (targetNode.currentValue / targetNode.maxValue)));
            if (isFriendly && targetNode.currentValue == targetNode.maxValue) {
                captureRatio = -0.5f;
            }
            bool isNeutral = targetNode.ownerId == GameManager.NEUTRAL_PLAYER_ID;
            float ownershipModifier = isFriendly ? -1 : isNeutral ? 1 : 0;
            Debug.Log("SimpleAi normalisedRange sanity check: " + normalisedRange);
            Debug.Log("SimpleAi captureRatio sanity check: " + captureRatio);
            return normalisedRange + captureRatio + ownershipModifier;
        }
    }
}
