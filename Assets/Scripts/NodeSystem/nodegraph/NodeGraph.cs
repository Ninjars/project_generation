using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}

