using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SurroundingTileManager {

    public static int surroundTileIndex;

    public static void WriteSurroundingTileDescription()
    {
        // enclosed, so no description of hallway, other rooms etc...
        if (Tile.GetCurrent.enclosed)
        {
            return;
        }


        /// light / night
        if (!Player.Instance.CanSee())
        {
            return;
        }

        PhraseKey.WritePhrase(GetSurroundingTilesDescription());
    }

    public struct SurroundingTile
    {
        public List<Player.Orientation> orientations;
        public Tile tile;
    }

    public static string GetSurroundingTilesDescription()
    {
        // get tiles
        List<string> phrases = new List<string>();

        // get text
        string str = "";

        List<SurroundingTile> surroundingTiles= new List<SurroundingTile>();

        foreach (var tile in Player.Instance.SurroundingTiles())
        {
            SurroundingTile surroundingTile = surroundingTiles.Find(x => x.tile.tileItem.index == tile.tileItem.index);
            if ( surroundingTile.tile == null )
            {
                surroundingTile = new SurroundingTile();
                surroundingTile.tile = tile;
                surroundingTile.orientations = new List<Player.Orientation> { tile.OrientationToPlayer() };
                surroundingTiles.Add(surroundingTile);
            }
            else
            {
                surroundingTile.orientations.Add(tile.OrientationToPlayer());
            }
        }

        int index = 0;
        foreach (var surroundingTile in surroundingTiles)
        {
            Debug.Log("surrounding tile : " + surroundingTile.tile.tileItem.debug_name);

            PhraseKey.SetOverrideOrientation(surroundingTile.orientations);

            Tile tile = surroundingTile.tile;

            if (Tile.GetCurrent.tileItem.SameTypeAs(surroundingTile.tile.tileItem))
            {
                if (Tile.GetCurrent.tileItem.stackable)
                {
                    // tu es dans une forêt, la forêt continue
                    str += PhraseKey.GetPhrase("surroundingTile_continue", tile.tileItem);
                }
                else
                {
                    // tu es près d'une maison, tu vois une maison que tu connais pas
                    str += PhraseKey.GetPhrase("surroundingTile_discover", tile.tileItem);
                }
            } else if (Interior.InsideInterior())
            {
                // tu es dans la cuisine, et tu vois LE couloir ( dans un intérieur, les articles définis ont plus de sens )
                if (tile.tileItem.stackable)
                {
                    // tu es dans une forêt, la forêt continue
                    str += PhraseKey.GetPhrase("surroundingTile_continue", tile.tileItem);
                }
                else
                {
                    // tu es près d'une maison, tu vois une maison que tu connais pas
                    str += PhraseKey.GetPhrase("surroundingTile_visited", tile.tileItem);
                }
            }
            else
            {
                // ici
                if (tile.visited)
                {
                    // tu vois es près d'une maison
                    str += PhraseKey.GetPhrase("surroundingTile_visited", tile.tileItem);
                }
                else
                {
                    str += PhraseKey.GetPhrase("surroundingTile_discover", tile.tileItem);
                }
            }

            if (index == surroundingTiles.Count - 2)
            {
                str += " and ";
            }
            // dernier
            else if (index == surroundingTiles.Count - 1)
            {
                str += "";
            }
            // courrant de la phrase
            else
            {
                str += ", ";
            }

            ++index;
        }

        return str;
    }
    
}