using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Player : Function {
    public override void Call() {
        base.Call();
        Call(this);
    }

    void move() {
        if (GetParam(0) == "door") {
            ToDoor();
            return;
        }

        if (GetParam(0) == "tile") {
            ToTile();
            return;
        }

        if (GetParam(0) == "relative") {
            ToOrientation();
            return;
        }
    }

    void ToOrientation() {
        var moveOrientation = (Player.Orientation)ParseParam(1);
        Player.Instance.Move(Player.OrientationToCardinal(moveOrientation));
    }

    void ToTile() {
        var targetItem = base.targetItem();
        var tile = targetItem as Tile;

        Player.Instance.MoveToTile(tile);
    }

    void ToDoor() {
        var targetItem = base.targetItem();

        var prop = targetItem.GetPropertyOfType("direction");
        var orientation = Coords.GetOrientationFromString(prop.name);

        Debug.Log("target orientation = " + orientation);

        if (Tile.GetCurrent.getAdjacent(orientation) == null) {
            Debug.Log("exiting because no tile after");

            // no tile, so exit interior
            Interior.Current.Exit();
        } else {
            Player.Instance.Move(orientation);
        }
    }

    void orient() {
        var lookOrientation = (Player.Orientation)ParseParam(0);
        Player.Instance.Orient(lookOrientation);
    }

    void equip() {
        Player.Instance.GetBody.Equip(targetItem());
    }

    void unequip() {
        Player.Instance.GetBody.Unequip(targetItem());

    }

}
