using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Coords
{
    public string GetCode()
    {
        return "x:" + x + ",y:" + y;
    }

    public int x;
    public int y;

    public static Direction GetDirectionFromCoords(Coords c1, Coords c2)
    {
        Coords c = c2 - c1;

        Vector2 v = (Vector2)c;

        Direction direction = Direction.North;

        Direction closestDirection = Direction.North;

        while (direction != Direction.None)
        {
            Coords ce = (Coords)direction;

            float angle = Vector2.Angle(v, (Vector2)ce);

            float closestDirectionAngle = Vector2.Angle(v, (Vector2)((Coords)closestDirection));

            string direction_str = Coords.GetWordsDirection(direction).GetContent("au chien");

            if (angle < closestDirectionAngle)
            {
                closestDirection = direction;
            }

            ++direction;
        }
        return closestDirection;
    }

    public static Coords random
    {
        get
        {
            return new Coords(Random.Range(1, TileSet.current.width - 1), Random.Range(1, TileSet.current.height - 1));
        }
    }

    public Coords(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static string GetOrientationText(List<Player.Orientation> orientations)
    {
        string str = "";

        int i = 0;

        foreach (var facing in orientations)
        {
            string directionWord = Coords.GetOrientationText(facing);

            str += directionWord;

            // avant dernier
            if (i == orientations.Count - 2)
            {
                str += " et ";
            }
            // dernier
            else if (i == orientations.Count - 1)
            {

            }
            // courrant de la phrase
            else
            {
                str += ", ";
            }

            i++;
        }

        if (DebugManager.Instance.colorWords)
        {
            return "<color=yellow>" + str + "</color>";
        }
        else
        {
            return str;
        }
    }

    public static string GetOrientationText(Player.Orientation orientation)
    {
        string[] directionPhrases = new string[8] {
        "devant vous",
        "quelques pas devant vous, vers la droite",
        Random.value < 0.5f ? "à votre droite" : "sur votre droite",
        "derrière vous, à droite",
        "derrière vous",
        "derrière vous, à gauche",
        Random.value < 0.5f ? "à votre gauche" : "sur votre gauche",
        "quelques pas devant vous, à votre gauche"
    };

        return directionPhrases[(int)orientation];
    }

    public static Direction GetDirectionFromString(string str)
    {

        foreach (var item in System.Enum.GetValues(typeof(Direction)))
        {
            if (item.ToString() == str)
            {
                //				Debug.Log ("found direction : " + item);
                return (Direction)item;
            }
        }

        return Direction.None;

    }

    public static void WriteDirectionToNorth()
    {
        string facing = Coords.GetOrientationText(Coords.GetFacing(Player.Instance.direction));
        Phrase.Write("/compas_giveNorth/" + facing);
    }

    public bool OutOfMap()
    {
        return
            x > TileSet.current.width - 2 || x < 1 ||
            y > TileSet.current.height - 2 || y < 1;
    }

    public static Coords Zero
    {
        get
        {
            return new Coords(0, 0);
        }
    }
    // overrides
    // == !=
    public static bool operator ==(Coords c1, Coords c2)
    {
        return c1.x == c2.x && c1.y == c2.y;
    }
    public static bool operator !=(Coords c1, Coords c2)
    {
        return !(c1 == c2);
    }

    // < >
    public static bool operator <(Coords c1, Coords c2)
    {
        return c1.x < c2.x && c1.y < c2.y;
    }
    public static bool operator >(Coords c1, Coords c2)
    {
        return c1.x > c2.x && c1.y > c2.y;
    }
    public static bool operator <(Coords c1, int i)
    {
        return c1.x < i || c1.y < i;
    }
    public static bool operator >(Coords c1, int i)
    {
        return c1.x > i || c1.y > i;
    }

    // >= <=
    public static bool operator >=(Coords c1, Coords c2)
    {
        return c1.x >= c2.x && c1.y >= c2.y;
    }
    public static bool operator <=(Coords c1, Coords c2)
    {
        return c1.x <= c2.x && c1.y <= c2.y;
    }
    public static bool operator >=(Coords c1, int i)
    {
        return c1.x >= i || c1.y >= i;
    }
    public static bool operator <=(Coords c1, int i)
    {
        return c1.x <= i || c1.y <= i;
    }

    // + -
    public static Coords operator +(Coords c1, Coords c2)
    {
        return new Coords(c1.x + c2.x, c1.y + c2.y);
    }
    public static Coords operator -(Coords c1, Coords c2)
    {
        return new Coords(c1.x - c2.x, c1.y - c2.y);
    }
    public static Coords operator +(Coords c1, int i)
    {
        return new Coords(c1.x + i, c1.y + i);
    }
    public static Coords operator -(Coords c1, int i)
    {
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
    public static explicit operator Coords(Direction dir)  // explicit byte to digit conversion operator
    {
        switch (dir)
        {
            case Direction.North:
                return new Coords(0, 1);
            case Direction.NorthEast:
                return new Coords(1, 1);
            case Direction.East:
                return new Coords(1, 0);
            case Direction.SouthEast:
                return new Coords(1, -1);
            case Direction.South:
                return new Coords(0, -1);
            case Direction.SouthWest:
                return new Coords(-1, -1);
            case Direction.West:
                return new Coords(-1, 0);
            case Direction.NorthWest:
                return new Coords(-1, 1);
            case Direction.None:
                return new Coords(0, 0);
        }

        return new Coords();
    }

    public static Word GetWordsDirection(Direction direction)
    {
        return Item.dataItems[(int)direction + 1].word;
    }
    public static Item GetDirectionItem(Direction direction)
    {
        return Item.dataItems[(int)direction + 1];
    }

    // string
    public override string ToString()
    {
        return "X : " + x + " / Y : " + y;
    }

    public static Player.Orientation GetFacing(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return Player.Orientation.Front;
            case Direction.NorthEast:
                break;
            case Direction.East:
                return Player.Orientation.Left;
            case Direction.SouthEast:
                break;
            case Direction.South:
                return Player.Orientation.Back;
            case Direction.SouthWest:
                break;
            case Direction.West:
                return Player.Orientation.Right;
            case Direction.NorthWest:
                break;
            case Direction.None:
                break;
        }

        return Player.Orientation.None;

    }

    public void Turn()
    {
         if ( x == 0 && y == 1 || x == 0 && y == -1)
        {
            if ( Random.value < 0.5f)
            {
                x = 1;
                y = 0;
            }
            else
            {
                x = -1;
                y = 0;
            }
        }
        else if (x == 1 && y == 0 || x == -1 && y == 0)
        {
            if (Random.value < 0.5f)
            {
                x = 0;
                y = 1;
            }
            else
            {
                x = 0;
                y = -1;
            }
        }

    }

    public static Coords GetRandom4()
    {
        int randomDir = Random.Range(0, 4);

        switch (randomDir)
        {
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

    public static Direction GetRelativeDirection(Direction direction, Player.Orientation facing)
    {
        int a = (int)direction + (int)facing;
        if (a >= 8)
        {
            a -= 8;
        }

        //		Debug.Log ( "player is turned " + direction + ", so the returned dir is " + (Direction)a );

        return (Direction)a;
    }
}

