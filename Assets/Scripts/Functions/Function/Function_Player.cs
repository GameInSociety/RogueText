using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Player : Function
{
    public override void TryCall(ItemGroup itemGroup)
    {
        base.TryCall(itemGroup);
        Call(this);
    }

    void move()
    {
        if (GetParam(0) == "door")
        {
            ToDoor();
            return;
        }

        if (GetParam(0) == "tile")
        {
            ToTile();
            return;
        }

        if (GetParam(0) == "relative")
        {
            ToOrientation();
            return;
        }
    }

    void ToOrientation()
    {
        Player.Orientation moveOrientation = (Player.Orientation)ParseParam(1);
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
        Humanoid.Orientation orientation = Coords.GetOrientationFromString(prop.name);

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

    void orient()
    {
        Humanoid.Orientation lookOrientation = (Player.Orientation)ParseParam(0);
        Player.Instance.Orient(lookOrientation);
    }

    void equip()
    {
        Player.Instance.body.Equip(GetItem());
    }

    void unequip()
    {
        Player.Instance.body.Unequip(GetItem());

    }

}
