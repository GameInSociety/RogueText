using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class Player : Humanoid {

    public static Player Instance;

    public override void Init()
    {

        base.Init();

        coords = Tile.GetCurrent.coords;

        AddItem("bag");
    }

    public static Item Inventory
    {
        get
        {
            return Instance.GetItem("bag");
        }
    }

    public override void WriteDescription()
    {
        //base.WriteDescription();

        foreach (var item in body.GetContainedItems)
        {
            if ( item.HasProperties() )
            {
                item.WriteProperties();
            }
        }
    }

    #region vision
    public bool CanSee()
    {
        if (TimeManager.Instance.currentPartOfDay == TimeManager.PartOfDay.Night)
        {
            if (HasItemWithProperty("source of light"))
            {
                TextManager.Write("lamp_on");
                return true;
            }
            else
            {
                TextManager.Write("lamp_off");
                return false;
            }
        }

        return true;
    }
    #endregion

    #region movement
    public override void Move(Coords targetCoords)
    {
        // cancel if path blocked
        if (!CanMoveForward(targetCoords))
        {
            WriteBlockFeedback(targetCoords);
            return;
        }

        // first of all, advance time ( and items states etc... )
        TimeManager.Instance.AdvanceTime();

        base.Move(targetCoords);

        Tile.SetCurrent(TileSet.current.GetTile(coords));

        // generate tile items
        Tile.GetCurrent.Describe();

        MapTexture.Instance.UpdateFeedbackMap();
    }

    public void MoveToTile(Tile tile)
    {
        if ( tile != null)
        {
            Debug.Log("moving to tile : " + tile.debug_name + " / " +  tile.debug_randomID);

            if ( tile.coords == coords)
            {
                TextManager.Write("You are already &on the dog&", tile);
                return;
            }

            Move(tile.coords);
        }

    }

   
    #endregion

    public void WriteBlockFeedback(Coords c)
    {
        Tile targetTile = TileSet.current.GetTile(c);

        if (targetTile == null)
        {
            Debug.LogError("no tile : " + c.ToString());
            TextManager.Write("blocked_void");
            return;
        }

        switch (targetTile.debug_name)
        {
            case "hill":
                TextManager.Write("blocked_hill");
                break;
            case "mountain":
                TextManager.Write("blocked_mountain");
                break;
            case "sea":
                TextManager.Write("blocked_sea");
                break;
            case "lake":
                TextManager.Write("blocked_lake");
                break;
            case "river":
                TextManager.Write("blocked_river");
                break;
            default:
                break;
        }
    }

    public override void Orient(Orientation orientation)
    {
        TextManager.SetOverrideOrientation(orientation);
        TextManager.Write("position_orientPlayer");

        base.Orient(orientation);
    }
}