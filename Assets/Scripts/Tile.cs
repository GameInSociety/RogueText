using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;

[System.Serializable]
public class Tile : Item
{
    // location of tile in world
    public Coords coords;

    public static bool itemsChanged = false;

    public bool saidTheres = false;

    #region tile description
    // describe from the target cardinal ( example : Player Direction if move, window direction if look at window )
    // pas mal
    public void Describe()
    {
        CoroutineManager.Instance.onWait += DescribeDelay;
        CoroutineManager.Instance.Wait();
    }

    void DescribeDelay()
    {
        CoroutineManager.Instance.onWait -= DescribeDelay;

        //Debug.Log("[DESCRIBING TILE]");

        SetPrevious(GetCurrent);
        SetCurrent(this);

        DisplayDescription.Instance.Renew();

        if (Player.Instance.coords == coords)
        {
            // don't change orientation
        }
        else
        {
            // change orientation, so the description is correct
            Coords dir = coords - Player.Instance.coords;
            Cardinal cardinal = (Cardinal)dir;
            Player.Instance.Orient(Movable.CardinalToOrientation(cardinal));
        }

        TryGenerateItems();

        DescribeSelf();

        TryGetSurroundingTiles();

        WriteContainedItems();


        ConditionManager.GetInstance().WriteDescription();

        // time of day
        TimeManager.Instance.WriteDescription();

        // weather
        TimeManager.Instance.WriteWeatherDescription();

        DisplayDescription.Instance.UseAI();
    }


    public void TryGetSurroundingTiles()
    {
        List<Movable.Orientation> orientations = new List<Movable.Orientation>
        {
            Movable.Orientation.front,
            Movable.Orientation.right,
            Movable.Orientation.left,
            Movable.Orientation.back
        };

        foreach (var orientation in orientations)
        {
            /*Cardinal cardinal = Movable.OrientationToCardinal(orientation);
            string cardinalItemName = cardinal.ToString();*/

            Tile adjacentTile = GetAdjacent(orientation);

            if (adjacentTile == null)
            {
                continue;
            }



            string orientation_itemName = orientation.ToString();
            string opposite_itemName = Movable.GetOpposite(orientation).ToString();

            if (!HasItem(orientation_itemName))
            {
                Item dirItem = CreateInItem(orientation_itemName);

                // if in interior, create door
                if (adjacentTile.HasProperty("enclosed"))
                {
                    Item tileDoor = dirItem.CreateInItem("door");
                    tileDoor.CreateProperty("direction / " + orientation_itemName);

                    if (!adjacentTile.HasItem(opposite_itemName))
                    {
                        Item adjDir = adjacentTile.CreateInItem(opposite_itemName);
                        Item oppositeDoor = adjDir.CreateInItem("door");
                        oppositeDoor.CreateProperty("direction / " + opposite_itemName);
                        continue;
                    }
                }
                else
                {
                    if (!Player.Instance.CanSee())
                    {
                        return;
                    }

                    if (GetItem(orientation_itemName) == null)
                    {
                        Debug.LogError("no " + orientation_itemName + " in " + debug_name);
                    }

                    if (!GetItem(orientation_itemName).HasItem(adjacentTile))
                    {
                        GetItem(orientation_itemName).AddItem(adjacentTile);
                    }
                }

            }
        }

        //SocketManager.Instance.DescribeItems(SurroundingTiles(), null);
    }

    public override void WriteContainedItems()
    {
        saidTheres = false;
        base.WriteContainedItems();
    }
    public void DescribeSelf()
    {

        string str = "";

        if (info.discovered)
        {
            if (GetPrevious != null && GetPrevious.coords == coords)
            {
                str = "tile_wait";
            }
            else
            {
                str = "tile_goback";
            }
        }
        else
        {
            if (SameTypeAs(GetPrevious))
            {
                str = "tile_continue";
            }
            else
            {
                str = "tile_discover";
            }
        }

        TextManager.Write(str, (Item)this);
    }



    public Tile GetAdjacent(Movable.Orientation orientation)
    {
        Cardinal dir = Movable.OrientationToCardinal(orientation);

        Coords targetCoords = coords + (Coords)dir;

        return TileSet.current.GetTile(targetCoords);
    }
    public Tile GetAdjacent(Cardinal cardinal)
    {
        Coords targetCoords = coords + (Coords)cardinal;

        return TileSet.current.GetTile(targetCoords);
    }
    #endregion

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
    public static Tile GetPrevious
    {
        get
        {
            return _previous;
        }
    }

    private static Tile _current;
    public static Tile GetCurrent
    {
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

    public static void SetPrevious(Tile tile)
    {
        _previous = tile;

        if (_previous == null)
        {
            return;
        }

        // delete cardinals, to prevent recursive stack overflow
        List<Cardinal> cards = new List<Cardinal>() { Cardinal.east, Cardinal.north, Cardinal.south, Cardinal.west };

        List<Movable.Orientation> orientations = new List<Movable.Orientation>
        {
            Movable.Orientation.front,
            Movable.Orientation.right,
            Movable.Orientation.left,
            Movable.Orientation.back
        };

        foreach (var orientation in orientations)
        {
            string str = orientation.ToString();

            if (tile.HasItem(str))
            {
                Item orientationItem = tile.GetItem(str);

                tile.RemoveItem(orientationItem);

            }
        }
    }

}