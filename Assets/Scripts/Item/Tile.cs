using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[System.Serializable]
public class Tile : Item {
    // location of tile in world
    
    [System.Serializable]
    public struct Info {
        public Coords coords;
        public int tilesetId;
        public Info(Coords c, int i) {
            coords = c;
            tilesetId = i;
        }
        public Tile GetTile() {
            return TileSet.GetTileSet(tilesetId).GetTile(coords);
        }
    }

    public static bool itemsChanged = false;
    public static Tile Create(Info info, string _name, string prop_text = "") {

        var tile = ItemData.Generate_Special(_name) as Tile;
        if (!string.IsNullOrEmpty(prop_text)) {
            tile.SetProp($"dif | description:{prop_text}");
        }

        return tile;
    }

    #region tile description
    public void Describe() {
        // update tiles to create doors  
        var adjTiles = GetAdjacentTiles();
        var itemsToDescribe = new List<Item>();
        if (HasChildItems())
            itemsToDescribe.AddRange(GetChildItems());

        itemsToDescribe.AddRange(adjTiles);
        Debug.Log($"adjacent tiles : {adjTiles.Count}");

        ItemDescription.AddItems("tile description", itemsToDescribe, $"start:{GetText("on a dog")}, ");
        //ItemDescription.AddItems("tile description", itemsToDescribe, $"start:{GetText("on a dog")}, / type:group");
    }
    
    public List<Item> GetAdjacentTiles() {

        List<Item> exits = new List<Item>();

        for (int i = 0; i < Humanoid.orientations.Count; i++) {
            var orientation = Humanoid.orientations[i];
            var adjacentTile = GetAdjacentTile(orientation);
            // skip if the tile is null (a void)
            if (adjacentTile == null)
                continue;
            var exit = (Item)null;
                // if the tile is enclosed (often an interior)
            if (GetProp("enclosed")?.GetNumValue() == 1 || adjacentTile.GetProp("enclosed")?.GetNumValue() == 1) {
                // there's a tileDoor
                exit = GetChildItems()?.Find(x=> x.HasProp(orientation.ToString()));
                if (exit == null)
                    exit = CreateChildItem("door");
                exit.SetProp($"link | description:{adjacentTile.GetText("the dog")}");
            } else {
                exit = adjacentTile;
                exits.Add(exit);
            }

            exit.RemovePropOfType("orientation");
            var prop = exit.AddProp(orientation.ToString());
            prop.SetValue(Player.Instance.GetCardinalFromOrientation(orientation), "value", false);
        }

        return exits;
    }


    public Tile GetAdjacentTile(string orientation) {
        var targetCoords = GetCoords() + Player.Instance.GetCoordsFromOrientation(orientation);
        var tile = TileSet.GetCurrent.GetTile(targetCoords);
        return tile;
    }
    #endregion

    /// <summary>
    /// TOOLS
    /// </summary>
    private static Tile _previous;
    public static Tile GetPrevious => _previous;

    private static Tile _current;
    public static Tile GetCurrent {
        get {
            var playerts = Player.Instance.tilesetId;
            var tileset = TileSet.GetTileSet(playerts);
            var player_coords = Coords.PropToCoords(Player.Instance.GetProp("coords"), playerts);
            var currentTile = tileset.GetTile(player_coords);
            if (currentTile == null)
                Debug.LogError("current tile null");
            return currentTile;
        }
    }

    public static void SetCurrent(Tile tile) {
        SetPrevious(_current);
        _current = tile;
        tile.RemovePropOfType("orientation");
    }

    public static void SetPrevious(Tile tile) {
        _previous = tile;
        if (tile == null)
            return;

    }

}