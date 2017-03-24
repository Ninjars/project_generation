using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenGame {
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

            GlobalRegister.clearResources();
            populate(random, landTiles);
    	}

        /*
         * Selects random tiles to place resources on them, manipulating unpopulatedTiles 
         * to remove these resource tiles from it.
         */
        private void populate(System.Random random, List<Coord> unpopulatedTiles) {
            for (int i = 0; i < resourceCount; i++) {
                int tileIndex = random.Next(0, unpopulatedTiles.Count);
                Coord resTile = unpopulatedTiles[tileIndex];
                unpopulatedTiles.Remove(resTile);
                Instantiate(resourceObj, new Vector3(resTile.tileX, 0, resTile.tileY), Quaternion.identity);
            }
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
}
