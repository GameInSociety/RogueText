using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gardening
{
    public static void Grow(Item.Property prop)
    {
        // also remove dry property
        if (prop.item.HasProperty("dry"))
        {
            Item.Property growProp = prop.item.GetProperty("dry");
            TimeManager.GetInstance().onNextHour -= growProp.HandleOnNextHour;
        }
        

        Item.Remove(prop.item);

        string targetItemName = prop.param;

        Item newItem = Item.GetDataItem(targetItemName);
        newItem = Item.CreateNew(newItem);

        Tile.GetCurrent.AddItem(newItem);

        Phrase.SetOverrideItem(newItem);
        string str = "&un chien (override item)& apparait à la place &du chien (main item)&";
        Phrase.Write(str);
    }

    public static void Dry(Item.Property prop)
    {
        // remove grow event also
        Item.Property growProp = prop.item.GetProperty("grow");
        TimeManager.GetInstance().onNextHour -= growProp.HandleOnNextHour;

        Item.Remove(prop.item);

        Item newItem = Item.GetDataItem("pousse morte");
        newItem = Item.CreateNew(newItem);
        Tile.GetCurrent.AddItem(newItem);

        Phrase.SetOverrideItem(newItem);
        Phrase.Write("&le chien (override item)& a sêché et laisse place à une pousse morte");
    }

    public static void Water()
    {
        InputInfo.GetCurrent.MainItem.GetProperty("dry").SetValue(2);

        DisplayFeedback.Instance.Display("Vous arrosez &le chien&");
    }
}
