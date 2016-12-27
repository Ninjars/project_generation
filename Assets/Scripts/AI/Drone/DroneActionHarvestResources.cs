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
        target = updateTarget(target);
        return target != null;
    }

    private GameObject updateTarget(GameObject currentTarget) {
        List<GameObject> allTargets = GlobalRegister.resources;
        GameObject nearest = currentTarget;
        float nearestDistance = float.MaxValue; // todo: max value for search range?
        if (currentTarget != null) {
            nearestDistance = (nearest.transform.position - transform.position).sqrMagnitude;
        }

        foreach (GameObject t in allTargets) {
            float sqrDistance = (t.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < nearestDistance) {
                nearest = t;
                nearestDistance = sqrDistance;
            }
        }
        return nearest;
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
