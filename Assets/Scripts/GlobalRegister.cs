using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalRegister {

    private static List<GameObject> resources = new List<GameObject>();
    private static List<Interfaces.IListChangeListener> resourceChangeListeners = new List<Interfaces.IListChangeListener>();

    public static List<GameObject> getResources() {
        return new List<GameObject>(resources);
    }

    public static void addResource(GameObject gameObject) {
        Resource resource = gameObject.GetComponent<Resource>();
        if (resource == null) {
            throw new MissingComponentException("GlobalRegister.addResource() passed an object without a Resource component");
        }
        resources.Add(gameObject);
    }

    public static void addResources(List<GameObject> gameObjects) {
        foreach (GameObject gameObject in gameObjects) {
            addResource(gameObject);
            notifyResourceListeners(gameObject, true);
        }
    }

    public static void removeResource(GameObject gameObject) {
        notifyResourceListeners(gameObject, false);
        resources.Remove(gameObject);
    }

    public static void clearResources() {
        if (resources != null) {
            foreach (GameObject obj in resources) {
                notifyResourceListeners(obj, false);
                Object.Destroy(obj);
            }
            resources.Clear();
        }
    }

    public static void registerResourceChangeListener(Interfaces.IListChangeListener listener) {
        resourceChangeListeners.Add(listener);
    }

    public static void unregisterResourceChangeListener(Interfaces.IListChangeListener listener) {
        resourceChangeListeners.Remove(listener);
    }

    private static void notifyResourceListeners(GameObject changingObject, bool isAdded) {
        foreach (Interfaces.IListChangeListener listener in resourceChangeListeners) {
            if (isAdded) {
                listener.onObjectAdded(changingObject);
            } else {
                listener.onObjectRemoved(changingObject);
            }
        }
    }
}
