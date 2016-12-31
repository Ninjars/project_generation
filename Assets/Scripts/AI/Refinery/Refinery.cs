using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refinery : MonoBehaviour, Interfaces.IResourceStockpile {

    public int maxRawResources = 100;
    public int currentRawResources = 0;

    public int maxRefinedResources = 20;
    public int currentRefinedResources = 0;

    public int refineResourceCost = 5;
    public double secondsPerRefine = 5;
    private double refineTimer;

    public GameObject harvesterObj;
    public double resourceDetectionRadius = 100;
    private double squaredDetectionRadius;
    public float targetHarvesterToResourcePointRatio = 0.5f;

    // map of harvesters to assigned resource
    public Dictionary<GameObject, GameObject> managedHarvesters = new Dictionary<GameObject, GameObject>();

	void Start () {
        squaredDetectionRadius = resourceDetectionRadius * resourceDetectionRadius;
	}
	
	void Update () {
        // check if new harvester should be created
        if (!doesHarvesterCountExceedTargetRatio()) {
            createNewHarvester();
        }

        // check to see if refining can be done
        if (currentRawResources < refineResourceCost || currentRefinedResources >= maxRefinedResources) {
            refineTimer = 0;
        } else {
            refineTimer += Time.deltaTime;
            // check if enough time has elapsed to perform a refine action
            if (refineTimer >= secondsPerRefine) {
                currentRawResources -= refineResourceCost;
                currentRefinedResources++;
                refineTimer -= secondsPerRefine;
            }
        }
	}

    private void createNewHarvester() {
        GameObject harvesterInstance = Instantiate(harvesterObj, gameObject.transform.position, Quaternion.identity);
        Interfaces.IHarvester harvester = harvesterInstance.GetComponent<Interfaces.IHarvester>();
        if (harvester == null) {
            Debug.Log("Refinery harvester object doesn't implement IHarvester interface " + harvesterInstance);
            Destroy(harvesterInstance);
        } else {
            harvester.setHomeLocation(gameObject);
            managedHarvesters.Add(harvesterInstance, null);
        }
    }

    /*
     * Attempt to deposit resources.  Will return the number of resources that weren't depositied.
     * If the depositor is a harvester managed by the refinery, it may want to destroy the harvester.
     */
    public int deposit(GameObject depositor, int count) {
        int remainingSpace = maxRawResources - currentRawResources;
        if (remainingSpace >= count) {
            currentRawResources += count;
            checkToDecomissionHarvester(depositor);
            return 0;
        } else {
            count -= remainingSpace;
            currentRawResources = maxRawResources;
            return count;
        }
    }

    public GameObject getHarvesterTarget(GameObject harvester) {
        // sanity check that this harvester is correctly registered here
        if (!managedHarvesters.ContainsKey(harvester)) {
            throw new KeyNotFoundException("getHarvesterTarget() called with unregistered harvester");
        }
        List<GameObject> allTargets = getResourcesInRange();
        GameObject best = managedHarvesters[harvester];
        double bestScore = 0;
        if (best != null) {
            // weight towards current target
            bestScore = getTargetScore(harvester, best) * 1.2;
        }

        foreach (GameObject target in allTargets) {
            double score = getTargetScore(harvester, target);
            if (score > bestScore) {
                best = target;
                bestScore = score;
            }
        }
        managedHarvesters[harvester] = best;
        return best;
    }

    /*
     * Returns a score for the passed target based on distance, resources available and 
     * sustainability of harvesting from it. 
     * returns -1 for cannot harvest, 0 for worst, 1 for best
     */
    private double getTargetScore(GameObject harvesterObject, GameObject target) {
        Resource resource = target.GetComponent<Resource>();
        if (resource == null) {
            // don't select targets that can't be harvested
            Debug.Log("Non-resource object considered for Refinery.getTargetScore() target! " + target);
            return -1;
        }
        Interfaces.IHarvester harvester = harvesterObject.GetComponent<Interfaces.IHarvester>();
        if (harvester == null) {
            // require a harvester to get score
            Debug.Log("Non-harvester object considered for Refinery.getTargetScore()! " + harvesterObject);
            return -1;
        }

        double distanceFactor = 1 / Vector3.Distance(target.transform.position, harvesterObject.transform.position);

        // prefer abundant resources over sufficient resources, and strongly prefer sufficient over insufficient 
        // - 20%
        double quantityFactor;
        int resourcesNeeded = harvester.getResourcesNeeded();
        if (resource.currentValue >= resourcesNeeded * 2) {
            quantityFactor = 1;
        } else if (resource.currentValue >= resourcesNeeded) {
            quantityFactor = 0.85;
        } else {
            quantityFactor = 0.2;
        }

        // prefer sustainably harvestable resource 
        double sustainabilityFactor;
        if (resource.destroyIfDepleted && resource.doesRegen()) {
            // allow margin for restoration when abstaining from depletable resources
            sustainabilityFactor = resource.currentValue >= resourcesNeeded * 1.35 ? 1 : 0;
        } else {
            sustainabilityFactor = 1;
        }

        double congestionFactor;
        if (managedHarvesters.ContainsValue(target)) {
            // strongly prefer uncontested targets
            congestionFactor = 0;
        } else {
            congestionFactor = 1;
        }

        // weight each component differently
        return (1 * sustainabilityFactor) + (0.3 * distanceFactor) + (0.2 * quantityFactor) + (0.5 * congestionFactor);
    }

    private void checkToDecomissionHarvester(GameObject depositor) {
        if (!managedHarvesters.ContainsKey(depositor)) {
            // don't decomission depositors that the refinery doesn't own
            return;
        }
        if (doesHarvesterCountExceedTargetRatio()) {
            Debug.Log("RefineryAgent marking drone for decomission");
            Interfaces.IHarvester harvester = depositor.GetComponent<Interfaces.IHarvester>();
            if (harvester == null) {
                Debug.Log("RefineryAgent is managing a harvester that isn't an IHarvester! Sanity check!!");
                return;
            } else {
                harvester.decomission();
            }
        }
    }

    private bool doesHarvesterCountExceedTargetRatio() {
        return managedHarvesters.Count <= 1 ? false :
            (float) managedHarvesters.Count / (float)getResourceInRangeCount() >= targetHarvesterToResourcePointRatio;
    }

    private int getResourceInRangeCount() {
        int count = 0;
        foreach (GameObject resource in GlobalRegister.resources) {
            double distance = (transform.position - resource.gameObject.transform.position).sqrMagnitude;
            if (distance <= squaredDetectionRadius) {
                count++;
            }
        }
        return count;
    }

    private List<GameObject> getResourcesInRange() {
        List<GameObject> inRange = new List<GameObject>();
        foreach (GameObject resource in GlobalRegister.resources) {
            double distance = (transform.position - resource.gameObject.transform.position).sqrMagnitude;
            if (distance <= squaredDetectionRadius) {
                inRange.Add(resource);
            }
        }
        return inRange;
    }
}
