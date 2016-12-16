using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneActionHarvestResources : GOAPAction {

    private bool completed = false;

    public DroneActionHarvestResources() {
        addEffect("possessResources", true);
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
        target = GameObject.Find("Resource");
        return target != null;
    }

    public override bool perform(GameObject agent) {
        DroneAgent drone = agent.GetComponent<DroneAgent>();
        bool success = drone.gainResources(target);
        completed = drone.isFullOfResources();
        return success;
    }
}
