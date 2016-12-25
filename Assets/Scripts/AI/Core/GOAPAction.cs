using UnityEngine;
using System.Collections.Generic;

public abstract class GOAPAction : MonoBehaviour {

	private Dictionary<string, object> preconditions;
	private Dictionary<string, object> effects;

    private bool inRange = false;
    private float maxTriggerRange = 1f;
    public float cost = 1f;

    public GameObject target;

    public GOAPAction() {
		preconditions = new Dictionary<string, object>();
		effects = new Dictionary<string, object>();
    }

    public void doReset() {
        inRange = false;
        target = null;
        reset();
    }

    public abstract void reset();

    public abstract bool isDone();

    public abstract bool checkProceduralPrecondition(GameObject agent);

    public abstract bool perform(GameObject agent);

    public abstract bool requiresInRange();

    public bool isInRange() {
        return inRange;
    }

    public void setMaxTriggerRange(float range) {
        maxTriggerRange = range;
    }

    public float getMaxTriggerRange() {
        return maxTriggerRange;
    }

    public void setInRange(bool val) {
        inRange = val;
    }

	public void addPrecondition(string key, object value) {
		preconditions.Add(key, value);
    }

    public void removePrecondition(string key) {
		if (preconditions.ContainsKey (key)) {
			preconditions.Remove (key);
		}
    }

	public void addEffect(string key, object value) {
		if (effects.ContainsKey(key)) {
			effects [key] = value;
		} else {
			effects.Add(key, value);
		}
    }

	public void removeEffect(string key) {
		if (effects.ContainsKey (key)) {
			effects.Remove (key);
		}
    }

    public Dictionary<string, object> Preconditions {
        get {
            return preconditions;
        }
    }

    public Dictionary<string, object> Effects {
        get {
            return effects;
        }
    }
}
