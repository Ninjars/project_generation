using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAgent : MobileAgent, Interfaces.IHarvester {

    public GameObject homeLocation;
    public int resourceCapacity = 10;
    public int currentResourceCount;
	public float harvestTime = 1f;

	internal static string PLAN_DELIVER = "supplyResources";
    internal static string PLAN_COLLECTED = "collectedResources";

	private enum HarvestState { NONE, HARVESTING };
	private HarvestState harvestState = HarvestState.NONE;

    private bool decomissioned = false;

	void Start () {
        setSpeed(10f);
        health = 1;
    }

	public override Dictionary<string, object> createGoalState() {
		Dictionary<string, object> goal = new Dictionary<string, object>();
        goal[PLAN_DELIVER] = true;
        return goal;
    }

    public override void receiveDamage(int damage) {
        health -= damage;
        // todo: handle death
    }

    public void setHomeLocation(GameObject location) {
        Interfaces.IResourceStockpile stockpile = location.GetComponent<Interfaces.IResourceStockpile>();
        if (stockpile == null) {
            throw new ArgumentException("DroneAgent sanity check: setting location as a non-stockpile object; this is unsupported");
        }
        homeLocation = location;
    }

    public void decomission() {
        // todo: add decomission action
        this.decomissioned = true;
    }

    public int getCurrentResourceCount() {
        return currentResourceCount;
    }

    internal bool harvest(GameObject target, IActionPerformed callback) {
        if (target == null) {
            Debug.Log ("harvest() : null target");
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
            Debug.Log("performHarvest: DroneAgent harvest target doesn't have a Resource script component! This case is unhandled!");
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
        return worldData;
    }

    public GameObject getResourceTargetLocation() {
        Interfaces.IResourceStockpile stockpile = homeLocation == null ? null : homeLocation.GetComponent<Interfaces.IResourceStockpile>();
        if (stockpile == null) {
            throw new ArgumentException("DroneAgent sanity check: depositResources targetting a non-stockpile object; this is unsupported");
        }
        return stockpile.getHarvesterTarget(gameObject);
    }

    public int getResourcesNeeded() {
        return resourceCapacity - currentResourceCount;
    }
}
