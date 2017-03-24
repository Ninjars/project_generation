using UnityEngine;

namespace MapGenGame {
    public interface IHarvester : IDestroyable {
        void setHomeBase(IStockpile resourceDeposit);
        void decommission();
        int getResourcesNeeded();
        IResource getTargetResource();
        Vector3 getPosition();
        GameObject getGameObject();
    }
}
