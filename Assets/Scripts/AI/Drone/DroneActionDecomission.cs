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
        target = drone.homeBase == null ? null : drone.homeBase.getGameObject();
        return target != null  && drone.isDecommissioned();
    }

    public override bool perform(GameObject agent) {
        IStockpile stockpile = target.GetComponent<IStockpile>();
        IHarvester drone = agent.GetComponent<IHarvester>();
        if (drone != null) {
            stockpile.removeHarvester(drone);
            drone.destroy();
        }
        completed = true;
        return true;
    }
}
