using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

[System.Serializable]
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

    public Body body;
    public Condition condition;


    public override void Init()
    {
        base.Init();
        body = new Body();
        body.Init();
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
