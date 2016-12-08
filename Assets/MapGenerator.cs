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

    // remove wall regions smaller than this
    public int wallThresholdSize = 50;

    // remove room regions smaller than this
    public int roomThresholdSize = 25;

    int[,] map;

    void Start() {
        generateMap();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            generateMap();
        }
    }

    struct Coord {
        public int tileX;
        public int tileY;

        public Coord(int x, int y) {
            tileX = x;
            tileY = y;
        }
    }

    void generateMap() {
        map = new int[width, height];
        randomFillMap();

        for (int i = 0; i < smoothingPasses; i++) {
            smoothMap();
        }

        processMap();

        /*
        // surround map with walls.  May have unexpected effects with minimum-sized wall culling.
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
        */

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.generateMesh(map, 1);
    }

    void randomFillMap() {
        if (!useRandomSeed && seed != null) {
            Random.InitState(seed.GetHashCode());
        }

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                    map[x, y] = 1;
                } else {
                    map[x, y] = Random.value < randomFillPercent / 100f ? 1 : 0;
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
                if (isInMapRange(neighbourX, neighbourY)) {
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

    void processMap() {
        List<List<Coord>> wallRegions = getRegions(1);
        foreach (List<Coord> wallRegion in wallRegions) {
            Debug.Log(wallRegion.Count);
            if (wallRegion.Count < wallThresholdSize) {
                foreach (Coord tile in wallRegion) {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }
        
        List<List<Coord>> roomRegions = getRegions(0);
        foreach (List<Coord> roomRegion in roomRegions) {
            Debug.Log(roomRegion.Count);
            if (roomRegion.Count < roomThresholdSize) {
                foreach (Coord tile in roomRegion) {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
        }
    }

    List<List<Coord>> getRegions(int tileType) {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // find unchecked tile to begin searching for tile from
                if (mapFlags[x, y] == 0 && map[x, y] == tileType) {
                    List<Coord> newRegion = getRegionTiles(x, y);
                    regions.Add(newRegion);
                    // mark each tile in the region as having been looked at
                    foreach(Coord tile in newRegion) {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> getRegionTiles(int startX, int startY) {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0) {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
                    // check inside map bounds, ignoring diagonal tiles
                    if (isInMapRange(x, y) && (y == tile.tileY || x == tile.tileX)) {
                        // check that this tile hasn't been checked before and that it is of the matching type
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType) {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool isInMapRange(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

}

