using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenGame {
    public class ActionReportResources : GOAPAction {

        private bool completed;

        public ActionReportResources() {
            addEffect(ScoutAgent.PLAN_REPORT, true);
            addPrecondition(ScoutAgent.PLAN_SCOUT, true);
            cost = 100f;
            setMaxTriggerRange(2f);
        }

        public override void reset() {
            completed = false;
            target = null;
        }

        public override bool isDone() {
            return completed;
        }

        public override bool requiresInRange() {
            return true;
        }

        public override bool checkProceduralPrecondition(GameObject agent) {
            ScoutAgent scout = agent.GetComponent<ScoutAgent>();
            Debug.Assert(scout != null, "Scout doesn't have ScoutAgent component!");
            target = scout.homeBase.getGameObject();
            return true;
        }

        public override bool perform(GameObject agent) {
            ScoutAgent scout = agent.GetComponent<ScoutAgent>();
            Debug.Assert(scout != null);
            scout.reportResources();
            completed = true;
            return true;
        }
    }
}
