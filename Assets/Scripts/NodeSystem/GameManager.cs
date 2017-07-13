using System;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(Player))]
    public class GameManager : MonoBehaviour {
        private static readonly int NEUTRAL_PLAYER_ID = 0;

        public static float nodeConnectionRange = 10f;

        public float slowBeatSeconds = 4f;
        public float mediumBeatSeconds = 2f;
        public float fastBeatSeconds = 1f;

        private GameNode[] gameNodes;
        private BaseAi[] aiPlayers;
        private NodeGraph nodeGraph;
        private Player neutralPlayer;

        private float elapsedSlow = 0;
        private float elapsedMedium = 0;
        private float elapsedFast = 0;


        // Use this for initialization
        void Awake() {
            neutralPlayer = GetComponent<Player>();
            neutralPlayer.id = NEUTRAL_PLAYER_ID;
            gameNodes = FindObjectsOfType<GameNode>();
            aiPlayers = FindObjectsOfType<BaseAi>();
            nodeGraph = new NodeGraph();
            nodeGraph.populate(gameNodes);
        }

        public Player getNeutralPlayer() {
            return neutralPlayer;
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

        public void onGameNodeOwnerChange(Player oldOwner, GameNode node) {
            Debug.Log("GameManager: onGameNodeOwnerChange " + oldOwner + " > " + node); 
            if (oldOwner != null && !oldOwner == neutralPlayer) {
                checkNodeConnectionsForPlayer(oldOwner);
            }
            if (aiPlayers != null) {
                foreach (BaseAi ai in aiPlayers) {
                    ai.onGameNodeOwnerChange(node);
                }
            }
        }

        public List<GameNode> getGameNodesForPlayer(Player player) {
            Debug.Log("GameManager: getGameNodesForPlayer " + player);
            List<GameNode> playerNodes = new List<GameNode>();
            foreach (GameNode node in gameNodes) {
                if (node.getOwningPlayer() == player) {
                    playerNodes.Add(node);
                }
            }
            Debug.Log("GameManager: returning " + playerNodes.Count);
            return playerNodes;
        }

        public NodeGraph getNodeGraph() {
            return nodeGraph;
        }

        private void checkNodeConnectionsForPlayer(Player player) {
            GameNode homeNode = player.getHomeNode();
            List<GameNode> nodesToCheck = getGameNodesForPlayer(player);
            nodesToCheck.Remove(homeNode);
            List<GameNode> nodesToRelease = new List<GameNode>();
            while (nodesToCheck.Count > 0) {
                Debug.Log("GameManager: checkNodeConnectionsForPlayer: nodesToCheck " + nodesToCheck.Count);
                GameNode checkNode = nodesToCheck[0];
                List<GameNode> routeHome = nodeGraph.findShortestFriendlyPath(homeNode, checkNode);
                if (routeHome == null) {
                    nodesToRelease.Add(checkNode);
                    nodesToCheck.Remove(checkNode);
                } else {
                    nodesToCheck.RemoveAll(node => routeHome.Contains(node));
                }
            }
            foreach (GameNode node in nodesToRelease) {
                node.onDisconnectedFromHome();
            }
        }
    }
}
