using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class NodeInteractionManager {

        private static Node mFirstNode = null;

        public static void onInteraction(Node node) {
            Debug.Log("NodeInteractionManager: onInteraction() " + node);
            if (node == null) {
                clearInteraction();
            } else if (mFirstNode == null) {
                beginInteraction(node);
            } else {
                endInteraction(node);
            }
        }

        private static void beginInteraction(Node startNode) {
            mFirstNode = startNode;
        }

        private static void clearInteraction() {
            mFirstNode = null;
        }

        private static void endInteraction(Node endNode) {
            // TODO
        }
    }
}
