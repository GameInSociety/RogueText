using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Interior : Function
{
    public override void Call(List<Item> items)
    {
        base.Call(items);

        if ( GetParam(0) == "enter")
        {
            Interior interior = GetItem().interior;

            if ( interior == null)
            {
                interior = new Interior();
                interior.Genererate(GetItem());
            }

            interior.Enter();
            return;
        }

        if ( GetParam(0) == "describeOut")
        {
            Cardinal cardinal = Coords.GetCardinalFromString(GetItem().GetProperty("direction").value);
            Coords targetCoords = TileSet.map.playerCoords + (Coords)cardinal;

            Tile tile = TileSet.map.GetTile(targetCoords);

            if (tile == null)
            {
                TextManager.Write("The view is blocked by some bushes");
            }
            else
            {
                TextManager.Write("tile_describeExterior", tile);
            }
            return;
        }
    }
}
