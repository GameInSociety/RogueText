using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Tile : Item {
    // location of tile in world
    public Coords coords;
    public static bool itemsChanged = false;
    public static Tile create(Coords _coords, string _name, string spec_str = "") {
        var tile = Generate_Special(_name) as Tile;
        tile.coords = _coords;

        if (!string.IsNullOrEmpty(spec_str))
            tile.setSpec(spec_str, spec_str, "main");

        return tile;
    }


    #region tile description
    public void describe() {
        
        if (FunctionSequence.current != null) {
            FunctionSequence.onFinishSequences += describe;
            return;
        }

        DisplayDescription.Instance.renew();

        // change orientation, so the description is correct
        if (Player.Instance.coords != coords) {
            var dir = coords - Player.Instance.coords;
            var cardinal = (Cardinal)dir;
            Player.Instance.Orient(Humanoid.CardinalToOrientation(cardinal));
        }

        generateItems();
        writeDescription();
        writeHumanoids();
        TimeManager.Instance.writeTimeOfDay();
        TimeManager.Instance.writeWeatherDescription();

        //Property.DescribeUpdated();
    }

    public override void writeDescription() {
        //base.WriteDescription();

        // "you're on a tile...
        TextManager.write(getDescriptionType, this);

        // putting items that contain things and items that don't in the same list
        // (testing to see how it looks)
        List<Item> mainItms = getItems().FindAll(x => x as Tile == null);
        string main_str = $"there's {DescriptionGroup.NewDescription(mainItms)}";
        TextManager.write(main_str);

        // main items
        /*List<Item> mainItms = getItems().FindAll(x => !x.containsItems() && x as Tile == null);
        string main_str = $"there's {TextManager.listItems(mainItms)}";
        TextManager.write(main_str);

        // containers
        List<Item> containers = getItems().FindAll(x => x.containsItems() && x as Tile == null);
        foreach (var item in containers)
            item.writeContainedItems();*/

        List<Item> tiles = getExits();
        DebugManager.Instance.adjacentTiles = tiles;
        TextManager.write($"around you, {DescriptionGroup.NewDescription(tiles, true)}");

        // humanoids now...
    }
    public string getDescriptionType {
        get {
            if (HasInfo("discovered"))
                return GetPrevious != null && GetPrevious.coords == coords ? "tile_wait" : "tile_goback";
            else
                return GetPrevious != null && SameTypeAs(GetPrevious) && GetPrevious.coords != coords ? "tile_continue" : "tile_discover";
        }
    }

    
    public List<Item> getExits() {

        var orientations = new List<Humanoid.Orientation>
        {
            Humanoid.Orientation.front,
            Humanoid.Orientation.right,
            Humanoid.Orientation.left,
            Humanoid.Orientation.back
        };

        List<Item> exits = new List<Item>();

        foreach (var orientation in orientations) {
            var adjacentTile = getAdjacent(orientation);
            // skip if the tile is null (a void)
            if (adjacentTile == null)
                continue;
            var opp_orientation = Humanoid.getOpp(orientation).ToString();
            // if the tile is enclosed (often an interior)
            if (adjacentTile.HasInfo("enclosed")) {

                // there's a door
                var door = getItemWithSpec(orientation.ToString());
                if (door == null)
                    door = addItem("door");
                else
                    Debug.LogError($"{debug_name} already has item with spec {orientation.ToString()}");
                door.setSpec($">on the {orientation}", orientation.ToString(), orientation.ToString());
                exits.Add(door);

            } else if (Player.Instance.canSee()){
                adjacentTile.setSpec($">on the {orientation}", orientation.ToString(), orientation.ToString());
                exits.Add(adjacentTile);
            }
        }

        return exits;
    }


    public Tile getAdjacent(Humanoid.Orientation orientation) {
        var dir = Humanoid.OrientationToCardinal(orientation);

        var targetCoords = coords + (Coords)dir;

        return TileSet.current.GetTile(targetCoords);
    }
    public Tile GetAdjacent(Cardinal cardinal) {
        var targetCoords = coords + (Coords)cardinal;

        return TileSet.current.GetTile(targetCoords);
    }
    #endregion

    public void writeHumanoids() {

        foreach (var item in getEnemies()) {
            TextManager.write("&a dog& is " + item.GetProperty("steps").GetDescription(), item);
        }
    }

    #region info
    public static bool SameAsPrevious() {
        if (GetPrevious == null) {
            return false;
        }

        return GetCurrent.SameTypeAs(GetPrevious);
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