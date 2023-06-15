using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

public class Humanoid : Item
{
    public Cardinal previousCardinal;
    public Cardinal currentCarnidal;

    // STATES
    public int health = 0;
    public int maxHealth = 10;

    public Coords prevCoords = new Coords(-1, -1);
    public Coords coords = new Coords(-1, -1);
    public Coords direction = new Coords(-1, -1);

    public class Body
    {

        public void Init()
        {
            foreach (Part part in Enum.GetValues(typeof(Part)))
            {
                string itemName = TextManager.ToLowercaseNamingConvention(part.ToString(), true);
                Item item = ItemManager.Instance.CreateFromData(itemName);
                parts.Add(item);
            }
        }

        public enum Part
        {
            Head,
            Neck,
            Chest,
            LeftArm,
            RightArm,
            LeftHand,
            RightHand,
            LeftLeg,
            RightLeg,
            LeftFoot,
            RightFoot,
        }

        private List<Item> parts = new List<Item>();

        public Item GetPart(Part part)
        {
            return parts[(int)part];
        }

        public List<Item> GetParts()
        {
            return parts;
        }
    }

    public class Equipment
    {
        public void Init()
        {
            foreach (Socket socket in Enum.GetValues(typeof(Socket)))
            {
                string itemName = TextManager.ToLowercaseNamingConvention(socket.ToString(), true);
                Item item = ItemManager.Instance.CreateFromData(itemName);
                Wearable wearable = new Wearable();
                wearable.socket = socket;
                wearables.Add(wearable);
            }


        }

        public Item GetItem(Socket socket, int layer)
        {
            return Get(socket).items.Find(x => x.GetProperty(socket.ToString()).GetInt() == layer);
        }

        public bool HasItem(Socket socket, int layer)
        {
            return GetItem(socket, layer) != null;
        }

        public void RemoveItem(Socket socket, int layer)
        {
            Get(socket).items.Remove(GetItem(socket, layer));
        }

        public Wearable Get(Socket socket)
        {
            return wearables[(int)socket];
        }

        public void Equip(Socket socket , Item item )
        {
            int layer = item.GetProperty(socket.ToString()).GetInt();

            Item _item = GetItem(socket, layer);
            if ( _item != null)
            {
                TextManager.Write("You already have &the dog& equiped on the " + socket.ToString());
                return;
            }

            Get(socket).items.Add(item);
        }

        public void Unequip(Socket socket, Item item)
        {
            int layer = item.GetProperty(socket.ToString()).GetInt();

            if (!HasItem(socket, layer))
            {
                TextManager.Write("You don't have anything on the " + socket.ToString());
                return;
            }

            RemoveItem(socket, layer);
        }

        public List<Wearable> wearables = new List<Wearable>();

        public class Wearable
        {
            public Socket socket;
            public List<Item> items;
            public bool HasItem(Item item)
            {
                return items.Contains(item);
            }

        }
        public enum Socket
        {
            Head,
            Neck,
            Chest,
            Legs,
            Hands,
            Feet,
        }


    }

    public virtual void Init()
    {
        Body body = new Body();
        body.Init();
        Equipment equipment = new Equipment();
        equipment.Init();

    }

    public bool CanMoveForward(Coords c)
    {
        Tile targetTile = TileSet.current.GetTile(c);

        if (targetTile == null)
        {
            return false;
        }

        if (targetTile.HasProperty("blocking"))
        {
            return false;
        }

        return true;
    }

    public void Move(Orientation orientation)
    {
        Move(OrientationToCardinal( orientation));
    }
    public void Move(Cardinal targetCardinal)
    {
        Coords targetCoords = coords + (Coords)targetCardinal;
        Move(targetCoords);
    }

    public virtual void Move(Coords targetCoords)
    {

        // change current coords
        prevCoords = coords;

        coords = targetCoords;

        direction = coords - prevCoords;

        // set new direction
        currentCarnidal = (Cardinal)direction;
    }



    public virtual void Orient(Orientation orientation)
    {
        SetDirection(OrientationToCardinal(orientation));
    }
    
    public void SetDirection(Cardinal cardinal)
    {
        previousCardinal = currentCarnidal;
        currentCarnidal = cardinal;
    }

    public static Cardinal OrientationToCardinal( Orientation orientation)
    {

        int a = (int)Player.Instance.currentCarnidal + (int)orientation;
        if (a >= 8)
        {
            a -= 8;
        }

        return (Cardinal)a;
    }

    public static Orientation CardinalToOrientation(Cardinal cardinal)
    {

        int a = (int)cardinal - (int)Player.Instance.currentCarnidal;
        if (a < 0)
        {
            a += 8;
        }

        return (Orientation)a;
    }

    public static Orientation GetOpposite(Orientation orientation)
    {
        switch (orientation)
        {
            case Orientation.front:
                return Orientation.back;
            case Orientation.right:
                return Orientation.left;
            case Orientation.back:
                return Orientation.front;
            case Orientation.left:
                return Orientation.right;
            default:
                break;
        }

        Debug.LogError("couldn't find the opposite orientation of : " + orientation);
        return Orientation.None;
    }

    public enum Orientation
    {
        front,
        FrontRight,
        right,
        BackRight,
        back,
        BackLeft,
        left,
        FrontLeft,

        None,

        Current,
    }
}
