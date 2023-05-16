using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;

[System.Serializable]
public class Tile : Item
{
    // location of tile in world
    public Coords coords;

    public bool enclosed = false;

    public static bool itemsChanged = false;

    #region tile description
    // describe from the target cardinal ( example : Player Direction if move, window direction if look at window )
    // pas mal
    public void Describe( )
    {
        DisplayDescription.Instance.Renew();

        if (Player.Instance.coords== coords)
        {
            // don't change orientation
        }
        else
        {
            // change orientation, so the description is correct
            Coords dir = coords - Player.Instance.coords;
            Cardinal cardinal = (Cardinal) dir;
            Player.Instance.Orient(Movable.CardinalToOrientation(cardinal));
        }


        DescribeSelf();

        if (!Player.Instance.CanSee())
        {
            return;
        }

        TryGetSurroundingTiles();

        WriteContainedItems();



    }

    public void TryGetSurroundingTiles()
    {
        if (enclosed)
        {
            return;
        }

        List<Movable.Orientation> orientations = new List<Movable.Orientation>
        {
            Movable.Orientation.Front,
            Movable.Orientation.Right,
            Movable.Orientation.Left
        };

        foreach (var orientation in orientations)
        {
            Cardinal cardinal = Movable.OrientationToCardinal(orientation);
            string cardinalItemName = cardinal.ToString();
            Debug.Log(cardinalItemName);

            Tile targetTile = GetTile(orientation);

            if (targetTile == null)
            {
                continue;
            }

            if (targetTile.enclosed)
            {
                continue;
            }

            if (!HasItem(cardinalItemName))
            {
                CreateInItem(cardinalItemName);

                if (!GetItem(cardinalItemName).HasItem(targetTile))
                {
                    GetItem(cardinalItemName).AddItem(targetTile);
                }
            }
            else
            {
            }
        }

        //SocketManager.Instance.DescribeItems(SurroundingTiles(), null);
    }

    public void DescribeSelf()
    {
        TryGenerateItems();

        if ( SameAsPrevious() && stackable)
        {
            // on the same road, same hallway, don't write anything
            return;
        }

        if (SameAsPrevious())
        {
            TextManager.Write("tile_continue", (Item)this);
        }
        else if (!generatedItems)
        {
            TextManager.Write("tile_discover", (Item)this);
        }
        else
        {
            TextManager.Write("tile_goback", (Item)this);
        }


    }

    

    public Tile GetTile( Movable.Orientation orientation)
    {
        Cardinal dir = Movable.OrientationToCardinal(orientation);

        Coords targetCoords = coords + (Coords)dir;

        return TileSet.current.GetTile(targetCoords);
    }
    public Tile GetAdjacentTile (Cardinal cardinal)
    {
        Coords targetCoords = coords + (Coords)cardinal;

        return TileSet.current.GetTile(targetCoords);
    }
    #endregion

    public override void TryGenerateItems()
    {
        base.TryGenerateItems();

        foreach (var item in DebugManager.Instance.itemsOnTile)
        {
            ItemManager.Instance.CreateInItem(this, item);
        }
    }

    /*public override void WriteContainedItems()
    {
        //base.WriteContainedItemDescription();

        // i put "used" here because otherwise, the items woudl'nt generate
        // because of items I put on load ( doors etc... )
        if (!used)
        {
            TryGenerateItems();
        }

        /*Socket socket = new Socket();
        socket.SetPosition("&on the dog (tile)&");

        SocketManager.Instance.DescribeItems(GetContainedItems, null);
    }*/

    #region info
    public static bool SameAsPrevious()
    {
        if (GetPrevious == null)
        {
            return false;
        }

        return GetCurrent.SameTypeAs(GetPrevious);
    }
    public Movable.Orientation OrientationToPlayer()
    {
        Coords dir = coords - Player.Instance.coords;

        Cardinal cardinal = (Cardinal)dir;

        Movable.Orientation orientation = Movable.CardinalToOrientation(cardinal);

        return orientation;
    }
    #endregion

    /// <summary>
    /// TOOLS
    /// </summary>
    private static Tile _previous;
    public static Tile GetPrevious {
        get
        {
            return _previous;
        }
    }
    public static void SetPrevious(Tile tile)
    {
        _previous = tile;
    }

    private static Tile _current;
    public static Tile GetCurrent {
        get
        {
            return _current;
        }
    }

    public static void SetCurrent(Tile tile)
    {
        DebugManager.Instance.tile = tile;
        _current = tile;
    }

}