using UnityEngine;

public class Resource : MonoBehaviour, IResource {
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
            GlobalRegister.removeResource(this);
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
            timeSinceLastRegen -= secondsPerRegen;
            performRegen();
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

    public bool doesRegenerate() {
        return amountGainedPerRegen > 0 && secondsPerRegen > 0;
    }

    public int getCurrentValue() {
        return currentValue;
    }

    public bool willDestroyIfDepleted() {
        return destroyIfDepleted;
    }

    public Vector3 getPosition() {
        return gameObject.transform.position;
    }

    public GameObject getGameObject() {
        return gameObject;
    }

    public void destroy() {
        Destroy(transform.root.gameObject);
    }
}
