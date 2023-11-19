using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using UnityEngine;

public class Zombie : Humanoid {
    public override void Move(Coords targetCoords) {

        base.Move(targetCoords);

        _ = TileSet.GetCurrent.GetTile(targetCoords).CreateChildItem(this);
    }

    public void Advance() {
        var targetCoords = coords + (Coords)currentCarnidal;

        if (!CanMoveForward(targetCoords)) {
            Turn();
            return;
        }

        Move(targetCoords);
    }

    public void Turn() {
        currentCarnidal += 2;
        if (currentCarnidal == 0) {
            currentCarnidal = 0;
        }
    }
}
