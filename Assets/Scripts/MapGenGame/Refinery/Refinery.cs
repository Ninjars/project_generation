﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenGame {
    public class Refinery : MonoBehaviour, IStockpile, IListChangeListener<IResource> {

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

        public List<IHarvester> managedHarvesters = new List<IHarvester>();
        private HashSet<IResource> knownResources = new HashSet<IResource>();

    	void Start () {
            squaredDetectionRadius = resourceDetectionRadius * resourceDetectionRadius;
            //knownResources = getResourcesInRange();
            Debug.Assert(knownResources != null);
            //GlobalRegister.registerResourceChangeListener((IListChangeListener<IResource>)this);
        }

        public void onListItemAdded(IResource resource) {
            if (isInRange(resource.getPosition())) {
                knownResources.Add(resource);
            }
        }

        public void onListItemRemoved(IResource resource) {
            knownResources.Remove(resource);
        }

        /**
         * Called on start to populate inital resources list, which is then maintained by listening for changes
         */
        private List<IResource> getResourcesInRange() {
            List<IResource> allResourcesList = GlobalRegister.getResources();
            List<IResource> resourcesList = new List<IResource>();
            foreach (IResource resource in allResourcesList) {
                if (resource != null && isInRange(resource.getPosition())) {
                    resourcesList.Add(resource);
                }
            }
            return resourcesList;
        }

        private bool isInRange(Vector3 position) {
            double distance = (transform.position - position).sqrMagnitude;
            if (distance > squaredDetectionRadius) {
                return false;
            } else {
                return true;
            }
        }
    	
    	void Update () {
            // check if new harvester should be created
            if (requireAdditionalHarvesters()) {
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
            IHarvester harvester = harvesterInstance.GetComponent<IHarvester>();
            if (harvester == null) {
                Debug.Log("Refinery harvester object doesn't implement IHarvester interface " + harvesterInstance);
                Destroy(harvesterInstance);
            } else {
                harvester.setHomeBase(this);
                managedHarvesters.Add(harvester);
            }
        }

        /*
         * Attempt to deposit resources.  Will return the number of resources that weren't depositied.
         * If the depositor is a harvester managed by the refinery, it may want to destroy the harvester.
         */
        public int deposit(IHarvester harvester, int count) {
            int remainingSpace = maxRawResources - currentRawResources;
            if (remainingSpace >= count) {
                currentRawResources += count;
                decomissionHarvesterIfNeeded(harvester);
                return 0;
            } else {
                count -= remainingSpace;
                currentRawResources = maxRawResources;
                return count;
            }
        }

        public void removeHarvester(IHarvester harvester) {
            managedHarvesters.Remove(harvester);
        }

        public IResource getHarvesterTarget(IHarvester harvester) {
            // sanity check that this harvester is correctly registered here
            if (!managedHarvesters.Contains(harvester)) {
                throw new KeyNotFoundException("getHarvesterTarget() called with unregistered harvester");
            }
            knownResources.RemoveWhere(entry => entry == null || entry.Equals(null));
            List<IResource> resourceTargets = new List<IResource>(knownResources);
            IResource best = harvester.getTargetResource();
            double bestScore = 0;
            if (best != null) {
                // weight towards current target
                bestScore = getTargetScore(harvester, best) * 1.2;
                resourceTargets.Remove(best);
            }
            
            foreach (IResource target in resourceTargets) {
                if (target == null || target.Equals(null)) {
                    Debug.Log("Refinery ignoring null target resource");
                    continue;
                }
                double score = getTargetScore(harvester, target);
                if (score > bestScore) {
                    best = target;
                    bestScore = score;
                }
            }
            return best;
        }

        /*
         * Returns a score for the passed target based on distance, resources available and 
         * sustainability of harvesting from it. 
         * returns -1 for cannot harvest, 0 for worst, 1 for best
         */
        private double getTargetScore(IHarvester queryingHarvester, IResource resource) {
            if (queryingHarvester == null || resource == null) {
                return -1;
            }
            double distanceFactor = 1 / Vector3.Distance(resource.getPosition(), queryingHarvester.getPosition());

            // prefer abundant resources over sufficient resources, and strongly prefer sufficient over insufficient 
            // - 20%
            double quantityFactor;
            int resourcesNeeded = queryingHarvester.getResourcesNeeded();
            if (resource.getCurrentValue() >= resourcesNeeded * 2) {
                quantityFactor = 1;
            } else if (resource.getCurrentValue() >= resourcesNeeded) {
                quantityFactor = 0.85;
            } else {
                quantityFactor = 0.2;
            }

            // prefer sustainably harvestable resource 
            double sustainabilityFactor;
            if (resource.willDestroyIfDepleted() && resource.doesRegenerate()) {
                // allow margin for restoration when abstaining from depletable resources
                sustainabilityFactor = resource.getCurrentValue() >= resourcesNeeded * 1.35 ? 1 : 0;
            } else {
                sustainabilityFactor = 1;
            }

            double congestionFactor = 1;
            // strongly prefer uncontested targets
            foreach (IHarvester managedHarvester in managedHarvesters) {
                if (managedHarvester != null && !managedHarvester.Equals(queryingHarvester)) {
                    IResource otherResource = managedHarvester.getTargetResource();
                    if (otherResource != null && otherResource.Equals(resource)) {
                        congestionFactor *= 0.5;
                    }
                }
            }

            // weight each component differently
            return (1 * sustainabilityFactor) + (0.3 * distanceFactor) + (0.2 * quantityFactor) + (0.5 * congestionFactor);
        }

        public bool decomissionHarvesterIfNeeded(IHarvester harvester) {
            if (!managedHarvesters.Contains(harvester)) {
                // don't decomission depositors that the refinery doesn't own
                return false;
            }
            if (doesHarvesterCountExceedTargetRatio()) {
                if (harvester == null) {
                    throw new UnityException("Sanity check: Refinery.decomissionHarvesterIfNeeded() called on a non-harvester object, which is unexpected.");
                } else {
                    harvester.decommission();
                    return true;
                }
            }
            return false;
        }

        private bool doesHarvesterCountExceedTargetRatio() {
            return managedHarvesters.Count == 0 ? false :
                (float) managedHarvesters.Count / (float)getResourceInRangeCount() > targetHarvesterToResourcePointRatio;
        }

        private bool requireAdditionalHarvesters() {
            int resourceCount = getResourceInRangeCount();
            return managedHarvesters.Count == 0 && resourceCount > 0 ? true :
                (float) managedHarvesters.Count / (float)resourceCount < targetHarvesterToResourcePointRatio;
        }

        private int getResourceInRangeCount() {
            return knownResources.Count;
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }

        public GameObject getGameObject() {
            return gameObject;
        }

        public void reportResources(List<IResource> resources) {
            knownResources.UnionWith(resources);
        }
    }
}
