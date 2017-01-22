using System.Collections.Generic;
using UnityEngine;

public interface IStockpile {
    int deposit(IHarvester depositor, int count); // returns amount unable to deposit
    IResource getHarvesterTarget(IHarvester harvester);
    bool decomissionHarvesterIfNeeded(IHarvester harvester);
    void removeHarvester(IHarvester harvester);
    Vector3 getPosition();
    GameObject getGameObject();
    void reportResources(List<IResource> resources);
}
