using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interfaces : MonoBehaviour {

	public interface IWallGenerator {
        // an outline is a list of indexes pointing to a Vector3 vertex in the vertices list
        Mesh generate(List<List<int>> outlines, List<Vector3> vertices);
    }

    public interface IListChangeListener {
        void onObjectAdded(GameObject newObject);
        void onObjectRemoved(GameObject newObject);
    }

    public interface IHarvester {
        void setHomeBase(GameObject resourceDeposit);
        void decommission();
        int getResourcesNeeded();
        GameObject getTargetResource();
    }

    public interface IResourceStockpile {
        int deposit(GameObject depositor, int count); // returns amount unable to deposit
        GameObject getHarvesterTarget(GameObject harvester);
        bool decomissionHarvesterIfNeeded(GameObject harvesterObject);
        void removeHarvester(GameObject harvester);
    }
}
