using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileSet {

    public static int ids = 0;
    public int id = 0;

    public static TileSet current;
    public static TileSet map;

    public Coords coords;
    public Coords playerCoords = Coords.Zero;

    public Dictionary<Coords, Tile> tiles = new Dictionary<Coords, Tile> ();
    public int width;
    public int height;

    public Coords Center
    {
        get
        {
            return new Coords( (int)((float)width / 2f) , (int)((float)height /2f) ); 
        }
    }

    public void Init()
    {
        width   = MapTexture.Instance.mainMap_Texture.width;
        height  = MapTexture.Instance.mainMap_Texture.height;
    }

	public void Add (Coords c, Tile newTile)
	{
		tiles.Add (c, newTile);
	}

    public static void SetCurrent (TileSet _tileSet)
    {
        current = _tileSet;
    }

    public Tile GetTile(Coords coords)
    {
        if (tiles.ContainsKey(coords) == false)
        {
            return null;
        }

        return tiles[coords];
    }

    public void RemoveAt (Coords c)
    {
        tiles.Remove(c);
    }

}