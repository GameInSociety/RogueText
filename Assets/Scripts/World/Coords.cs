using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public struct Coords {
    public int x;
    public int y;

    public Coords(int x, int y) {
        this.x = x;
        this.y = y;
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


    // string
    public override string ToString() {
        return "X : " + x + " / Y : " + y;
    }
}

