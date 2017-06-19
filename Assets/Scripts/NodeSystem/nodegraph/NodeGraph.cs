using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Node {
    public class NodeGraph {
        private Dictionary<GameNode, IList<GameNode>> connectionsForNodes;
        private HashSet<NodeConnection> uniqueConnections;

        public void populate(GameNode[] nodes) {
            connectionsForNodes = new Dictionary<GameNode, IList<GameNode>>();
            uniqueConnections = new HashSet<NodeConnection>();
            foreach (GameNode node in nodes) {
                List<GameNode> connectedNodes = new List<GameNode>();
                foreach (GameNode other in nodes) {
                    if (other == node) {
                        continue;
                    }
                    // check to see if other node has already been processed and has checked for connections here
                    // to avoid wasting time re-measuring each connection
                    IList<GameNode> otherGraph = new List<GameNode>();
                    bool hasOtherGraph = connectionsForNodes.TryGetValue(other, out otherGraph);
                    bool otherConnectsToThis = hasOtherGraph ? otherGraph.Contains(node) : false;
                    if (otherConnectsToThis || (!hasOtherGraph && isNodeInRange(node, other))) {
                        connectedNodes.Add(other);
                        uniqueConnections.Add(new NodeConnection(node, other));
                    }
                }
                connectionsForNodes.Add(node, new List<GameNode>(connectedNodes).AsReadOnly());
            }
            Debug.Log("NodeGraph:populate " + connectionsForNodes.Count);
            Debug.Log("NodeGraph:> connections " + uniqueConnections.Count);
        }

        private bool isNodeInRange(GameNode a, GameNode b) {
            return Vector3.Distance(a.getPosition(), b.getPosition()) <= GameManager.nodeConnectionRange;
        }

        public IList<GameNode> getConnectedNodes(GameNode node) {
            return connectionsForNodes[node];
        }

        public List<GameNode> findShortestFriendlyPath(GameNode fromNode, GameNode toNode) {
            Debug.Assert(fromNode.getOwnerId() == toNode.getOwnerId());

            List<GameNode> checkedNodes = new List<GameNode>();
            List<List<GameNode>> currentWorkingPaths = new List<List<GameNode>>();
            List<List<GameNode>> nextGenerationPaths = new List<List<GameNode>>();
            checkedNodes.Add(fromNode);

            foreach (GameNode node in getNewFriendlyConnectableNodes(fromNode, checkedNodes)) {
                List<GameNode> currentPath = new List<GameNode>();
                currentPath.Add(fromNode);
                currentPath.Add(node);
                if (node == toNode) {
                    return currentPath;
                } else {
                    currentWorkingPaths.Add(currentPath);
                }
            }
            for (int generation = 0; generation < connectionsForNodes.Count; generation++) {
                foreach (List<GameNode> path in currentWorkingPaths) {
                    GameNode workingNode = path[path.Count - 1];
                    checkedNodes.Add(workingNode);
                    foreach (GameNode node in getNewFriendlyConnectableNodes(workingNode, checkedNodes)) {
                        List<GameNode> workingPath = new List<GameNode>();
                        workingPath.AddRange(path);
                        workingPath.Add(node);
                        if (node == toNode) {
                            return workingPath;
                        } else {
                            nextGenerationPaths.Add(workingPath);
                        }
                    }
                    currentWorkingPaths.Clear();
                    currentWorkingPaths.AddRange(nextGenerationPaths);
                    nextGenerationPaths.Clear();
                }
            }
            return null;
        }

        private List<GameNode> getNewFriendlyConnectableNodes(GameNode a, List<GameNode> checkedNodes) {
            return connectionsForNodes[a]
                .Where(node => node.getOwnerId() == a.getOwnerId() && !checkedNodes.Contains(node))
                .ToList();
        }
    }
}

