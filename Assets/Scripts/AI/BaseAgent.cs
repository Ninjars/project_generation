using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAgent : MonoBehaviour, IGOAP {

    protected int health;

    public abstract void receiveDamage(int damage);

    public abstract HashSet<KeyValuePair<string, object>> createGoalState();

    public void planFailed(HashSet<KeyValuePair<string, object>> failedGoal) {

    }

    public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GOAPAction> action) {

    }

    public void actionsFinished() {

    }

    public void planAborted(GOAPAction aborter) {

    }

    public abstract HashSet<KeyValuePair<string, object>> getWorldState();

    public abstract bool moveAgent(GOAPAction nextAction);
}
