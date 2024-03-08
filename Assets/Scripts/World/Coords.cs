using UnityEngine;

[System.Serializable]
public struct Coords {
    public int x;
    public int y;

    public static Coords TextToCoords(string text, int tilesetId = 0) {
        var split = text.Split('/');
        int x = 0;
        if (split[0] == "?") {
            Debug.Log($"getting random coords {text} in tile set : {tilesetId}");
            var randomCoords = TileSet.GetTileSet(tilesetId).GetRandomCoords();
            Debug.Log($"random coords : {randomCoords}");
            return randomCoords;
        }

        return new Coords(int.Parse(split[0]), int.Parse(split[1]));
    }
    public static Coords PropToCoords(Property prop, int tilesetId = 0) {
        return TextToCoords(prop.GetTextValue(), tilesetId);
    }
    public static string CoordsToText(Coords coords) {
        return $"{coords.x}/{coords.y}";
    }
    public static Property CoordsToProp(Coords c) {
        var prop = new Property();
        prop.name = "coords";
        prop.AddPart("value", CoordsToText(c));
        return prop;
    }

    public Coords(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public static Coords zero => new Coords(0, 0);
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

