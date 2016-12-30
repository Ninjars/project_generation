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
        return drone.getResourceTargetLocation();
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
