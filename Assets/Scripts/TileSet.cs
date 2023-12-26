using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSet {

    public static List<TileSet> tileSets = new List<TileSet>();
    public static TileSet GetCurrent => tileSets[Player.Instance.tilesetId];
    public static TileSet GetTileSet(int id) { return tileSets[id]; }
    public static void SetCurrent (int id) { Player.Instance.tilesetId = id; }
    public int timeScale = 10;
    public static TileSet world => tileSets[0];
    public Coords startCoords = Coords.Zero;

    public Dictionary<Coords, Tile> tiles = new Dictionary<Coords, Tile>();
    public int width;
    public int height;
    public int id;

    public Coords Center => new Coords((int)(width / 2f), (int)(height / 2f));

    public void Init() {
        width = MapTexture.Instance.mainMap_Texture.width;
        height = MapTexture.Instance.mainMap_Texture.height;
    }

    public void Add(Coords c, Tile newTile) {
        tiles.Add(c, newTile);
    }

    public static void ChangeTileSet(int id) {
        // save currnet player coords
        SetCurrent(id);
        Player.Instance.coords = GetCurrent.startCoords;
        TimeManager.ChangeMovesPerHour(GetCurrent.timeScale);
        MapTexture.Instance.UpdateInteriorMap();
        Player.Instance.Move(Cardinal.None);
    }

    public Tile GetTile(Coords coords) {
        if (tiles.ContainsKey(coords) == false)
            return null;
        return tiles[coords];
    }
}