using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Player : Function
{
    public override void Call(List<Item> items)
    {
        base.Call(items);

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
    }

    void ToTile()
    {
        Item targetItem = GetItem();
        Tile tile = targetItem as Tile;

        Player.Instance.MoveToTile(tile);
    }

    void ToDoor()
    {
        Item targetItem = GetItem();

        Property prop = targetItem.GetPropertyOfType("direction");
        Movable.Orientation orientation = Coords.GetOrientationFromString(prop.name);

        Debug.Log("target orientation = " + orientation);

        if (Tile.GetCurrent.GetAdjacent(orientation) == null)
        {
            Debug.Log("exiting because no tile after");

            // no tile, so exit interior
            Interior.Current.Exit();
        }
        else
        {
            Player.Instance.Move(orientation);
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
        Equipment.Instance.Equip(part, GetItem());
    }

    void Unequip()
    {
        Equipment.Part part = Equipment.GetPartFromString(GetParam(1));
        Equipment.Instance.Unequip(part);
    }

}
