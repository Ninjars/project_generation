using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagGenSimple : MonoBehaviour {

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    void Start () {
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                map[x, y] = 0;
            }
        }
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.generateMesh(map, 1);

        bool usingFixedSeed = !useRandomSeed && seed != null;
        System.Random random;
        if (usingFixedSeed) {
            random = new System.Random(seed.GetHashCode());
        } else {
            random = new System.Random();
        }
        PopulateMap mapPopulator = GetComponent<PopulateMap>();
        mapPopulator.populateMap(random, map);

        GlobalRegister.setWorldMap(map);
	}
}
