using UnityEngine;

public interface IResource : IDestroyable {
    GameObject getGameObject();
    Vector3 getPosition();
    int getCurrentValue();
    bool willDestroyIfDepleted();
    bool doesRegenerate();
}
