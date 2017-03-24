using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenGame {
public class ResourceFlower : SpawnerFlower {
        public override void onGenerated(GameObject newObject) {
            Debug.Log("SpawnerFlower:onGenerated()");
            IResource resource = newObject.GetComponent<IResource>();
            Debug.Assert(resource != null);
            GlobalRegister.addResource(resource);
            Debug.Log("SpawnerFlower:generate: onGenerated /end");
        }
    }
}
