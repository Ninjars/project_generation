using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAgent : MonoBehaviour, IGOAP {

    protected int health;

    public abstract void receiveDamage(int damage);

	public abstract Dictionary<string, object> createGoalState();

	public void planFailed(Dictionary<string, object> failedGoal) {

    }

	public void planFound(Dictionary<string, object> goal, Queue<GOAPAction> action) {

    }

    public void actionsFinished() {

    }

    public void planAborted(GOAPAction aborter) {

    }

	public abstract Dictionary<string, object> getWorldState();

    public abstract bool moveAgent(GOAPAction nextAction);
}
