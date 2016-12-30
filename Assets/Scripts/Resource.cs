using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour {

    public int maxValue = 15;
    public int currentValue = 5;
    public int amountGainedPerRegen = 1;
    public float secondsPerRegen = 8;
    public bool destroyIfDepleted = false;

    private float timeSinceLastRegen = 0;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
    void Update () {
        if (destroyIfDepleted && currentValue == 0) {
            GlobalRegister.resources.Remove(gameObject);
            Destroy(gameObject);
            return;
        }
        if (currentValue >= maxValue) {
            // no need to continue attempting to regen if maxed out
            timeSinceLastRegen = 0;
            return;
        }
        timeSinceLastRegen += Time.deltaTime;
        if (timeSinceLastRegen >= secondsPerRegen) {
            while (timeSinceLastRegen >= secondsPerRegen) {
                bool ableToRegen = performRegen();
                timeSinceLastRegen -= secondsPerRegen;
                if (!ableToRegen) {
                    // can fail to regen when at max capacity;
                    break;
                }
            }
        }
	}

    private bool performRegen() {
        if (currentValue >= maxValue) {
            return false;
        } else {
            currentValue = Mathf.Min(maxValue, currentValue + amountGainedPerRegen);
            return true;
        }
    }

    /*
     * returns number of resources provided, max desiredAmount, min zero
     */
    public int harvest(int desiredAmount) {
        int returnVal;
        if (currentValue >= desiredAmount) {
            currentValue -= desiredAmount;
            returnVal = desiredAmount;
        } else {
            returnVal = currentValue;
            currentValue = 0;
        }
        return returnVal;
    }

    public bool doesRegen() {
        return amountGainedPerRegen > 0 && secondsPerRegen > 0;
    }
}
