using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenGame {
    public class SpawnerFlower : MonoBehaviour {

        public GameObject spawnObject;
        public int secondsToRegenerate;
        public bool onlySpawnOne = true;
        private bool isSpawning = false;
        private Vector3 spawnLocation;
        private GameObject lastSpawnedInstance;

    	void Start () {
            spawnLocation = transform.FindChild("SpawnLocation").position;
            beginSpawning();
    	}

        private void beginSpawning() {
            isSpawning = true;
            StartCoroutine(generate());
        }

        internal IEnumerator generate() {
            Debug.Log("SpawnerFlower:generate()");
            yield return new WaitForSeconds(secondsToRegenerate);
            Debug.Log("SpawnerFlower:generate: continuing");
            if (onlySpawnOne && lastSpawnedInstance != null) {
                Debug.Log("SpawnerFlower:generate: breaking early");
                yield break;
            }
            lastSpawnedInstance = Instantiate(spawnObject, spawnLocation, Random.rotation);
            onGenerated(lastSpawnedInstance);
            isSpawning = false;
        }

        public virtual void onGenerated(GameObject generatedObject) {
            Debug.Log("SpawnerFlower:onGenerated()");
            // to be overridden as needed
        }

    	void Update () {
            bool canSpawn = lastSpawnedInstance == null || !onlySpawnOne;
            if (canSpawn && !isSpawning) {
                beginSpawning();
            }
    	}
    }
}
