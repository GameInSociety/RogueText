using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileSet {

    public static List<TileSet> tileSets = new List<TileSet>();
    public static TileSet GetCurrent {
        get {
            var tilesetId = tileSets[Player.Instance.tilesetId];
            MapTexture.Instance.DisplayMap(tilesetId);
            return tilesetId;
        }
    }
    public static TileSet GetTileSet(int id) { return tileSets[id]; }
    public int timeScale = 10;
    public static TileSet world => tileSets[0];
    public Coords startCoords = Coords.zero;

    public Dictionary<Coords, Tile> tiles = new Dictionary<Coords, Tile>();
    public int width;
    public int height;

    public Coords Center => new Coords((int)(width / 2f), (int)(height / 2f));

    public void Init() {
        width = MapTexture.Instance.mainMap_Texture.width;
        height = MapTexture.Instance.mainMap_Texture.height;
    }

    public Coords GetRandomCoords() {
        List<Coords> cs = tiles.Keys.ToList();
        return cs[Random.Range(0, cs.Count)];
    }

    public void Add(Coords c, Tile newTile) {
        if ( c.x > width )
            width = c.x;
        if ( c.y > height )
            height = c.y;

        tiles.Add(c, newTile);
    }

    public Tile GetTile(Coords coords) {
        if (tiles.ContainsKey(coords) == false) {
            return null;
        }
        return tiles[coords];
    }
}