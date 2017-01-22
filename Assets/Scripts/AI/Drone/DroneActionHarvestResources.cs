using UnityEngine;

public class DroneActionHarvestResources : GOAPAction, IActionPerformed {
    public float range = 2f;
    private bool completed = false;

	public DroneActionHarvestResources() {
		addEffect(DroneAgent.PLAN_COLLECTED, true);
        cost = 100f;
        setMaxTriggerRange(range);
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
        Debug.Assert(drone != null, "agent doesn't have drone component!");
        target = updateTarget(drone, target);
        return target != null;
    }

    private GameObject updateTarget(IHarvester drone, GameObject currentTarget) {
        IResource res = drone.getTargetResource();
        return res == null || res.Equals(null) ? null : res.getGameObject();
    }

	public override bool perform(GameObject agent) {
        DroneAgent drone = agent.GetComponent<DroneAgent>();
		bool success = drone.harvest(target, this);
        if (!success) {
            drone.clearTargetResource();
        }
//		Debug.Log ("perform() " + success);
        return success;
    }

	public void onActionPerformed() {
		completed = true;
	}
}
