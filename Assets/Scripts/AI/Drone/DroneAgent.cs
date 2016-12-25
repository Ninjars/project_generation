﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAgent : MobileAgent {

    public GameObject homeResourceStore;
    public int resourceCapacity = 10;
    public int currentResourceCount;
	public float harvestTime = 1f;

	internal static string PLAN_DELIVER = "supplyResources";
	internal static string PLAN_COLLECTED = "collectedResources";

	private enum HarvestState { NONE, HARVESTING };

	private HarvestState harvestState = HarvestState.NONE;

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

    public int getCurrentResourceCount() {
        return currentResourceCount;
    }

	internal bool harvest(GameObject target, IActionPerformed callback) {
		if (isFullOfResources ()) {
			Debug.Log ("harvest() : full");
			return false;
		}
		switch (harvestState) {
			case HarvestState.NONE:
				harvestState = HarvestState.HARVESTING;
				StartCoroutine (performHarvest ());
				return true;
			case HarvestState.HARVESTING:
				return true;
			default:
				return false;
        }
	}

	internal IEnumerator performHarvest() {
		yield return new WaitForSeconds(harvestTime);
		// todo: possibly impact on resourse collection target, 
		//       eg cause damage or reduce remaining resource count
		currentResourceCount++;
		harvestState = HarvestState.NONE;
	}

    internal bool depositResources(GameObject target) {
        // todo: add resources to target
        currentResourceCount = 0;
        return true;
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
		worldData[PLAN_DELIVER] = false; // the 'false' here could be based on the resource store's values instead
		worldData[PLAN_COLLECTED] = isFullOfResources();
        return worldData;
    }
}
