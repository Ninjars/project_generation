using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenCentreBias : MonoBehaviour {
    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0f, 1f)]
    public float centreFillPercent = 0.8f;
    [Range(0f, 1f)]
    public float edgeFillPercent = 0.2f;

    public int numberOfSimulationSteps = 12;
    public int deathLimit = 3;
    public int birthLimit = 5;

    // remove wall regions smaller than this
    public int wallThresholdSize = 50;
    // remove room regions smaller than this
    public int roomThresholdSize = 25;
    public bool connectRooms = false;
    public int passageRadius = 1;

    struct Coord {
        public int tileX;
        public int tileY;

        public Coord(int x, int y) {
            tileX = x;
            tileY = y;
        }
    }

    class Room : System.IComparable<Room> {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessible;
        public bool isMainRoom;

        public Room(List<Coord> roomTiles, int[,] map) {
            tiles = roomTiles;
            roomSize = tiles.Count;
            edgeTiles = new List<Coord>();
            connectedRooms = new List<Room>();

            foreach (Coord tile in tiles) {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
                        if (x == tile.tileX || y == tile.tileY) {
                            if (map[x, y] == 1) {
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
            if (a.isAccessible) {
                b.setAccessibleFromMainRoom();
            } else if (b.isAccessible) {
                a.setAccessibleFromMainRoom();
            }
            a.connectedRooms.Add(b);
            b.connectedRooms.Add(a);
        }

        public bool isConnected(Room other) {
            return connectedRooms.Contains(other);
        }

        public void setAccessibleFromMainRoom() {
            if (!isAccessible) {
                isAccessible = true;
                foreach (Room connectedRoom in connectedRooms) {
                    connectedRoom.setAccessibleFromMainRoom();
                }
            }
        }

        public int CompareTo(Room other) {
            return other.roomSize.CompareTo(roomSize);
        }
    }

    void Start() {
        generateMap();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            generateMap();
        }
    }

    void generateMap() {
        int[,] map = randomFillMap();
        for (int i = 0; i < numberOfSimulationSteps; i++) {
            map = stepSimulation(map);
        }
        map = processRooms(map);

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.generateMesh(map, 1);
    }

    int[,] randomFillMap() {
        bool usingFixedSeed = !useRandomSeed && seed != null;
        System.Random random;
        if (usingFixedSeed) {
            random = new System.Random(seed.GetHashCode());
        } else {
            random = new System.Random();
        }

        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                    map[x, y] = 1;
                } else {
                    float halfWidth = width / 2f;
                    float xOffsetPercent = Mathf.Abs(halfWidth - x) / halfWidth;
                    float halfHeight = height / 2f;
                    float yOffsetPercent = Mathf.Abs(halfHeight - y) / halfHeight;
                    float radialOffset = Mathf.Sqrt(xOffsetPercent * xOffsetPercent + yOffsetPercent * yOffsetPercent);
                    float fillPercent = centreFillPercent + (edgeFillPercent - centreFillPercent) * radialOffset;
                    map[x, y] = random.NextDouble() < fillPercent ? 0 : 1;
                }
            }
        }
        return map;
    }

    int[,] stepSimulation(int[,] oldMap) {
        int[,] newMap = new int[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int occupiedNeighbours = getSurroundingWallCount(oldMap, x, y);

                // if occupied, check there are enough neighbours to sustain it
                if (oldMap[x, y] == 1) {
                    if (occupiedNeighbours < deathLimit) {
                        newMap[x, y] = 0;
                    } else {
                        newMap[x, y] = 1;
                    }
                    // if unoccupied, check if there are enough neighbours to occupy
                } else {
                    if (occupiedNeighbours > birthLimit) {
                        newMap[x, y] = 1;
                    } else {
                        newMap[x, y] = 0;
                    }
                }
            }
        }
        return newMap;
    }

    int getSurroundingWallCount(int[,] queryMap, int gridX, int gridY) {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                // check all tiles within map bounds to see if they are walls
                if (isInMapRange(neighbourX, neighbourY)) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        // walls are values as 1 and void is 0
                        wallCount += queryMap[neighbourX, neighbourY];
                    }
                    // tiles outside map always count as walls
                } else {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    bool isInMapRange(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    int[,] processRooms(int[,] map) {
        List<List<Coord>> wallRegions = getRegions(map, 1);
        foreach (List<Coord> wallRegion in wallRegions) {
            if (wallRegion.Count < wallThresholdSize) {
                foreach (Coord tile in wallRegion) {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = getRegions(map, 0);
        List<Room> validRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions) {
            if (roomRegion.Count < roomThresholdSize) {
                foreach (Coord tile in roomRegion) {
                    map[tile.tileX, tile.tileY] = 1;
                }
            } else {
                validRooms.Add(new Room(roomRegion, map));
            }
        }
        if (connectRooms) {
            validRooms.Sort();
            validRooms[0].isMainRoom = true;
            validRooms[0].isAccessible = true;
            map = connectClosestRooms(map, validRooms);
        }
        return map;
    }

    List<List<Coord>> getRegions(int[,] map, int tileType) {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // find unchecked tile to begin searching for tile from
                if (mapFlags[x, y] == 0 && map[x, y] == tileType) {
                    List<Coord> newRegion = getRegionTiles(map, x, y);
                    regions.Add(newRegion);
                    // mark each tile in the region as having been looked at
                    foreach (Coord tile in newRegion) {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> getRegionTiles(int[,] map, int startX, int startY) {
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

    int[,] connectClosestRooms(int[,] map, List<Room> rooms, bool forceAccessibilityFromMainRoom = false) {
        List<Room> roomListA = new List<Room>();
        List<Room> accessibleRooms = new List<Room>();

        if (forceAccessibilityFromMainRoom) {
            foreach (Room room in rooms) {
                if (room.isAccessible) {
                    accessibleRooms.Add(room);
                } else {
                    roomListA.Add(room);
                }
            }
        } else {
            roomListA = rooms;
            accessibleRooms = rooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room a in roomListA) {
            if (!forceAccessibilityFromMainRoom) {
                // if we're forcing accessibility we want to consider all rooms for best match to any 
                // accessible rooms, not just the first pairing as better rooms in the network may be found
                possibleConnectionFound = false;

                // skip room if already connected and we're not forcing accessibility
                if (a.connectedRooms.Count > 0) {
                    continue;
                }
            }
            foreach (Room b in accessibleRooms) {
                if (a == b || a.isConnected(b)) {
                    // don't try to connect a room to itself or to a room it's already connected to
                    continue;
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

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
                // only create passage if not waiting for best accessible match
                createPassage(map, bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom) {
            // only create passage out of loop if waiting for best accessible match
            createPassage(map, bestRoomA, bestRoomB, bestTileA, bestTileB);
            // check again for more required connections
            connectClosestRooms(map, rooms, true);
        }

        if (!forceAccessibilityFromMainRoom) {
            connectClosestRooms(map, rooms, true);
        }
        return map;
    }

    void createPassage(int[,] map, Room roomA, Room roomB, Coord tileA, Coord tileB) {
        Room.connectRooms(roomA, roomB);
        List<Coord> line = getLine(tileA, tileB);
        foreach (Coord c in line) {
            drawCircle(map, c, passageRadius);
        }
    }

    void drawCircle(int[,] map, Coord tile, int radius) {
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                if (x * x + y * y <= radius * radius) {
                    int drawX = tile.tileX + x;
                    int drawY = tile.tileY + y;
                    if (isInMapRange(drawX, drawY)) {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    /**
     * Iterate along the longest axis between the two coords checking for the point 
     * at which the gradient crosses the threshold on the other axis.
    **/
    List<Coord> getLine(Coord from, Coord to) {
        List<Coord> line = new List<Coord>();
        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);
        bool inverted;
        int step;
        int gradientStep;

        if (longest < shortest) {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);
            step = System.Math.Sign(dy);
            gradientStep = System.Math.Sign(dx);
        } else {
            inverted = false;
            step = System.Math.Sign(dx);
            gradientStep = System.Math.Sign(dy);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++) {
            line.Add(new Coord(x, y));

            if (inverted) {
                y += step;
            } else {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest) {
                if (inverted) {
                    x += gradientStep;
                } else {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }
}
