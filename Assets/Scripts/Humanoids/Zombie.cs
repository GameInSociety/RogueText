using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zombie : Humanoid
{
    public override void Init()
    {
        base.Init();

        /*List<Tile> tiles = TileSet.current.tiles.Values.ToList().FindAll(x =>
        !x.HasProperty("blocking")
        );

        coords = tiles[UnityEngine.Random.Range(0, tiles.Count)].coords;*/

        currentCarnidal = (Cardinal)(Random.Range(0, 4));

        Move(coords);

    }

    public override void Move(Coords targetCoords)
    {

        base.Move(targetCoords);

        TileSet.current.GetTile(targetCoords).AddItem(this);
    }

    public void Advance()
    {
        Coords targetCoords = coords + (Coords)currentCarnidal;

        if (!CanMoveForward(targetCoords))
        {
            Turn();
            return;
        }

        Move(targetCoords);
    }

    public void Turn()
    {
        currentCarnidal += 2;
        if ( currentCarnidal == (Cardinal)0)
        {
            currentCarnidal = 0;
        }
    }
}
