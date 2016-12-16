using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAgent : MobileAgent {

    public GameObject homeResourceStore;
    public int resourceCapacity = 10;
    public int currentResourceCount;

	void Start () {
        setSpeed(10f);
        health = 1;
    }

    public override HashSet<KeyValuePair<string, object>> createGoalState() {
        HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();
        goal.Add(new KeyValuePair<string, object>("supplyResources", true));
        return goal;
    }

    public override void receiveDamage(int damage) {
        health -= damage;
        // todo: handle death
    }

    public int getCurrentResourceCount() {
        return currentResourceCount;
    }

    internal bool gainResources(GameObject target) {
        // todo: possibly impact on resourse collection target, 
        //       eg cause damage or reduce remaining resource count
        if (currentResourceCount < resourceCapacity) {
            currentResourceCount++;
            return true;
        } else {
            currentResourceCount = resourceCapacity;
            return false;
        }
    }

    internal bool depositResources(GameObject target) {
        // todo: add resources to target
        currentResourceCount = 0;
        return true;
    }

    internal bool isFullOfResources() {
        return currentResourceCount == resourceCapacity;
    }

    /*
     * returns the states that this agent knows and cares about
     * could be supplemented by the base class providing some global values that 
     * agents then add to, to create balance of individual and group goals
     */
    public override HashSet<KeyValuePair<string, object>> getWorldState() {
        HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();
        worldData.Add(new KeyValuePair<string, object>("supplyResources", false)); // the 'false' here could be based on the resource store's values instead
        return worldData;
    }
}
