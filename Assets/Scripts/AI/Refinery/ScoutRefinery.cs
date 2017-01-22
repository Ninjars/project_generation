using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoutRefinery : MonoBehaviour, IScoutBase {

    public int maxRawResources = 100;
    public int currentRawResources = 0;

    public int maxRefinedResources = 20;
    public int currentRefinedResources = 0;

    public int minTransmitCount = 5;
    public int maxScouts = 3;

    public GameObject scoutObj;
    public GameObject stockpileObj;
    private IStockpile stockpile;

    private List<IScout> managedScouts = new List<IScout>();
    private List<IResource> knownResources = new List<IResource>();
    private Vector3 spawnLocation;

    private void Start() {
        spawnLocation = transform.FindChild("SpawnLocation").position;
        stockpile = stockpileObj.GetComponent<IStockpile>();
        if (stockpile == null) {
            throw new Exception("no stockpile component attached to ScoutRefinery");
        }
    }

    void Update() {
        // check if new harvester should be created
        if (canSendAdditionalScout()) {
            createNewScout();
        }
        if (stockpile != null && knownResources.Count >= minTransmitCount) {
            stockpile.reportResources(knownResources);
            knownResources.Clear();
        }
    }

    private void createNewScout() {
        GameObject scoutInstance = Instantiate(scoutObj, spawnLocation, Quaternion.identity);
        IScout scout = scoutInstance.GetComponent<IScout>();
        if (scout == null) {
            Debug.Log("ScoutBase scout object doesn't implement IScout interface " + scoutInstance);
            Destroy(scoutInstance);
        } else {
            scout.setHomeBase(this);
            managedScouts.Add(scout);
        }
    }

    private bool canSendAdditionalScout() {
        return managedScouts.Count < maxScouts;
    }

    public Vector3 getPosition() {
        return gameObject.transform.position;
    }

    public GameObject getGameObject() {
        return gameObject;
    }

    public void onResourceLocationsFound(HashSet<IResource> hashSet) {
        knownResources.AddRange(hashSet);
    }
}
