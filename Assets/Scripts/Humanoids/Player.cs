using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class Player : Humanoid {

    public static Player Instance;

    public Item Inventory;

    public override void Init(Item copy) {
        base.Init(copy);

        Inventory = addItem("bag");
        Inventory.addItem("plate");
    }


    public override void writeDescription() {
        //base.WriteDescription();

        foreach (var item in GetBody.getContainedItems) {
            if (item.hasProperties()) {
                item.WriteProperties();
            }
        }
    }

    #region vision
    public bool canSee() {
        if (TimeManager.Instance.currentPartOfDay == TimeManager.PartOfDay.Night) {
            if (HasItemWithProperty("source of light")) {
                TextManager.write("lamp_on");
                return true;
            } else {
                TextManager.write("lamp_off");
                return false;
            }
        }

        return true;
    }
    #endregion

    #region movement
    public override void Move(Coords targetCoords) {
        // cancel if path blocked
        if (!CanMoveForward(targetCoords)) {
            WriteBlockFeedback(targetCoords);
            return;
        }

        // first of all, advance time ( and items states etc... )
        TimeManager.Instance.AdvanceTime();

        base.Move(targetCoords);

        Tile.SetCurrent(TileSet.current.GetTile(coords));


        // generate tile items
        Tile.GetCurrent.describe();

        MapTexture.Instance.UpdateFeedbackMap();
    }

    public void MoveToTile(Tile tile) {
        if (tile != null) {
            Debug.Log("moving to tile : " + tile.debug_name + " / " + tile.debug_randomID);

            if (tile.coords == coords) {
                TextManager.write("You are already &on the dog&", tile);
                return;
            }

            Move(tile.coords);
        }

    }


    #endregion

    public void WriteBlockFeedback(Coords c) {
        var targetTile = TileSet.current.GetTile(c);

        if (targetTile == null) {
            Debug.LogError("no tile : " + c.ToString());
            TextManager.write("blocked_void");
            return;
        }

        switch (targetTile.debug_name) {
            case "hill":
                TextManager.write("blocked_hill");
                break;
            case "mountain":
                TextManager.write("blocked_mountain");
                break;
            case "sea":
                TextManager.write("blocked_sea");
                break;
            case "lake":
                TextManager.write("blocked_lake");
                break;
            case "river":
                TextManager.write("blocked_river");
                break;
            default:
                break;
        }
    }

    public override void Orient(Orientation orientation) {
        TextManager.SetOverrideOrientation(orientation);
        TextManager.write("position_orientPlayer");

        base.Orient(orientation);
    }
}