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

        SocketManager.Instance.DescribeItems(Player.Instance.SurroundingTiles(), null);
    }

    public struct SurroundingTile
    {
        public List<Player.Orientation> orientations;
        public Tile tile;
    }

    private static string GetSurroundingTilesDescription()
    {
        // à conserver parce que il y a des trucs intéressants ( je crois )
        return "VIELLE MANIERE DE GERER LA DESCRIPTION SANS LES NOUVEAUX SOCKETS";
        // get tiles
        List<string> phrases = new List<string>();

        // get text
        string str = "";

        List<SurroundingTile> surroundingTiles= new List<SurroundingTile>();

        /*foreach (var tile in Player.Instance.SurroundingTiles())
        {
            SurroundingTile surroundingTile = surroundingTiles.Find(x => x.tile.dataIndex == tile.dataIndex);
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

            TextManager.SetOverrideOrientation(surroundingTile.orientations);

            Tile tile = surroundingTile.tile;

            str += GetTileText();

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

        return str;*/
    }

    string GetTileText(Item targetTile)
    {
        if (Tile.GetCurrent.SameTypeAs(targetTile))
        {
            if (Tile.GetCurrent.stackable)
            {
                // tu es dans une forêt, la forêt continue
                return TextManager.GetPhrase("surroundingTile_continue", targetTile);
            }
            else
            {
                if (Interior.InsideInterior())
                {

                    // tu es dans la cuisine, et tu vois LE couloir ( dans un intérieur, les articles définis ont plus de sens )
                    return TextManager.GetPhrase("surroundingTile_visited", targetTile);
                }
                else
                {
                    // tu es près d'une maison, tu vois une maison que tu connais pas
                    return TextManager.GetPhrase("surroundingTile_discover", targetTile);
                }
            }
        }
        else
        {
            // ici
            if ( ((Tile)targetTile).visited )
            {
                // tu vois es près d'une maison
                return TextManager.GetPhrase("surroundingTile_visited", targetTile);
            }
            else
            {
                return TextManager.GetPhrase("surroundingTile_discover", targetTile);
            }
        }
    }
    
}