using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGroupDescription {

    public static int surroundTileIndex;

    public static List<TileGroup> tileGroups = new List<TileGroup> ();

    public static Player.Orientation GetFacingWithTile(string str)
    {
        if (str == Tile.current.tileItem.word.text)
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

    public static string GetSurroundingTileDescription()
    {
        // enclosed, so no description of hallway, other rooms etc...
        if (Tile.current.enclosed)
        {
            return "";
        }

        string str = "";

        /// light / night
        if ( TimeManager.GetInstance().currentPartOfDay == TimeManager.PartOfDay.Night)
        {
            if ( Inventory.Instance.GetItem("lampe torche (a)") != null)
            {
                str += "La lampe torche vous éclaire" + "\n";
            }
            else
            {
                str += "Il fait trop sombre, vous ne voyez rien autour de vous";
                return str;
            }
        }

        // get tiles
        GetSurroundingTiles();

        List<string> phrases = new List<string>();

        // get text
        Debug.Log("number of tile group : " + tileGroups.Count);

		foreach (var surroundingTile in tileGroups) {
            string newPhrase = GetSurroundingTileDescription(surroundingTile);
            phrases.Add ( newPhrase );
            Debug.Log("new phrase : " + newPhrase);
        }

        // display text
        foreach (var phrase in phrases)
        {
            str += TextUtils.FirstLetterCap(phrase);

            if (surroundTileIndex < tileGroups.Count - 1)
            {
                str += "\n";
            }
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
        orientations.Add(Player.Orientation.Back);

        foreach (var orientation in orientations)
        {
            Direction dir = Player.Instance.GetDirection(orientation);

            Coords targetCoords = Player.Instance.coords + (Coords)dir;

            Tile targetTile = TileSet.current.GetTile(targetCoords);

            if (targetTile == null)
            {
                Debug.Log("no tile : moving on");
                continue;
            }

            if (targetTile.enclosed)
            {
                Debug.Log("tile " + targetTile.GetName() + " is enclosed");
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

            Debug.Log("adding " + newTileGroup.tile.GetName() + " to tile group");


        }
    }

    public static string GetSurroundingTileDescription(TileGroup surroundingTile)
    {
        Phrase.orientations = surroundingTile.orientations;

        // same tile
        if ( Tile.current.tileItem.SameTypeAs(surroundingTile.tile.tileItem))
        {
            if (Tile.current.tileItem.stackable)
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