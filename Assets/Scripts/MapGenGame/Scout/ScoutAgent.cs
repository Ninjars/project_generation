using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenGame {
    public class ScoutAgent : MobileAgent, IScout {

        internal static string PLAN_SCOUT = "scout";
        internal static string PLAN_REPORT = "report";

        public IScoutBase homeBase;
        public int minimumResourcesToReport = 3;
        private HashSet<IResource> foundResources = new HashSet<IResource>();

        // Use this for initialization
        void Start () {
            base.start(10f);
            health = 1;
    	}
    	
    	// Update is called once per frame
    	void Update () {
    		
    	}

        public void setHomeBase(IScoutBase scoutBase) {
            homeBase = scoutBase;
        }

        //void OnDrawGizmos() {
        //    ActionScoutNewLocation locationAction = GetComponent<ActionScoutNewLocation>();
        //    GameObject pos = null;
        //    if (locationAction != null) {
        //        pos = locationAction.target;
        //    }
        //    if (pos != null) {
        //        Gizmos.color = Color.green;
        //        Gizmos.DrawLine(gameObject.transform.position, pos.transform.position);
        //    }
        //}

        public override Dictionary<string, object> createGoalState() {
            Dictionary<string, object> goal = new Dictionary<string, object>();
            goal[PLAN_REPORT] = true;
            return goal;
        }

        public override void receiveDamage(int damage) {
            health -= damage;
            if (health <= 0) {
                destroy();
            }
        }

        public override Dictionary<string, object> getWorldState() {
            Dictionary<string, object> worldData = new Dictionary<string, object>();
            worldData[PLAN_REPORT] = false;
            worldData[PLAN_SCOUT] = false;
            return worldData;
        }

        public void destroy() {
            Destroy(transform.root.gameObject);
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }

        public GameObject getGameObject() {
            return gameObject;
        }

        public HashSet<IResource> getFoundResources() {
            return foundResources;
        }

        public void reportResources() {
            homeBase.onResourceLocationsFound(foundResources);
            foundResources.Clear();
        }
    }
}
