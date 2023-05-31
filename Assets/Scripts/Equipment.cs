using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment {

    public static Equipment Instance;

    // MAKE PARTS ITEMS
	public enum Part
    {
        Weapon,
        Head,
        Top,
        Bottom,
        Feet,
        Hands,
        Misc,

        None,
    }

    List<Item> items = new List<Item>();

    public void Init()
    {
        Instance = this;

        InitItems();
    }

    private void InitItems()
    {
        for (int i = 0; i < System.Enum.GetValues(typeof(Part)).Length; i++)
        {
            //Debug.Log("init equipement part : " + (Part)i);

            items.Add(null);
        }
    }

    public void Event_Equip()
    {
        Part part = GetPartFromString(FunctionManager.GetParam(0));

        if ( GetEquipement(part) != null)
        {
            Inventory.Instance.AddItem(GetEquipement(part));
        }

        SetEquipment(part, FunctionManager.GetCurrentItem());

        Item.Remove(FunctionManager.GetCurrentItem());

        TextManager.Write("/_bag_equip/" + part.ToString());

    }

    public void Event_Unequip()
    {
        Part part = GetPartFromString(FunctionManager.GetParam(0));

        if ( GetEquipement(part) != FunctionManager.GetCurrentItem())
        {
            TextManager.Write("bag_nothingToEquip");
            return;
        }

        Inventory.Instance.AddItem(FunctionManager.GetCurrentItem());

        TextManager.Write("bag_unequip");

        SetEquipment(part, null);
    }

    public Part GetPartFromString (string str)
    {
        Part part = Part.None;

        for (int i = 0; i < System.Enum.GetNames(typeof(Part)).Length; i++)
        {
            Part tmpPart = (Part)i;
            if (tmpPart.ToString().ToLower() == str)
            {
                Debug.Log("found part : " + tmpPart);
                part = tmpPart;
                break;
            }
        }

        if (part == Part.None)
        {
            Debug.LogError("did not find part in : " + str);
        }

        return part;
    }

    public void SetEquipment (Part part, Item item)
    {
        items[(int)part] = item;
    }

    public Item GetEquipement(Part part)
    {
        return items[(int)part];
    }


}
