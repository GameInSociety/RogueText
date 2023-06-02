using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Write : Function
{
    public override void Call()
    {
        base.Call();

        if ( GetParam(0) == "_north")
        {
            Movable.Orientation orientation = Coords.GetOrientationFromNorth(Player.Instance.currentCarnidal);
            TextManager.SetOverrideOrientation(orientation);
            TextManager.Write("compas_giveNorth");
            return;
        }

        if ( GetParam(0) == "_tile")
        {
            Tile.Current.Describe();
            return;
        }

        TextManager.Write(GetParam(0));
    }
}
