using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneActionDecomission : GOAPAction {

    private bool completed = false;

    public DroneActionDecomission() {
        addEffect(DroneAgent.PLAN_DECOMMISSION, true);
        cost = 50f;
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
        target = drone.homeLocation;
        return target != null  && drone.isDecommissioned();
    }

    public override bool perform(GameObject agent) {
        Interfaces.IResourceStockpile stockpile = target.GetComponent<Interfaces.IResourceStockpile>();
        stockpile.removeHarvester(agent);
        completed = true;
        Destroy(gameObject);
        return true;
    }
}
