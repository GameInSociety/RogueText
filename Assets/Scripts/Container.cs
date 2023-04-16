using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{
    public static bool opened = false;
    public static Item CurrentItem;

    public List<Item> items = new List<Item>();

    public int id = 0;

    public Item item;

    public bool hasBeenUsed = true;
}
         