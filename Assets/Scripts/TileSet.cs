using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileSet {

    public static List<TileSet> tileSets = new List<TileSet>();
    public static TileSet GetCurrent {
        get {
            var tilesetId = tileSets[Player.Instance.tilesetId];
            return tilesetId;
        }
    }
    public static TileSet NewTileset() {
        /// Create tile set 
		TileSet tileSet = new TileSet();
        tileSet.id = tileSets.Count;
        tileSet.width = MapLoader.Instance.width;
        tileSet.height = MapLoader.Instance.height;
        tileSets.Add(tileSet);
        return tileSet;
    }
    public static TileSet GetTileSet(int id) { return tileSets[id]; }
    public int timeScale = 10;
    public static TileSet world => tileSets[0];
    public Coords startCoords = Coords.zero;

    public Dictionary<Coords, Tile> tiles = new Dictionary<Coords, Tile>();
    public int id;
    public int width;
    public int height;

    public Coords Center => new Coords((int)(width / 2f), (int)(height / 2f));

    public Coords GetRandomCoords() {
        List<Coords> cs = tiles.Keys.ToList();
        return cs[Random.Range(0, cs.Count)];
    }

    public void Add(Coords c, Tile newTile) {
        if ( c.x > width )
            width = c.x+1;
        if ( c.y > height )
            height = c.y+1;

        if (tiles.ContainsKey(c)) {
            Debug.LogError($"already {newTile._debugName} to {c.ToString()}");
            return;
        }
        tiles.Add(c, newTile);
    }

    public Tile GetTile(Coords coords) {
        if (tiles.ContainsKey(coords) == false) {
            Debug.LogError($"tile {coords} does not exist");
            return null;
        }
        return tiles[coords];
    }
}