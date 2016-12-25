using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Plans what actions can be completed in order to fulfill a goal state.
 */
public class GOAPPlanner {

    /**
	 * Plan what sequence of actions can fulfill the goal.
	 * Returns null if a plan could not be found, or a list of the actions
	 * that must be performed, in order, to fulfill the goal.
	 */
    public Queue<GOAPAction> plan(GameObject agent,
                                  HashSet<GOAPAction> availableActions,
                                  Dictionary<string, object> worldState,
                                  Dictionary<string, object> goal) {
        // reset the actions so we can start fresh with them
        foreach (GOAPAction a in availableActions) {
            a.doReset();
        }

        // check what actions can run using their checkProceduralPrecondition
        HashSet<GOAPAction> usableActions = new HashSet<GOAPAction>();
        foreach (GOAPAction a in availableActions) {
            if (a.checkProceduralPrecondition(agent))
                usableActions.Add(a);
        }

        // we now have all actions that can run, stored in usableActions

        // build up the tree and record the leaf nodes that provide a solution to the goal.
        List<Node> leaves = new List<Node>();

        // build graph
        Node start = new Node(null, 0, worldState, null);
        bool success = buildGraph(start, leaves, usableActions, goal);

        if (!success) {
            // oh no, we didn't get a plan
//            Debug.Log("NO PLAN");
            return null;
        }

        // get the cheapest leaf
        Node cheapest = null;
        foreach (Node leaf in leaves) {
            if (cheapest == null)
                cheapest = leaf;
            else {
                if (leaf.runningCost < cheapest.runningCost)
                    cheapest = leaf;
            }
        }

        // get its node and work back through the parents
        List<GOAPAction> result = new List<GOAPAction>();
        Node n = cheapest;
        while (n != null) {
            if (n.action != null) {
                result.Insert(0, n.action); // insert the action in the front
            }
            n = n.parent;
        }
        // we now have this action list in correct order

        Queue<GOAPAction> queue = new Queue<GOAPAction>();
        foreach (GOAPAction a in result) {
            queue.Enqueue(a);
        }

        // hooray we have a plan!
        return queue;
    }

    /**
	 * Returns true if at least one solution was found.
	 * The possible paths are stored in the leaves list. Each leaf has a
	 * 'runningCost' value where the lowest cost will be the best action
	 * sequence.
	 */
    protected bool buildGraph(Node parent, List<Node> leaves, HashSet<GOAPAction> usableActions, Dictionary<string, object> goal) {
        bool foundOne = false;
        // go through each action available at this node and see if we can use it here
        foreach (GOAPAction action in usableActions) {
            // if the parent state has the conditions for this action's preconditions, we can use it here
			if (inState(action.Preconditions, parent.state)) {
                // apply the action's effects to the parent state
				Dictionary<string, object> currentState = populateState(parent.state, action.Effects);
                Node node = new Node(parent, parent.runningCost + action.cost, currentState, action);

                if (goalInState(goal, currentState)) {
                    // we found a solution!
                    leaves.Add(node);
                    foundOne = true;
                } else {
                    HashSet<GOAPAction> subset = actionSubset(usableActions, action);
                    bool found = buildGraph(node, leaves, subset, goal);
                    if (found)
                        foundOne = true;
                }
            }
        }
        return foundOne;
    }

    /**
	 * Create a subset of the actions excluding the removeMe one. Creates a new set.
	 */
    protected HashSet<GOAPAction> actionSubset(HashSet<GOAPAction> actions, GOAPAction removeMe) {
        HashSet<GOAPAction> subset = new HashSet<GOAPAction>();
        foreach (GOAPAction a in actions) {
            if (!a.Equals(removeMe))
                subset.Add(a);
        }
        return subset;
    }

    /*
	 * Checks if at least one goal is met. 
	 * to-do: Create a system for weighting towards paths that fulfill more goals
	 */
	protected bool goalInState(Dictionary<string, object> test, Dictionary<string, object> state) {
        bool match = false;
		foreach (string key in test.Keys) {
			if (state.ContainsKey(key) && state [key].Equals (test [key])) {
				return true;
			}
        }
        return match;
    }

    /*
	 * Check that all items in 'test' are in 'state'. If just one does not match or is not there
	 * then this returns false.
	 */
	protected bool inState(Dictionary<string, object> test, Dictionary<string, object> state) {
		foreach (string testKey in test.Keys) {
			if (!state.ContainsKey (testKey)) {
				return false;
			} else if (!test[testKey].Equals(state[testKey])) {
				return false;
			}
		}
		return true;
    }

    /**
	 * Apply the stateChange to the currentState
	 */
	protected Dictionary<string, object> populateState(Dictionary<string, object> currentState, Dictionary<string, object> stateChange) {
		Dictionary<string, object> state = new Dictionary<string, object>();
        // copy the values over as new objects
		foreach (string key in currentState.Keys) {
			state.Add(key,  currentState[key]);
        }

		foreach (string key in stateChange.Keys) {
            // add/replace state values with stateChange values
			if (state.ContainsKey(key)) {
				state [key] = stateChange [key];
			} else {
				state.Add(key, stateChange[key]);
			}
        }
        return state;
    }

    /**
	 * Used for building up the graph and holding the running costs of actions.
	 */
    protected class Node {
        public Node parent;
        public float runningCost;
		public Dictionary<string, object> state;
        public GOAPAction action;

		public Node(Node parent, float runningCost, Dictionary<string, object> state, GOAPAction action) {
            this.parent = parent;
            this.runningCost = runningCost;
            this.state = state;
            this.action = action;
        }
    }
}
