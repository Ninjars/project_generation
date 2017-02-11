using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

    public List<GameObject> linkToNodesObjs;
    private List<Node> knownNodes;

    void Awake () {
        knownNodes = new List<Node>();
        foreach (GameObject node in linkToNodesObjs) {
            Node nodeComponent = node.GetComponent<Node>();
            if (nodeComponent != null) {
                knownNodes.Add(nodeComponent);
            } else {
                Debug.LogWarning("Node.linkToNodesObjs contained a gameobject with no Node component");
            }
        }
    }
	
	void Update () {
		
	}

    public List<Node> getKnownNodes() {
        return knownNodes;
    }
}
