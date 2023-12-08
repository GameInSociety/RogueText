using Newtonsoft.Json;
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
    public Coords coords;
    public static bool itemsChanged = false;
    public static Tile Create(Coords _coords, string _name, string prop_text = "") {

        var tile = ItemData.Generate_Special(_name) as Tile;

        tile.coords = _coords;

        if (!string.IsNullOrEmpty(prop_text)) {
            tile.SetProp($"dif / description:{prop_text}");
        }

        return tile;
    }


    #region tile description
    public void Describe() {

        DisplayDescription.Instance.renew();

        // change orientation, so the description is correct
        if (Player.Instance.coords != coords) {
            var dir = coords - Player.Instance.coords;
            var cardinal = (Cardinal)dir;
            Player.Instance.Orient(Humanoid.CardinalToOrientation(cardinal));
        }

        GenerateChildItems();
        WriteDescription();

        //Property.DescribeUpdated();
    }

    public override void WriteDescription() {
        //base.WriteDescription();

        // "you're on a tile...
        string intro = GetDescriptionType;
        TextManager.Write(intro);
        SetProp("definite");

        // putting items that contain things and items that don't in the same list
        // (testing to see how it looks)
        if (HasChildItems()) {
            var mainItms = GetChildItems().FindAll(x => x as Tile == null);
            string main_str = $"there's {ItemDescription.NewDescription(mainItms, "show props")}";
            TextManager.Write(main_str);
        }

        AdjacentTilesDescription();
    }

    public void AdjacentTilesDescription() {
        List<Item> tiles = GetAdjacentTiles();
        DebugManager.Instance.adjacentTiles = tiles;
        foreach (var tile in tiles)
            tile.GenerateChildItems();

        List<Item> similarTiles = tiles.FindAll(x => x.GetVisibleProp(0).GetDescription() == GetVisibleProp(0).GetDescription());
        if ( similarTiles.Count > 0 ) {
            string text = $"{GetText("the dog")} continues ";
            foreach (var item in similarTiles) {
                text += $"{item.GetProp("orientation").GetDescription()}, ";
                tiles.Remove(item);
            }
            TextManager.Write(text);
        }

        string description = ItemDescription.NewDescription(tiles, "split lines, show props");
        TextManager.Write($"around you,\n{description}");
    }

    public string GetDescriptionType {
        get {
            if (HasProp("definite")) {
                if (GetPrevious != null && GetPrevious.coords == coords) {
                    return $"you're still {GetText("on the special dog")}";
                } else {
                    return $"you're back {GetText("on the special dog")}";
                }
            } else {
                if (GetPrevious != null && sameTypeAs(GetPrevious) && GetPrevious.coords != coords) {
                    return $"you continue {GetText("on the dog")}";
                } else {
                    return $"you're {GetText("on a special dog")}";
                }
            }
        }
    }

    
    public List<Item> GetAdjacentTiles() {

        var orientations = new List<Humanoid.Orientation>
        {
            Humanoid.Orientation.front,
            Humanoid.Orientation.right,
            Humanoid.Orientation.left,
            Humanoid.Orientation.back
        };

        List<Item> exits = new List<Item>();

        foreach (var orientation in orientations) {
            var adjacentTile = GetAdjacentTile(orientation);
            // skip if the tile is null (a void)
            if (adjacentTile == null)
                continue;
            // if the tile is enclosed (often an interior)
            if (adjacentTile.HasProp("enclosed")) {
                // there's a tileDoor
                var tileDoor = GetChildItems()?.Find(x=> x.HasProp("orientation") && x.GetProp("orientation").GetPart("search").text == orientation.ToString());
                if (tileDoor == null)
                    tileDoor = CreateChildItem("door");
                tileDoor.SetProp($"orientation / description:on the {orientation} / search:{orientation} / after word:yes");

            } else {
                adjacentTile.SetProp($"orientation / description:on the {orientation} / search:{orientation} / after word:yes");
                exits.Add(adjacentTile);
            }
        }

        return exits;
    }


    public Tile GetAdjacentTile(Humanoid.Orientation orientation) {
        var dir = Humanoid.OrientationToCardinal(orientation);

        var targetCoords = coords + (Coords)dir;

        return TileSet.GetCurrent.GetTile(targetCoords);
    }
    public Tile GetAdjacent(Cardinal cardinal) {
        var targetCoords = coords + (Coords)cardinal;

        return TileSet.GetCurrent.GetTile(targetCoords);
    }
    #endregion


    #region info
    public static bool SameAsPrevious() {
        if (GetPrevious == null) {
            return false;
        }

        return GetCurrent.sameTypeAs(GetPrevious);
    }
    public Humanoid.Orientation OrientationToPlayer() {
        var dir = coords - Player.Instance.coords;

        var cardinal = (Cardinal)dir;

        var orientation = Humanoid.CardinalToOrientation(cardinal);

        return orientation;
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
        tile.SetProp("orientation / description:you're standing on it / search:standing on / layer:1 ");
    }

    public static void SetPrevious(Tile tile) {
        _previous = tile;
        if (tile == null)
            return;
    }

}