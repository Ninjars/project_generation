using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionScoutNewLocation : GOAPAction {
    public float scanRange = 50f;
    private bool hasScoutedLocation;

    public ActionScoutNewLocation() {
        addEffect(ScoutAgent.PLAN_SCOUT, true);
        cost = 100f;
        setMaxTriggerRange(2f);
    }
    
    public override void reset() {
        hasScoutedLocation = false;
    }

    public override bool isDone() {
        return hasScoutedLocation;
    }

    public override bool requiresInRange() {
        return true;
    }

    public override bool checkProceduralPrecondition(GameObject agent) {
        target = findScoutLocation(target);
        return target != null;
    }

    public override bool perform(GameObject agent) {
        Collider[] foundObjects = Physics.OverlapSphere(gameObject.transform.position, scanRange);
        ScoutAgent scout = agent.GetComponent<ScoutAgent>();
        Debug.Assert(scout != null, "agent doesn't have scout component!");
        HashSet<IResource> foundResources = scout.getFoundResources();
        foreach (Collider hit in foundObjects) {
            Transform parent = hit.transform.parent;
            if (parent == null) {
                continue;
            }
            IResource resource = parent.gameObject.GetComponent<IResource>();
            if (resource != null) {
                foundResources.Add(resource);
            }
        }
        hasScoutedLocation = true;
        return foundResources.Count > scout.minimumResourcesToReport;
    }

    private GameObject findScoutLocation(GameObject target) {
        int[,] map = GlobalRegister.getWorldMap();
        float maxX = map.GetLength(0) / 2;
        float maxY = map.GetLength(1) / 2;
        float x = Mathf.Floor(Random.value * maxX - maxX / 2f);
        float y = Mathf.Floor(Random.value * maxY - maxY / 2f);
        float height = Random.value * 3;
        Vector3 position = new Vector3(x, height, y);
        if (target == null) {
            target = new GameObject("target");
        }
        target.transform.position = position;
        return target;
    }
}
