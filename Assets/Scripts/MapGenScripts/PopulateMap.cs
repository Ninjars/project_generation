using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulateMap : MonoBehaviour {

	public GameObject resourceObj;

    public int resourceCount = 5;

	struct Coord {
		public int tileX;
		public int tileY;

		public Coord(int x, int y) {
			tileX = x;
			tileY = y;
		}
	}

    public void populateMap(System.Random random, int[,] map) {
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);
        List<Coord> landTiles = filterTileCoords(map, -mapWidth/2, -mapHeight/2, 0);

        if (GlobalRegister.resources != null) {
            foreach (GameObject obj in GlobalRegister.resources) {
                Destroy(obj);
            }
        }
        List<GameObject> resourceTiles = populateResources(random, landTiles);
        GlobalRegister.resources = resourceTiles;
	}

    /*
     * Selects random tiles to place resources on them, manipulating unpopulatedTiles 
     * to remove these resource tiles from it.
     */
    private List<GameObject> populateResources(System.Random random, List<Coord> unpopulatedTiles) {
        List<GameObject> resources = new List<GameObject>();
        for (int i = 0; i < resourceCount; i++) {
            int tileIndex = random.Next(0, unpopulatedTiles.Count);
            Coord resTile = unpopulatedTiles[tileIndex];
            unpopulatedTiles.Remove(resTile);
            GameObject resource = Instantiate(resourceObj, new Vector3(resTile.tileX, 0, resTile.tileY), Quaternion.identity);
            resources.Add(resource);
        }
        return resources;
    }

	private List<Coord> filterTileCoords(int[,] map, int horizontalOffset, int verticalOffset, int filterValue) {
		List<Coord> returnTiles = new List<Coord>();
		for (int x = 0; x < map.GetLength(0); x++) {
			for (int y = 0; y < map.GetLength(1); y++) {
				if (map[x, y] == filterValue) {
                    returnTiles.Add(new Coord(x + horizontalOffset, y + verticalOffset));
				}
			}
		}
		return returnTiles;
	}
}
