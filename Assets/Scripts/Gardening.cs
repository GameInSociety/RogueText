using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gardening
{
    public static void Grow(Property grow_property)
    {
        // also remove dry property
        if (grow_property.item.HasProperty("dry"))
        {
            Property growProp = grow_property.item.GetProperty("dry");
            TimeManager.GetInstance().onNextHour -= growProp.HandleOnNextHour;
        }

        // get the name of the vegetanble ( complètement con mais on verra plus tard )
        // c'est pas si mal en fait
        if ( grow_property.param == "?")
        {
            grow_property.param = grow_property.item.GetProperty("type").GetContent();
        }

        string targetItemName = grow_property.param;

        Item.Remove(grow_property.item);

        Item newItem = Item.GetDataItem(targetItemName);
        newItem = Item.CreateNew(newItem);

        if (grow_property.item.HasProperty("type"))
        {
            newItem.AddProperty(grow_property.item.GetProperty("type"));
        }

        Tile.GetCurrent.AddItem(newItem);

        PhraseKey.SetOverrideItem(newItem);
        PhraseKey.Write("gardening_grew");
    }

    public static void Dry(Property prop)
    {
        // remove grow event also
        Property growProp = prop.item.GetProperty("grow");
        TimeManager.GetInstance().onNextHour -= growProp.HandleOnNextHour;

        Item.Remove(prop.item);

        Debug.LogError("dry problem, it add the item on the curent tile");
        Item newItem = Item.GetDataItem("dead sprout");
        newItem = Item.CreateNew(newItem);
        Tile.GetCurrent.AddItem(newItem);

        /*Phrase.SetOverrideItem(newItem);
        Phrase.Write("&le chien (override item)& a sêché et laisse place à une pousse morte");*/
    }

    public static void Water()
    {
        InputInfo.GetCurrent.MainItem.GetProperty("dry").SetValue(2);

        PhraseKey.Write("gardening_water");
    }
}
