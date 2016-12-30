using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneActionHarvestResources : GOAPAction, IActionPerformed {

    private bool completed = false;

	public DroneActionHarvestResources() {
		addEffect(DroneAgent.PLAN_COLLECTED, true);
        cost = 100f;
    }

    public override void reset() {
        completed = false;
        target = null;
    }

    public override bool isDone() {
//		Debug.Log ("isDone() " + completed);
        return completed;
    }

    public override bool requiresInRange() {
        return true;
    }

    public override bool checkProceduralPrecondition(GameObject agent) {
        target = updateTarget(agent, target);
        return target != null;
    }

    private GameObject updateTarget(GameObject agent, GameObject currentTarget) {
        DroneAgent drone = agent.GetComponent<DroneAgent>();
        if (drone == null) {
            throw new MissingComponentException("agent doesn't have drone component!");
        }
        List<GameObject> allTargets = GlobalRegister.resources;
        GameObject best = currentTarget;
        double bestScore = 0;
        if (currentTarget != null) {
            // weight towards current target
            bestScore = getTargetScore(drone, currentTarget) * 1.2;
        }

        foreach (GameObject t in allTargets) {
            double score = getTargetScore(drone, t);
            if (score > bestScore) {
                best = t;
                bestScore = score;
            }
        }
        return best;
    }

    /*
     * Returns a score for the passed target based on distance, resources available and 
     * sustainability of harvesting from it. 
     * returns -1 for cannot harvest, 0 for worst, 1 for best
     */
    private double getTargetScore(DroneAgent agent, GameObject target) {
        Resource resource = target.GetComponent<Resource>();
        if (resource == null) {
            // don't select targets that can't be harvested
            Debug.Log("Non-resource object considered for DroneActionHarvestResource target! " + target);
            return -1;
        }

        double distanceFactor = 1 / Vector3.Distance(target.transform.position, transform.position);

        // prefer abundant resources over sufficient resources, and strongly prefer sufficient over insufficient 
        // - 20%
        double quantityFactor;
        int resourcesNeeded = agent.resourceCapacity - agent.currentResourceCount;
        if (resource.currentValue >= resourcesNeeded * 2) {
            quantityFactor = 1;
        } else if (resource.currentValue >= resourcesNeeded) {
            quantityFactor = 0.85;
        } else {
            quantityFactor = 0.2;
        }

        // prefer sustainably harvestable resource 
        double sustainabilityWeight;
        if (resource.destroyIfDepleted) {
            // allow margin for restoration when abstaining from depletable resources
            sustainabilityWeight = resource.currentValue >= resourcesNeeded * 1.35 ? 1 : 0;
        } else {
            sustainabilityWeight = 1;
        }

        // weight each component differently
        return (0.5 * sustainabilityWeight) + (0.3 * distanceFactor) + (0.2 * quantityFactor);
    }

	public override bool perform(GameObject agent) {
        DroneAgent drone = agent.GetComponent<DroneAgent>();
		bool success = drone.harvest(target, this);
//		Debug.Log ("perform() " + success);
        return success;
    }

	public void onActionPerformed() {
		completed = true;
	}
}
