using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using System.Net.Configuration;
using UnityEngine.Analytics;

[System.Serializable]
public class Tile : Item
{
    // location of tile in world
    public Coords coords;

    public static bool itemsChanged = false;

    public static Tile Create(Coords _coords, string _name)
    {
        Tile tile = Generate_Special(_name) as Tile;
        tile.coords = _coords;
        return tile; 
    }


    #region tile description
    // describe from the target cardinal ( example : Player Direction if move, window direction if look at window )
    // pas mal
    public void Describe()
    {
        
        if (!FunctionSequence.SequenceFinished)
        {
            FunctionSequence.onFinishSequences += Describe;
            return;
        }


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

        // time of day
        TimeManager.Instance.WriteDescription();

        // weather
        TimeManager.Instance.WriteWeatherDescription();

        //DisplayDescription.Instance.UseAI();

        //Property.DescribeUpdated();
    }

    public override void WriteContainedItems(bool describeContainers = false)
    {
        /*foreach (var item in GetAllItems())
        {
            item.visible = false;
        }*/

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
                continue;

            string opp_orientation = Humanoid.GetOpposite(orientation).ToString();

            // check if the current tile as the orientation item
            // je vais supprimer les "objets directions" ( left , right etc... ) mais leur rajouter des specs 

            if (adjacentTile.HasProperty("enclosed"))
            {
                Item tileDoor = CreateItem("door");
                tileDoor.AddProperty("direction / " + orientation.ToString());
                tileDoor.AddSpec(orientation.ToString());

                if (!adjacentTile.HasItem(opp_orientation))
                {
                    Item oppositeDoor = adjacentTile.CreateItem("door");
                    oppositeDoor.AddProperty("direction / " + opp_orientation);
                    oppositeDoor.AddSpec(opp_orientation);
                    continue;
                }
            }
            else
            {
                if (!Player.Instance.CanSee())
                    return;

                // here maybe put "a path leads to a field"
                // the road continues to a forest small forest.
                // et pas seulement "on the left, a field"

                Item newTile = AddTemporaryItem(adjacentTile);
                newTile.AddSpec(orientation.ToString());
            }

            //SocketManager.Instance.DescribeItems(SurroundingTiles(), null);
        }
    }

    public void DescribeSelf()
    {
        string str = "";

        if (HasInfo("discovered"))
        {
            if (GetPrevious != null && GetPrevious.coords == coords)
                str = "tile_wait";
            else
                str = "tile_goback";
        }
        else
        {
            if (GetPrevious != null && SameTypeAs(GetPrevious) && GetPrevious.coords != coords)
                str = "tile_continue";
            else
                str = "tile_discover";
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

        foreach (var item in GetEnemies())
        {
            TextManager.Write("&a dog& is " + item.GetProperty("steps").GetDescription(), item);
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
        SetPrevious(_current);

        DebugManager.Instance.tile = tile;
        _current = tile;
        _current.AddItem(Player.Instance);
    }

    public static void SetPrevious(Tile tile)
    {
        _previous = tile;


        if (_previous == null)
        {
            return;
        }

        tile.RemoveItem(Player.Instance);

        tile.DeleteTempItems();
    }


    public void DeleteTempItems()
    {

        foreach (var item in GetContainedItems)
        {
            if (item.temporary)
                Debug.Log("removing temp item : " + item.debug_name);
        }

        GetContainedItems.RemoveAll(x => x.temporary);

    }

}