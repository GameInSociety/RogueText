using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Player : Function
{
    public override void Call()
    {
        base.Call();

        if ( GetParam(0) == "move")
        {
            Move();
        }

        if ( GetParam(0) == "orient")
        {
            Orient();
            return;
        }
        
    }

    void Move()
    {
        if (GetParam(1) == "door")
        {
            ToDoor();
            return;
        }

        if (GetParam(1) == "tile")
        {
            ToTile();
            return;
        }

        if (GetParam(1) == "relative")
        {
            ToOrientation();
            return;
        }

        ToCardinal();
        return;
    }

    void ToCardinal()
    {
        Player.Instance.Move((Cardinal)ParseParam(1));
    }

    void ToOrientation()
    {
        Player.Orientation moveOrientation = (Player.Orientation)ParseParam(2);
        Player.Instance.Move(Player.OrientationToCardinal(moveOrientation));
        Debug.Log("moving relative");
    }

    void ToTile()
    {
        Item targetItem = WorldEvent.current.GetCurrentItem();
        Tile tile = targetItem as Tile;

        Player.Instance.MoveToTile(tile);
    }

    void ToDoor()
    {
        Item targetItem = WorldEvent.current.GetCurrentItem();

        if (!targetItem.HasProperty("direction"))
        {
            Interior.Current.Exit();
            return;
        }

        Property prop = targetItem.GetProperty("direction");
        Cardinal cardinal = Coords.GetCardinalFromString(targetItem.GetProperty("direction").value);

        if (Tile.Current.GetAdjacentTile(cardinal) == null)
        {
            // no tile, so exit interior
            Interior.Current.Exit();
        }
        else
        {
            Player.Instance.Move(cardinal);
        }
    }

    void Orient()
    {
        Movable.Orientation lookOrientation = (Player.Orientation)ParseParam(1);
        Player.Instance.Orient(lookOrientation);
    }

    void Equip()
    {
        Equipment.Part part = Equipment.GetPartFromString(GetParam(1));
        Equipment.Instance.Equip(part, WorldEvent.current.GetCurrentItem());
    }

    void Unequip()
    {
        Equipment.Part part = Equipment.GetPartFromString(GetParam(1));
        Equipment.Instance.Unequip(part);
    }

}
