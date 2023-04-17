using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurroundingTileManager {

    public static int surroundTileIndex;

    public static List<SurroundingTileGroup> tileGroups = new List<SurroundingTileGroup> ();
    public static SurroundingTileGroup currentSurroundingTile;

    public static Player.Orientation GetOrientationWithTile(string str)
    {
        if (str == Tile.GetCurrent.tileItem.word.text) 
        {
            return Player.Orientation.Current;
        }

        SurroundingTileGroup surr = tileGroups.Find( x => x.tile.tileItem.word.text.StartsWith(str) );

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
                PhraseKey.Write("lamp_on");
            }
            else
            {
                PhraseKey.Write("lamp_off");
                return;
            }
        }

        PhraseKey.Write("tile_surrounding_description");
    }

    public static string GetSurroundingTilesDescription()
    {
        // get tiles
        GetSurroundingTiles();

        List<string> phrases = new List<string>();

        // get text
        string str = "";

        for (int i = 0; i < tileGroups.Count; i++)
        {
            SurroundingTileGroup surroundingTile = tileGroups[i];

            string newPhrase = GetSurroundingTileDescription(surroundingTile);

            if (i == tileGroups.Count - 2)
            {
                str += " and ";
            }
            // dernier
            else if (i == tileGroups.Count - 1)
            {
                str += ", ";
            }
            // courrant de la phrase
            else
            {
                str += ", ";
            }

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
            Cardinal dir = Player.Instance.GetCardinal(orientation);

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

            SurroundingTileGroup newSurroundingTiles = tileGroups.Find(x => x.tile.type == targetTile.type);

            if (newSurroundingTiles == null)
            {
                newSurroundingTiles = new SurroundingTileGroup();
                newSurroundingTiles.tile = targetTile;

                newSurroundingTiles.orientations = new List<Player.Orientation>();
                newSurroundingTiles.orientations.Add(orientation);

                tileGroups.Add(newSurroundingTiles);
            }
            else
            {
                newSurroundingTiles.orientations.Add(orientation);
            }
        }
    }
    
    public static string GetSurroundingTileDescription(SurroundingTileGroup surroundingTile)
    {
        currentSurroundingTile = surroundingTile;

        // same tile
        if ( Tile.GetCurrent.tileItem.SameTypeAs(surroundingTile.tile.tileItem))
        {
            if (Tile.GetCurrent.tileItem.stackable)
            {
                // tu es dans une forêt, la forêt continue
                return PhraseKey.GetPhrase("surroundingTile_continue", surroundingTile.tile.tileItem);
            }
            else
            {
                // tu es près d'une maison, tu vois une maison que tu connais pas
                return PhraseKey.GetPhrase("surroundingTile_discover", surroundingTile.tile.tileItem);
            }
        }

        // new tile
        if (Interior.InsideInterior())
        {
            // tu es dans la cuisine, et tu vois LE couloir ( dans un intérieur, les articles définis ont plus de sens )
            if (surroundingTile.tile.tileItem.stackable)
            {
                // tu es dans une forêt, la forêt continue
                return PhraseKey.GetPhrase("surroundingTile_continue", surroundingTile.tile.tileItem);
            }
            else
            {
                // tu es près d'une maison, tu vois une maison que tu connais pas
                return PhraseKey.GetPhrase("surroundingTile_visited", surroundingTile.tile.tileItem);
            }
        }
        else
        {
            // ici
            if ( surroundingTile.tile.visited)
            {
                // tu vois es près d'une maison
                return PhraseKey.GetPhrase("surroundingTile_visited", surroundingTile.tile.tileItem);
            }
            else
            {
                return PhraseKey.GetPhrase("surroundingTile_discover", surroundingTile.tile.tileItem);
            }
        }


    }
}