using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IGOAP {

	Dictionary<string, object> getWorldState();

	Dictionary<string, object> createGoalState();

	void planFailed(Dictionary<string, object> failedGoal);

	void planFound(Dictionary<string, object> goal, Queue<GOAPAction> actions);

    void actionsFinished();

    void planAborted(GOAPAction aborter);

    bool moveAgent(GOAPAction nextAction);
}
