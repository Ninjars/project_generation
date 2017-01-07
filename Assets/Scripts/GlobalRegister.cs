using System.Collections.Generic;
using UnityEngine;

public class GlobalRegister {

    private static List<IResource> resources = new List<IResource>();
    private static List<IListChangeListener<IResource>> resourceChangeListeners = new List<IListChangeListener<IResource>>();

    public static List<IResource> getResources() {
        return new List<IResource>(resources);
    }

    public static void addResource(IResource resource) {
        resources.Add(resource);
    }

    public static void addResources(List<IResource> gameObjects) {
        foreach (IResource gameObject in gameObjects) {
            addResource(gameObject);
            notifyResourceListeners(gameObject, true);
        }
    }

    public static void removeResource(IResource gameObject) {
        notifyResourceListeners(gameObject, false);
        resources.Remove(gameObject);
    }

    public static void clearResources() {
        if (resources != null) {
            foreach (IResource obj in resources) {
                notifyResourceListeners(obj, false);
                obj.destroy();
            }
            resources.Clear();
        }
    }

    public static void registerResourceChangeListener(IListChangeListener<IResource> listener) {
        resourceChangeListeners.Add(listener);
    }

    public static void unregisterResourceChangeListener(IListChangeListener<IResource> listener) {
        resourceChangeListeners.Remove(listener);
    }

    private static void notifyResourceListeners(IResource changingObject, bool isAdded) {
        foreach (IListChangeListener<IResource> listener in resourceChangeListeners) {
            if (isAdded) {
                listener.onListItemAdded(changingObject);
            } else {
                listener.onListItemRemoved(changingObject);
            }
        }
    }
}
