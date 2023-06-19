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

    public static Tile Create(Coords _coords, string _name)
    {
        Tile tile = CreateFromDataSpecial(_name) as Tile;
        tile.coords = _coords;
        return tile;
    }


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
            Player.Instance.Orient(Humanoid.CardinalToOrientation(cardinal));
        }

        TryGenerateItems();

        DescribeSelf();

        TryGetSurroundingTiles();

        WriteContainedItems(true);

        CheckHumanoids();


        ConditionManager.GetInstance().WriteDescription();

        // time of day
        TimeManager.Instance.WriteDescription();

        // weather
        TimeManager.Instance.WriteWeatherDescription();

        DisplayDescription.Instance.UseAI();
    }

    public override void WriteContainedItems(bool describeContainers = false)
    {
        foreach (var item in GetAllItems())
        {
            item.visible = false;
        }

        base.WriteContainedItems(describeContainers);
    }


    public void TryGetSurroundingTiles()
    {
        List<Humanoid.Orientation> orientations = new List<Humanoid.Orientation>
        {
            Humanoid.Orientation.front,
            Humanoid.Orientation.right,
            Humanoid.Orientation.left,
            Humanoid.Orientation.back
        };

        foreach (var orientation in orientations)
        {
            /*Cardinal cardinal = Humanoid.OrientationToCardinal(orientation);
            string cardinalItemName = cardinal.ToString();*/

            Tile adjacentTile = GetAdjacent(orientation);

            if (adjacentTile == null)
            {
                continue;
            }

            string orientation_itemName = orientation.ToString();
            string opposite_itemName = Humanoid.GetOpposite(orientation).ToString();

            if (!HasItem(orientation_itemName))
            {
                Item dirItem = AddItem(orientation_itemName);

                // if in interior, create door
                if (adjacentTile.HasProperty("enclosed"))
                {
                    Item tileDoor = dirItem.AddItem("door");
                    tileDoor.CreateProperty("direction / " + orientation_itemName);

                    if (!adjacentTile.HasItem(opposite_itemName))
                    {
                        Item adjDir = adjacentTile.AddItem(opposite_itemName);
                        Item oppositeDoor = adjDir.AddItem("door");
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

    public void DescribeSelf()
    {

        string str = "";

        if (HasInfo("discovered"))
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



    public Tile GetAdjacent(Humanoid.Orientation orientation)
    {
        Cardinal dir = Humanoid.OrientationToCardinal(orientation);

        Coords targetCoords = coords + (Coords)dir;

        return TileSet.current.GetTile(targetCoords);
    }
    public Tile GetAdjacent(Cardinal cardinal)
    {
        Coords targetCoords = coords + (Coords)cardinal;

        return TileSet.current.GetTile(targetCoords);
    }
    #endregion

    public void CheckHumanoids()
    {
        foreach (var item in GetHumanoids())
        {

            Zombie zombie = item as Zombie;
            zombie.Sub();

            TextManager.Return();
            item.WriteDescription();
        }
    }

    #region info
    public static bool SameAsPrevious()
    {
        if (GetPrevious == null)
        {
            return false;
        }

        return GetCurrent.SameTypeAs(GetPrevious);
    }
    public Humanoid.Orientation OrientationToPlayer()
    {
        Coords dir = coords - Player.Instance.coords;

        Cardinal cardinal = (Cardinal)dir;

        Humanoid.Orientation orientation = Humanoid.CardinalToOrientation(cardinal);

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

        tile.DeleteDirectionItems();
    }

    public void UnsubHumanoids()
    {
        foreach (var item in GetHumanoids())
        {
            //Humanoid hum = item as Humanoid;
            Zombie zombie = item as Zombie;
            zombie.Unsub();

        }
    }

    public void DeleteDirectionItems()
    {
        // delete cardinals, to prevent recursive stack overflow
        List<Cardinal> cards = new List<Cardinal>() { Cardinal.east, Cardinal.north, Cardinal.south, Cardinal.west };

        List<Humanoid.Orientation> orientations = new List<Humanoid.Orientation>
        {
            Humanoid.Orientation.front,
            Humanoid.Orientation.right,
            Humanoid.Orientation.left,
            Humanoid.Orientation.back
        };

        foreach (var orientation in orientations)
        {
            string str = orientation.ToString();

            if (HasItem(str))
            {
                Item orientationItem = GetItem(str);

                RemoveItem(orientationItem);

            }
        }
    }

}