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
    public static Tile create(Coords _coords, string _name, string spec_str = "") {

        var tile = ItemData.Generate_Special(_name) as Tile;

        tile.coords = _coords;

        if (!string.IsNullOrEmpty(spec_str))
            tile.setSpec(spec_str, spec_str, "main");

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
        writeHumanoids();
        TimeManager.writeTimeOfDay();
        TimeManager.writeWeatherDescription();

        //Property.DescribeUpdated();
    }

    public override void WriteDescription() {
        //base.WriteDescription();

        // "you're on a tile...
        TextManager.Write(getDescriptionType, this);

        // putting items that contain things and items that don't in the same list
        // (testing to see how it looks)
        var mainItms = GetChildItems().FindAll(x => x as Tile == null);
        var groups = ItemGroup.GetGroups(mainItms);
        string main_str = $"there's {ItemGroup.GetDescription(groups, false)}";
        TextManager.Write(main_str);

        // main items
        /*List<Item> mainItms = getItems().FindAll(x => !x.HasChildItems() && x as Tile == null);
        string main_str = $"there's {TextManager.listItems(mainItms)}";
        TextManager.Write(main_str);

        // containers
        List<Item> containers = getItems().FindAll(x => x.HasChildItems() && x as Tile == null);
        foreach (var item in containers)
            item.WriteChildItems();*/

        List<Item> tiles = GetAdjacentTiles();
        foreach (var tile in tiles) {
            tile.GenerateChildItems();
        }
        DebugManager.Instance.adjacentTiles = tiles;
        var tileGroups = ItemGroup.GetGroups(tiles);
        DebugManager.Instance.debug_groups = tileGroups;
        //string description = ItemGroup.GetDescription(tileGroups, true);
        string description = ItemGroup.GetDescription(tiles, true);
        TextManager.Write($"around you,\n{description}");


        // humanoids now...
    }
    public string getDescriptionType {
        get {
            if (HasProp("definite"))
                return GetPrevious != null && GetPrevious.coords == coords ? "tile_wait" : "tile_goback";
            else
                return GetPrevious != null && sameTypeAs(GetPrevious) && GetPrevious.coords != coords ? "tile_continue" : "tile_discover";
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
            var opp_orientation = Humanoid.getOpp(orientation).ToString();
            // if the tile is enclosed (often an interior)
            if (adjacentTile.HasProp("enclosed")) {

                // there's a door
                var door = getItemWithSpec(orientation.ToString());
                if (door == null)
                    door = CreateChildItem("door");
                else
                    Debug.LogError($"{debug_name} already hasPart item with spec {orientation.ToString()}");
                door.setSpec($">on the {orientation}", orientation.ToString(), orientation.ToString());
                exits.Add(door);

            } else if (Player.Instance.canSee()){
                adjacentTile.setSpec($">on the {orientation}", orientation.ToString(), orientation.ToString());
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

    public void writeHumanoids() {

        
    }

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
    }

    public static void SetPrevious(Tile tile) {
        _previous = tile;
    }

}