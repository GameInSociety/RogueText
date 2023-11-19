using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public struct Coords {
    public string GetCode() {
        return "x:" + x + ",y:" + y;
    }

    public int x;
    public int y;

    public static Cardinal GetCardinalFromCoords(Coords coords) {
        var v = (Vector2)coords;

        var direction = Cardinal.north;

        var closestDirection = Cardinal.north;

        while (direction != Cardinal.None) {
            var ce = (Coords)direction;

            var angle = Vector2.Angle(v, (Vector2)ce);

            var closestDirectionAngle = Vector2.Angle(v, (Vector2)(Coords)closestDirection);

            if (angle < closestDirectionAngle) {
                closestDirection = direction;
            }

            ++direction;
        }
        return closestDirection;
    }

    public static Coords random => new Coords(Random.Range(1, TileSet.GetCurrent.width - 1), Random.Range(1, TileSet.GetCurrent.height - 1));

    public Coords(int x, int y) {
        this.x = x;
        this.y = y;
    }
    public static string GetOrientationWord(Humanoid.Orientation orientation) {
        switch (orientation) {
            case Humanoid.Orientation.front:
                return "front";
            case Humanoid.Orientation.right:
                return "right";
            case Humanoid.Orientation.back:
                return "back";
            case Humanoid.Orientation.left:
                return "left";
            case Humanoid.Orientation.None:
                return "eeeeeeeeeh";
            case Humanoid.Orientation.Current:
                return "here";
            default:
                break;
        }

        return "woops orientation";
    }

    public static Cardinal GetCardinalFromString(string str) {
        foreach (var item in System.Enum.GetValues(typeof(Cardinal))) {
            if (item.ToString() == str) {
                return (Cardinal)item;
            }
        }

        Debug.LogError("no cardinal found in " + str);

        return Cardinal.None;

    }

    public static Humanoid.Orientation GetOrientationFromString(string str) {
        foreach (var item in System.Enum.GetValues(typeof(Humanoid.Orientation))) {
            if (item.ToString() == str) {
                return (Humanoid.Orientation)item;
            }
        }


        Debug.LogError("no orientation found in " + str);

        return Humanoid.Orientation.None;

    }

    public bool OutOfMap() {
        return
            x > TileSet.GetCurrent.width - 2 || x < 1 ||
            y > TileSet.GetCurrent.height - 2 || y < 1;
    }

    public static Coords Zero => new Coords(0, 0);
    // overrides
    // == !=
    public static bool operator ==(Coords c1, Coords c2) {
        return c1.x == c2.x && c1.y == c2.y;
    }
    public static bool operator !=(Coords c1, Coords c2) {
        return !(c1 == c2);
    }

    // < >
    public static bool operator <(Coords c1, Coords c2) {
        return c1.x < c2.x && c1.y < c2.y;
    }
    public static bool operator >(Coords c1, Coords c2) {
        return c1.x > c2.x && c1.y > c2.y;
    }
    public static bool operator <(Coords c1, int i) {
        return c1.x < i || c1.y < i;
    }
    public static bool operator >(Coords c1, int i) {
        return c1.x > i || c1.y > i;
    }

    // >= <=
    public static bool operator >=(Coords c1, Coords c2) {
        return c1.x >= c2.x && c1.y >= c2.y;
    }
    public static bool operator <=(Coords c1, Coords c2) {
        return c1.x <= c2.x && c1.y <= c2.y;
    }
    public static bool operator >=(Coords c1, int i) {
        return c1.x >= i || c1.y >= i;
    }
    public static bool operator <=(Coords c1, int i) {
        return c1.x <= i || c1.y <= i;
    }

    // + -
    public static Coords operator +(Coords c1, Coords c2) {
        return new Coords(c1.x + c2.x, c1.y + c2.y);
    }
    public static Coords operator -(Coords c1, Coords c2) {
        return new Coords(c1.x - c2.x, c1.y - c2.y);
    }
    public static Coords operator +(Coords c1, int i) {
        return new Coords(c1.x + i, c1.y + i);
    }
    public static Coords operator -(Coords c1, int i) {
        return new Coords(c1.x - i, c1.y - i);
    }

    // vector2 cast

    public static explicit operator Coords(Vector2 v)  // explicit byte to digit conversion operator
    {
        return new Coords((int)v.x, (int)v.y);
    }
    public static explicit operator Vector2(Coords c)  // explicit byte to digit conversion operator
    {
        return new Vector2(c.x, c.y);
    }
    //
    //		// direction cast
    //	public static explicit operator Direction(Coords c)  // explicit byte to digit conversion operator
    //	{
    //		return new Direction (c.x, c.y);
    //	}
    public static explicit operator Coords(Cardinal dir)  // explicit byte to digit conversion operator
    {
        switch (dir) {
            case Cardinal.north:
                return new Coords(0, 1);
            case Cardinal.NorthEast:
                return new Coords(1, 1);
            case Cardinal.east:
                return new Coords(1, 0);
            case Cardinal.SouthEast:
                return new Coords(1, -1);
            case Cardinal.south:
                return new Coords(0, -1);
            case Cardinal.SouthWest:
                return new Coords(-1, -1);
            case Cardinal.west:
                return new Coords(-1, 0);
            case Cardinal.NorthWest:
                return new Coords(-1, 1);
            case Cardinal.None:
                return new Coords(0, 0);
        }

        return new Coords();
    }

    public static explicit operator Cardinal(Coords c) {
        if (c.x < 0) {
            switch (c.y) {
                case -1:
                    return Cardinal.SouthWest;
                case 0:
                    return Cardinal.west;
                case 1:
                    return Cardinal.NorthWest;
            }
        } else if (c.x > 0) {
            switch (c.y) {
                case -1:
                    return Cardinal.SouthEast; ;
                case 0:
                    return Cardinal.east;
                case 1:
                    return Cardinal.NorthEast;
            }
        } else {
            switch (c.y) {
                case -1:
                    return Cardinal.south;
                case 0:
                    return Cardinal.None;
                case 1:
                    return Cardinal.None;
            }
        }

        return Cardinal.None;
    }

    // string
    public override string ToString() {
        return "X : " + x + " / Y : " + y;
    }

    public static Humanoid.Orientation GetOrientationFromNorth(Cardinal direction) {
        switch (direction) {
            case Cardinal.north:
                return Humanoid.Orientation.front;
            case Cardinal.NorthEast:
                break;
            case Cardinal.east:
                return Humanoid.Orientation.left;
            case Cardinal.SouthEast:
                break;
            case Cardinal.south:
                return Humanoid.Orientation.back;
            case Cardinal.SouthWest:
                break;
            case Cardinal.west:
                return Humanoid.Orientation.right;
            case Cardinal.NorthWest:
                break;
            case Cardinal.None:
                break;
        }

        return Humanoid.Orientation.None;

    }

    public void Turn() {
        if ((x == 0 && y == 1) || (x == 0 && y == -1)) {
            if (Random.value < 0.5f) {
                x = 1;
                y = 0;
            } else {
                x = -1;
                y = 0;
            }
        } else if ((x == 1 && y == 0) || (x == -1 && y == 0)) {
            if (Random.value < 0.5f) {
                x = 0;
                y = 1;
            } else {
                x = 0;
                y = -1;
            }
        }

    }

    public static Coords GetRandom4() {
        var randomDir = Random.Range(0, 4);

        switch (randomDir) {
            case 0:
                return new Coords(0, 1);

            case 1:
                return new Coords(1, 0);

            case 2:
                return new Coords(0, -1);

            case 3:
                return new Coords(-1, 0);

            default:
                return new Coords(0, 0);
        }


    }

    public static Cardinal GetRelativeDirection(Cardinal direction, Humanoid.Orientation facing) {
        var a = (int)direction + (int)facing;
        if (a >= 8) {
            a -= 8;
        }

        //		Debug.Log ( "player is turned " + direction + ", so the returned dir is " + (Direction)a );

        return (Cardinal)a;
    }
}

