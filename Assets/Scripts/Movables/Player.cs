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
    private Coords startCoords = new Coords(9,9);

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

        PlayerActionManager.onPlayerAction += HandleOnAction;

        coords = startCoords;

    }

    void HandleOnAction(PlayerAction action)
    {
        switch (action.type) {
            case PlayerAction.Type.Move:
                Move((Cardinal)PlayerAction.GetCurrent.GetValue(0));
                break;
            case PlayerAction.Type.MoveRel:
                Orientation moveOrientation = (Orientation)PlayerAction.GetCurrent.GetValue(0);
                Move(OrientationToCardinal( moveOrientation));
                break;
            case PlayerAction.Type.OrientPlayer:
                Orientation lookOrientation = (Orientation)PlayerAction.GetCurrent.GetValue(0);
                Orient(lookOrientation);
                break;
            case PlayerAction.Type.MoveToTargetItem:
                MoveToTargetItem();
                break;
            case PlayerAction.Type.Look:
                break;
            case PlayerAction.Type.UseDoor:
                UseDoor();
                break;
            case PlayerAction.Type.Enter:
                Interior.Get(coords).Enter();
                break;
            default:
                break;
        }
    }

    void UseDoor()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        if (!targetItem.HasProperty("direction")){
            Interior.GetCurrent.Exit();
            return;
        }

        Property prop = targetItem.GetProperty("direction");
        Cardinal cardinal = Coords.GetCardinalFromString(targetItem.GetProperty("direction").value);

        if ( Tile.GetCurrent.GetAdjacentTile(cardinal) == null)
        {
            // no tile, so exit interior
            Interior.GetCurrent.Exit();
        }
        else
        {
            Move(cardinal);
        }
    }

    #region vision
    public bool CanSee()
    {
        if (TimeManager.GetInstance().currentPartOfDay == TimeManager.PartOfDay.Night)
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
        TimeManager.GetInstance().AdvanceTime();

        base.Move(targetCoords);

        // set preivous & current tile
        Tile.SetPrevious(Tile.GetCurrent);
        Tile.SetCurrent(TileSet.current.GetTile(coords));

        // generate tile items
        DisplayDescription.Instance.UpdateDescription();

        MapTexture.Instance.UpdateFeedbackMap();
    }

    public void MoveToTargetItem()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        Tile tile = targetItem as Tile;

        if ( tile != null)
        {
            if ( tile.coords == Player.Instance.coords)
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