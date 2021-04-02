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
        string str = "";

        /// light / night
        if ( TimeManager.Instance.currentPartOfDay == TimeManager.PartOfDay.Night)
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
		foreach (var surroundingTile in tileGroups) {
            string newPhrase = GetSurroundingTileDescription(surroundingTile);
            phrases.Add ( newPhrase );
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

            Tile tile = TileSet.current.GetTile(targetCoords);

            if (tile == null)
            {
                continue;
            }

            if (tile.locked)
            {
                Debug.Log("la tile " + tile.type + " est fermée donc on l'affiche pas ");
                continue;
            }

            TileGroup newTileGroup = tileGroups.Find(x => x.tile.type == tile.type);

            if (newTileGroup == null)
            {
                newTileGroup = new TileGroup();
                newTileGroup.tile = tile;

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
        Phrase.item = surroundingTile.tile.tileItem;
        Phrase.orientations = surroundingTile.orientations;

        // same tile
        if ( Tile.current.tileItem.SameTypeAs(surroundingTile.tile.tileItem))
        {
            if (Tile.current.tileItem.stackable)
            {
                // tu es dans une forêt, la forêt continue
                return Phrase.GetPhrase("surroundingTile_continue");
            }
            else
            {
                // tu es près d'une maison, tu vois une maison que tu connais pas
                return Phrase.GetPhrase("surroundingTile_discover");
            }
        }

        // new tile
        if (Interior.InsideInterior())
        {
            // tu es dans la cuisine, et tu vois LE couloir ( dans un intérieur, les articles définis ont plus de sens )
            return Phrase.GetPhrase("surroundingTile_continue");
        }
        else
        {
            // ici
            if ( surroundingTile.tile.visited)
            {
                // tu vois es près d'une maison
                return Phrase.GetPhrase("surroundingTile_visited");
            }
            else
            {
                return Phrase.GetPhrase("surroundingTile_discover");
            }
        }

    }
}