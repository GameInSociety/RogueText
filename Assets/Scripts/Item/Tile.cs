using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using static UnityEditor.Progress;

[System.Serializable]
public class Tile : Item {
    // location of tile in world
    public Coords coords {
        get {
            return TileInfo.coords;
        }
    }
    [System.Serializable]
    public struct Info {
        public Coords coords;
        public int setID;
        public Info(Coords c, int i) {
            coords = c;
            setID = i;
        }
        public Tile GetTile() {
            return TileSet.GetTileSet(setID).GetTile(coords);
        }
    }

    public static bool itemsChanged = false;
    public static Tile Create(Info info, string _name, string prop_text = "") {

        var tile = ItemData.Generate_Special(_name) as Tile;
        tile.TileInfo = info;
        if (!string.IsNullOrEmpty(prop_text)) {
            tile.SetProp($"dif / description:{prop_text}");
        }

        return tile;
    }

    #region tile description
    public void Describe() {
        DisplayDescription.Instance.ClearDescription();
        // update tiles to create doors
        GetAdjacentTiles();
        GenerateChildItems();
        WriteDescription();
    }

    public override void WriteDescription() {
        //base.WriteDescription();

        // "you're on a tile...
        string intro = GetDescriptionType();
        TextManager.Write(intro);
        SetProp("visited");

        TextManager.Return();

        // putting items that contain things and items that don't in the same list
        // (testing to see how it looks)
        if (HasChildItems()) {
            string main_str = $"you see {ItemDescription.NewDescription(GetChildItems())}";
            TextManager.Write(main_str);
            TextManager.Return();
        }

        string description = ItemDescription.NewDescription(GetAdjacentTiles());
        TextManager.Write($"there's {description}");
        //AdjacentTilesDescription();
    }

    public void AdjacentTilesDescription() {
        List<Item> tiles = GetAdjacentTiles();
        DebugManager.Instance.adjacentTiles = tiles;
        foreach (var tile in tiles)
            tile.GenerateChildItems();

        //List<Item> similarTiles = tiles.FindAll(x => x.GetVisibleProp(0).GetDescription() == GetVisibleProp(0).GetDescription());
        List<Item> similarTiles = tiles.FindAll(x => x.dataIndex == dataIndex);
        if ( similarTiles.Count > 0 ) {
            string text = $"{GetText("the dog")} continues ";
            for (int i = 0; i < similarTiles.Count; i++) {
                var tile = similarTiles[i];
                text += $"{tile.GetPropertyOfType("orientation")?.GetDescription()}{TextUtils.GetCommas(i, similarTiles.Count)}";
                tiles.Remove(tile);
            }
            TextManager.Write(text);
        }

        if (tiles.Count  > 0) {
            string description = ItemDescription.NewDescription(tiles);
            TextManager.Write($"there's {description}");
        }

        foreach (var tile in tiles) {
            tile.SetProp("definite");
        }
    }

    public string GetDescriptionType (){
        if (HasProp("visited")) {
            if (GetPrevious != null && GetPrevious.coords == coords)
                return $"you're still {GetText("on the special dog")}";
            
                return $"you're back {GetText("on the special dog")}";
        } else {
            if (GetPrevious != null && sameTypeAs(GetPrevious) && GetPrevious.coords != coords)
                return $"you continue {GetText("on the dog")}";
            else
                return $"you're {GetText("on a special dog")}";
        }
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

                var link = exit.GetProp("link");
                if ( link == null)
                    link = exit.AddProp("link");
                link.SetValue(adjacentTile.GetText("the dog"));
            } else {
                exit = adjacentTile;
                exits.Add(exit);
            }

            foreach (var or in Humanoid.orientations) {
                if (exit.HasProp(or.ToString())) {
                    exit.RemoveProp(or.ToString());
                    break;
                }
            }
            var prop = exit.AddProp(orientation.ToString());
            prop.SetValue(Player.Instance.GetCardinalFromOrientation(orientation));
        }

        return exits;
    }


    public Tile GetAdjacentTile(string orientation) {
        var targetCoords = coords + Player.Instance.GetCoordsFromOrientation(orientation);
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
    public static Tile GetCurrent => _current;

    public static void SetCurrent(Tile tile) {
        SetPrevious(_current);

        DebugManager.Instance.TILE = tile;
        _current = tile;
        tile.SetProp("around / description:you're standing on it / search:standing on");
    }

    public static void SetPrevious(Tile tile) {
        _previous = tile;
        if (tile == null)
            return;
    }

}