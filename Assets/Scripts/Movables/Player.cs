using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Player : Movable {

    public static Player Instance;

    
    /// <summary>
    /// STATs
    /// </summary>
    public Stats stats;

    // COORDS
    private Coords startCoords = new Coords(9,3);
    // beach 9 3
    // farm 9 9

    public Player()
    {

    }

    public override void Init()
    {
        base.Init();

        // equipement
        Equipment equipment = new Equipment();
        equipment.Init();

        // stats
        stats = new Stats();

        coords = startCoords;

    }

    #region vision
    public bool CanSee()
    {
        if (TimeManager.Instance.currentPartOfDay == TimeManager.PartOfDay.Night)
        {
            if (Inventory.Instance.HasItemWithProperty("source of light"))
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

        // set preivous & current tile
        
        if ( Tile.Current != null)
        {
            Tile.Current.SetPrevious();
        }

        Tile.SetCurrent(TileSet.current.GetTile(coords));

        // generate tile items
        Tile.Current.Describe();

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

    

    

    /// <summary>
    /// ORIENTATION : front, left, right, back etc...
    /// CARDINAL : north, west, south east
    /// DIRECTION : to north, to west, to east, to south
    /// </summary>

}

public class Stats
{
    public enum Type
    {
        Strengh,
        Dexterity,
        Charisma,
        Constitution,
    }

    public int[] values = new int[4];

    public int GetStat(Type t)
    {
        return values[(int)t];
    }
}