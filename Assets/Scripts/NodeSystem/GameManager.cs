using System;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class GameManager : MonoBehaviour {
        internal static readonly int NEUTRAL_PLAYER_ID = 0;

        public static float nodeConnectionRange = 10f;

        public float slowBeatSeconds = 4f;
        public float mediumBeatSeconds = 2f;
        public float fastBeatSeconds = 1f;

        private GameNode[] gameNodes;
        private BaseAi[] aiPlayers;
        private NodeGraph nodeGraph;

        private float elapsedSlow = 0;
        private float elapsedMedium = 0;
        private float elapsedFast = 0;

        // Use this for initialization
        void Awake() {
            gameNodes = FindObjectsOfType<GameNode>();
            aiPlayers = FindObjectsOfType<BaseAi>();
            nodeGraph = new NodeGraph();
            nodeGraph.populate(gameNodes);
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
                aiDecisionTick();
            }
            if (elapsedFast >= fastBeatSeconds) {
                updateGameNodesFastBeat();
                elapsedFast -= fastBeatSeconds;
            }
        }

        private void aiDecisionTick() {
            foreach (BaseAi ai in aiPlayers) {
                ai.onDecisionTick();
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

        public void onGameNodeOwnerChange(GameNode node) {
            if (aiPlayers == null) {
                return;
            }
            foreach (BaseAi ai in aiPlayers) {
                ai.onGameNodeOwnerChange(node);
            }
        }

        public List<GameNode> getGameNodesForPlayer(int playerId) {
            List<GameNode> playerNodes = new List<GameNode>();
            foreach (GameNode node in gameNodes) {
                if (node.getOwnerId() == playerId) {
                    playerNodes.Add(node);
                }
            }
            return playerNodes;
        }

        public NodeGraph getNodeGraph() {
            return nodeGraph;
        }
    }
}
