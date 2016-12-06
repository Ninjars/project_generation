using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public int width;
    public int height;


    public string seed;
    public bool useRandomSeed;

    [Range(1, 100)]
    public int randomFillPercent = 45;
    public int smoothingPasses = 5;
    public int wallThreshold = 5;
    public int voidThreshold = 3;

    int[,] map;

    void Start() {
        generateMap();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            generateMap();
        }
    }

    // For debugging representation
    /*
    void OnDrawGizmos() {
        if (map != null) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Gizmos.color = map[x, y] == 1 ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + 0.5f, 0, -height / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
    */

    void generateMap() {
        map = new int[width, height];
        randomFillMap();

        for (int i = 0; i < smoothingPasses; i++) {
            smoothMap();
        }

        int borderSize = 5;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++) {
            for (int y = 0; y < borderedMap.GetLength(1); y++) {
                if (x >= borderSize && x < borderedMap.GetLength(0) - borderSize && y >= borderSize && y < borderedMap.GetLength(1) - borderSize) {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                } else {
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.generateMesh(borderedMap, 1);
    }

    void randomFillMap() {
        if (useRandomSeed) {
            seed = DateTime.Now.Ticks.ToString();
        }

        System.Random random = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                    map[x, y] = 1;
                } else {
                    map[x, y] = random.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }
    }

    void smoothMap() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int neighbouringWalls = getSurroundingWallCount(x, y);

                if (neighbouringWalls >= wallThreshold) {
                    map[x, y] = 1;
                } else if (neighbouringWalls <= voidThreshold) {
                    map[x, y] = 0;
                }
            }
        }
    }

    int getSurroundingWallCount(int gridX, int gridY) {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                // check all tiles within map bounds to see if they are walls
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        // walls are values as 1 and void is 0
                        wallCount += map[neighbourX, neighbourY];
                    }
                    // tiles outside map always count as walls
                } else {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

}

