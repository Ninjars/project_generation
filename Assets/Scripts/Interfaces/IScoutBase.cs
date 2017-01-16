using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScoutBase {

    GameObject getGameObject();
    Vector3 getPosition();
    void onResourceLocationsFound(HashSet<IResource> hashSet);
}
