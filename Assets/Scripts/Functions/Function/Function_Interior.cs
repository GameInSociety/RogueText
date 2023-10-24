using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Interior : Function {
    public override void Call() {
        base.Call();
        Call(this);
    }

    void enter() {
        var interior = targetItem().interior;

        if (interior == null) {
            interior = new Interior();
            interior.Genererate(targetItem());
        }

        interior.Enter();
    }

    void describeOut() {
        var cardinal = Coords.GetCardinalFromString(targetItem().GetProperty("direction").value);
        var targetCoords = TileSet.map.playerCoords + (Coords)cardinal;

        var tile = TileSet.map.GetTile(targetCoords);

        if (tile == null) {
            TextManager.write("The view is blocked by some bushes");
        } else {
            TextManager.write("tile_describeExterior", tile);
        }
    }
}
