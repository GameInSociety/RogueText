using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{
    public static bool opened = false;
    public static Item CurrentItem;

    /// <summary>
    ///  NOW YOU SHOW LOOK FOR EVERY OBJECT IN THE SCENE
    ///  (containers, tile, inventory etc... )
    ///  peut être préciser en mode ("take milk FROM shelf")
    ///  ..tu vois ?
    /// </summary>

    public List<Item> items = new List<Item>();

    public int id = 0;

    public Item item;

    public bool hasBeenUsed = true;
}
         