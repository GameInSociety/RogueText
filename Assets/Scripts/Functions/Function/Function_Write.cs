using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class Function_Write : Function {
    public override void Call() {
        base.Call();
        Call(this);
    }

    void text() {
        TextManager.write(GetParam(0), targetItem());
    }

    void north() {
        var orientation = Coords.GetOrientationFromNorth(Player.Instance.currentCarnidal);
        TextManager.SetOverrideOrientation(orientation);
        TextManager.write("compas_giveNorth");
    }

    void _tile() {
        Tile.GetCurrent.describe();
    }

    void write() {
        TextManager.write(GetParam(0));
    }
}
