using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGroupDescription {

    public static int surroundTileIndex;

    public static List<TileGroup> tileGroups = new List<TileGroup> ();

    public static Player.Orientation GetFacingWithTile(string str)
    {
        if (str == Tile.GetCurrent.tileItem.word.text)
        {
            return Player.Orientation.Current;
        }

        TileGroup surr = tileGroups.Find( x => x.tile.tileItem.word.text.StartsWith(str) );

        if (surr.tile == null)
        {
            return Player.Orientation.None;
        }
        else
        {
            return surr.orientations[0];
        }
    }

    public static void WriteSurroundingTileDescription()
    {
        // enclosed, so no description of hallway, other rooms etc...
        if (Tile.GetCurrent.enclosed)
        {
            return;
        }

        /// light / night
        if (TimeManager.GetInstance().currentPartOfDay == TimeManager.PartOfDay.Night)
        {
            if (Inventory.Instance.HasItem("lampe"))
            {
                Phrase.Write("lamp_on");
            }
            else
            {
                Phrase.Write("lamp_off");
                return;
            }
        }

        Phrase.Write("tile_surrounding_description");
    }

    public static string GetSurroundingTileDescription()
    {
        // get tiles
        GetSurroundingTiles();

        List<string> phrases = new List<string>();

        // get text
        string str = "";

        foreach (var surroundingTile in tileGroups)
        {
            string newPhrase = GetSurroundingTileDescription(surroundingTile);
            str += newPhrase;
        }

        return str;
    }

    public static void GetSurroundingTiles()
    {
        tileGroups.Clear();

        // get description orientations
        List<Player.Orientation> orientations = new List<Player.Orientation>();
        orientations.Add(Player.Orientation.Front);
        orientations.Add(Player.Orientation.Right);
        orientations.Add(Player.Orientation.Left);

        // le gros dilemme : on met derrière ou pas ?
        //orientations.Add(Player.Orientation.Back);

        foreach (var orientation in orientations)
        {
            Direction dir = Player.Instance.GetDirection(orientation);

            Coords targetCoords = Player.Instance.coords + (Coords)dir;

            Tile targetTile = TileSet.current.GetTile(targetCoords);

            if (targetTile == null)
            {
                continue;
            }

            if (targetTile.enclosed)
            {
                continue;
            }

            TileGroup newTileGroup = tileGroups.Find(x => x.tile.type == targetTile.type);


            if (newTileGroup == null)
            {
                newTileGroup = new TileGroup();
                newTileGroup.tile = targetTile;

                newTileGroup.orientations = new List<Player.Orientation>();
                newTileGroup.orientations.Add(orientation);

                tileGroups.Add(newTileGroup);
            }
            else
            {
                newTileGroup.orientations.Add(orientation);
            }
        }
    }

    public static string GetSurroundingTileDescription(TileGroup surroundingTile)
    {
        Player.Instance.currentOrientations = surroundingTile.orientations;

        // same tile
        if ( Tile.GetCurrent.tileItem.SameTypeAs(surroundingTile.tile.tileItem))
        {
            if (Tile.GetCurrent.tileItem.stackable)
            {
                // tu es dans une forêt, la forêt continue
                return Phrase.GetPhrase("surroundingTile_continue", surroundingTile.tile.tileItem);
            }
            else
            {
                // tu es près d'une maison, tu vois une maison que tu connais pas
                return Phrase.GetPhrase("surroundingTile_discover", surroundingTile.tile.tileItem);
            }
        }

        // new tile
        if (Interior.InsideInterior())
        {
            // tu es dans la cuisine, et tu vois LE couloir ( dans un intérieur, les articles définis ont plus de sens )
            if (surroundingTile.tile.tileItem.stackable)
            {
                // tu es dans une forêt, la forêt continue
                return Phrase.GetPhrase("surroundingTile_continue", surroundingTile.tile.tileItem);
            }
            else
            {
                // tu es près d'une maison, tu vois une maison que tu connais pas
                return Phrase.GetPhrase("surroundingTile_visited", surroundingTile.tile.tileItem);
            }
        }
        else
        {
            // ici
            if ( surroundingTile.tile.visited)
            {
                // tu vois es près d'une maison
                return Phrase.GetPhrase("surroundingTile_visited", surroundingTile.tile.tileItem);
            }
            else
            {
                return Phrase.GetPhrase("surroundingTile_discover", surroundingTile.tile.tileItem);
            }
        }


    }
}