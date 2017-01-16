using UnityEngine;

public interface IScout : IDestroyable {
    void setHomeBase(IScoutBase scoutBase);
    Vector3 getPosition();
    GameObject getGameObject();
}
