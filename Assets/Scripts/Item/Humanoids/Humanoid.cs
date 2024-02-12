using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public class Humanoid : Item {

    public static List<string> orientations = new List<string>() {
        "front",
        "right",
        "back",
        "left"
    };
    public static List<string> cardinals = new List<string>() {
        "north",
        "east",
        "south",
        "west"
    };

    public Coords coords = new Coords(-1, -1);
    public int tilesetId;

    public override void Init() {
        base.Init();
        _ = CreateChildItem(ItemData.Generate_Special("body") as Body);
    }

    public bool CanMoveForward(Coords c) {
        var targetTile = TileSet.GetCurrent.GetTile(c);
        if (targetTile == null)
            return false;
        if (targetTile.HasProp("blocking"))
            return false;
        return true;
    }

    public virtual void Move(Coords targetCoords) {
        // change current coords
        var newOrientation = GetCardinalFromDirection(targetCoords - coords);
        Player.Instance.SetProp($"orientation / value:{newOrientation}");
        Debug.Log($"the player looks : {newOrientation}");
        coords = targetCoords;
    }


    public static string GetCardinalFromDirection(Coords coords) {
        if ( coords.x > 0) { return "east"; }
        else if (coords.y > 0) { return "north"; }
        else if (coords.y < 0) { return "south"; }
        else if (coords.x < 0) { return "west"; }
        else { return "north"; }
    }
    public static string GetOrientationFromDirection(Coords coords) {
        if ( coords.x > 0) { return "right"; }
        else if (coords.y > 0) { return "front"; }
        else if (coords.y < 0) { return "back"; }
        else if (coords.x < 0) { return "left"; } else { return "front"; }
    }
    public static Coords GetCoordsFromCardinal(string str) {
        switch (str) {
            case "north":
                return new Coords(0, 1);

            case "east":
                return new Coords(1, 0);

            case "south":
                return new Coords(0, -1);

            case "west":
                return new Coords(-1, 0);
            default:
                Debug.LogError($"no coords for cardinal {str}");
                return Coords.Zero;
        }
    }
    public Coords GetCoordsFromOrientation(string targetOrientation) {
        return GetCoordsFromCardinal(GetCardinalFromOrientation(targetOrientation));
    }

    public string GetCardinalFromOrientation(string targetOrientation) {
        var playerOrientation = GetProp("orientation").GetTextValue();
        int orientationNum = cardinals.IndexOf(playerOrientation);
        int cardinalNum = orientations.IndexOf(targetOrientation);
        if (orientationNum == -1 || cardinalNum == -1) {
            Debug.LogError($"orientation / cardinal error\n" +
                $"player orientation : {playerOrientation} / target orientation : {targetOrientation}");
            return "none";
        }
        return cardinals[(orientationNum + cardinalNum) % orientations.Count];
    }

}
