using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneActionDeliverResources : GOAPAction {

    private bool completed = false;

    public DroneActionDeliverResources() {
		addPrecondition(DroneAgent.PLAN_COLLECTED, true);
		addEffect(DroneAgent.PLAN_DELIVER, true);
        cost = 100f;
    }

    public override void reset() {
        completed = false;
        target = null;
    }

    public override bool isDone() {
        return completed;
    }

    public override bool requiresInRange() {
        return true;
    }

    public override bool checkProceduralPrecondition(GameObject agent) {
        DroneAgent drone = agent.GetComponent<DroneAgent>();
        target = drone.homeBase;
        return target != null;
    }

    public override bool perform(GameObject agent) {
        DroneAgent drone = agent.GetComponent<DroneAgent>();
        completed = true;
        return drone.depositResources(target);
    }
}
