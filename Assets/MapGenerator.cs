﻿using System.Collections;
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

    class Room {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;

        public Room(List<Coord> roomTiles, int[,] map) {
            tiles = roomTiles;
            roomSize = tiles.Count;
            edgeTiles = new List<Coord>();
            connectedRooms = new List<Room>();

            foreach (Coord tile in tiles) {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
                        if (x == tile.tileX || y == tile.tileY) {
                            if (map[x,y] == 1) {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public Room() {
            tiles = new List<Coord>();
            edgeTiles = new List<Coord>();
            connectedRooms = new List<Room>();
            roomSize = 0;
        }

        public static void connectRooms(Room a, Room b) {
            a.connectedRooms.Add(b);
            b.connectedRooms.Add(a);
        }

        public bool isConnected(Room other) {
            return connectedRooms.Contains(other);
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
        List<Room> validRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions) {
            Debug.Log(roomRegion.Count);
            if (roomRegion.Count < roomThresholdSize) {
                foreach (Coord tile in roomRegion) {
                    map[tile.tileX, tile.tileY] = 1;
                }
            } else {
                validRooms.Add(new Room(roomRegion, map));
            }
        }

        connectClosestRooms(validRooms);
    }

    void connectClosestRooms(List<Room> rooms) {
        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();

        foreach (Room a in rooms) {
            bool possibleConnectionFound = false;
            foreach (Room b in rooms) {
                if (a == b) {
                    // don't try to connect a room to itself
                    continue;
                }
                if (a.isConnected(b)) {
                    // don't continue to try connecting if connection is found and don't create passage
                    possibleConnectionFound = false;
                    break;
                    //continue; // if we continue, each room will check for other connections that may be shorter than what they already have
                }
                for (int tileIndexA = 0; tileIndexA < a.edgeTiles.Count; tileIndexA++) {
                    for (int tileIndexB = 0; tileIndexB < b.edgeTiles.Count; tileIndexB++) {
                        Coord tileA = a.edgeTiles[tileIndexA];
                        Coord tileB = b.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (!possibleConnectionFound || distanceBetweenRooms < bestDistance) {
                            possibleConnectionFound = true;
                            bestDistance = distanceBetweenRooms;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = a;
                            bestRoomB = b;
                        }
                    }
                }
            }

            if (possibleConnectionFound) {
                createPassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

    }

    void createPassage(Room roomA, Room roomB, Coord tileA, Coord tileB) {
        Room.connectRooms(roomA, roomB);
        Debug.DrawLine(coordToWorldPoint(tileA), coordToWorldPoint(tileB), Color.green, 5);
    }

    Vector3 coordToWorldPoint(Coord tile) {
        return new Vector3(-width / 2 + 0.5f + tile.tileX, 0.5f, -height / 2 + 0.5f + tile.tileY);
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

