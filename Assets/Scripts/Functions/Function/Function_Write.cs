using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class Function_Write : Function
{
    public override void TryCall(ItemGroup itemGroup)
    {
        base.TryCall(itemGroup);
        Call(this);
    }

    void text()
    {
        TextManager.Write(GetParam(0));
    }

    void north()
    {
        Humanoid.Orientation orientation = Coords.GetOrientationFromNorth(Player.Instance.currentCarnidal);
        TextManager.SetOverrideOrientation(orientation);
        TextManager.Write("compas_giveNorth");
    }

    void _tile()
    {
        Tile.GetCurrent.Describe();
    }

    void write()
    {
        TextManager.Write(GetParam(0));
    }
}
