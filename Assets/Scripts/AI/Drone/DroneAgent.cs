using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAgent : MobileAgent, Interfaces.IHarvester {

    public GameObject homeBase;
    public GameObject targetResource;
    public int resourceCapacity = 10;
    public int currentResourceCount;
	public float harvestTime = 1f;

	internal static string PLAN_DELIVER = "supplyResources";
    internal static string PLAN_COLLECTED = "collectedResources";
    internal static string PLAN_DECOMMISSION = "decommission";

	private enum HarvestState { NONE, HARVESTING };
	private HarvestState harvestState = HarvestState.NONE;

    private bool decommissioned = false;

	void Start () {
        setSpeed(10f);
        health = 1;
    }

    void Update() {
        if (!decommissioned && homeBase != null && targetResource == null) {
            // if we're not decommissioned but we have a homeBase and no target, we should look for a target
            Interfaces.IResourceStockpile stockpile = homeBase.GetComponent<Interfaces.IResourceStockpile>();
            if (stockpile == null) {
                throw new ArgumentException("DroneAgent sanity check: home base is a non-stockpile object; this is unsupported");
            }
            // check that we shouldn't actually be decommissioned
            decommissioned = stockpile.decomissionHarvesterIfNeeded(gameObject);
            if (!decommissioned) {
                // go harvesting!
                targetResource = stockpile.getHarvesterTarget(gameObject);
            }
        }
    }

    void OnDrawGizmos() {
        if (targetResource != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(gameObject.transform.position, targetResource.transform.position);
        }
        if (homeBase != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(gameObject.transform.position, homeBase.transform.position);
        }
    }

	public override Dictionary<string, object> createGoalState() {
		Dictionary<string, object> goal = new Dictionary<string, object>();
        goal[PLAN_DELIVER] = true;
        goal[PLAN_DECOMMISSION] = true;
        return goal;
    }

    public override void receiveDamage(int damage) {
        health -= damage;
        // todo: handle death
    }

    public void setHomeBase(GameObject targetObject) {
        Interfaces.IResourceStockpile stockpile = targetObject.GetComponent<Interfaces.IResourceStockpile>();
        if (stockpile == null) {
            throw new ArgumentException("DroneAgent sanity check: setting location as a non-stockpile object; this is unsupported");
        }
        homeBase = targetObject;
    }

    public void clearTargetResource() {
        targetResource = null;
    }

    private void setTargetResource(GameObject targetObject) {
        if (targetObject == null) {
            targetResource = null;
            return;
        }
        Resource resource = targetObject.GetComponent<Resource>();
        if (resource == null) {
            throw new ArgumentException("DroneAgent.setResourceTarget() passed object that isn't a resource " + targetObject);
        }
        targetResource = targetObject;
    }

    public void decommission() {
        this.decommissioned = true;
    }

    public int getCurrentResourceCount() {
        return currentResourceCount;
    }

    internal bool harvest(GameObject target, IActionPerformed callback) {
        if (target == null) {
            return false;
        }
		if (isFullOfResources ()) {
			return false;
        }
        switch (harvestState) {
			case HarvestState.NONE:
				harvestState = HarvestState.HARVESTING;
				StartCoroutine (performHarvest (target));
				return true;
			case HarvestState.HARVESTING:
				return true;
			default:
				return false;
        }
	}

    internal IEnumerator performHarvest(GameObject target) {
		yield return new WaitForSeconds(harvestTime);
		// todo: possibly impact on resourse collection target, 
		//       eg cause damage or reduce remaining resource count
        if (target == null) {
            // target may have been destroyed
            harvestState = HarvestState.NONE;
            yield break;
        }
        Resource resource = target.GetComponent<Resource>();
        if (resource == null) {
            throw new ArgumentException("performHarvest: DroneAgent harvest target doesn't have a Resource script component! This case is unhandled!");
        } else {
            int harvestedAmount = resource.harvest(1);
            if (harvestedAmount == 0) {
                // todo: do we need to handle this here, or in the action itself?
            } else {
                currentResourceCount += harvestedAmount;
            }
        }
		harvestState = HarvestState.NONE;
	}

    internal bool depositResources(GameObject target) {
        Interfaces.IResourceStockpile stockpile = target.GetComponent<Interfaces.IResourceStockpile>();
        if (stockpile == null) {
            throw new ArgumentException("DroneAgent sanity check: depositResources targetting a non-stockpile object; this is unsupported");
        }
        currentResourceCount = stockpile.deposit(gameObject, currentResourceCount);
        return currentResourceCount == 0;
    }

    internal bool isFullOfResources() {
        return currentResourceCount >= resourceCapacity;
    }

    /*
     * returns the states that this agent knows and cares about
     * could be supplemented by the base class providing some global values that 
     * agents then add to, to create balance of individual and group goals
     */
	public override Dictionary<string, object> getWorldState() {
		Dictionary<string, object> worldData = new Dictionary<string, object>();
        worldData[PLAN_DELIVER] = false;
        worldData[PLAN_COLLECTED] = isFullOfResources();
		worldData[PLAN_DECOMMISSION] = false;
        return worldData;
    }

    public GameObject getTargetResource() {
        return targetResource;
    }

    public int getResourcesNeeded() {
        return resourceCapacity - currentResourceCount;
    }

    public bool isDecommissioned() {
        return decommissioned;
    }
}
