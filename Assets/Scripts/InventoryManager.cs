using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager
{
    public static void Event_Throw()
    {
        Item item = Inventory.Instance.GetItem(FunctionManager.GetCurrentItem().word.text);

        if (item == null)
        {
            TextManager.Write("inventory_throw_nothing");
            return;
        }

        // remove && add
        Inventory.Instance.RemoveItem(item);
        Tile.GetCurrent.AddItem(FunctionManager.GetCurrentItem());

        TextManager.Write("inventory_throw_sucess", FunctionManager.GetCurrentItem());
    }

    public static void Event_PickUp()
    {
        if (Inventory.Instance.HasItem(FunctionManager.GetCurrentItem()))
        {
            TextManager.Write("inventory_pickUp_already", FunctionManager.GetCurrentItem());
        }
        else
        {
            FunctionManager.GetCurrentItem().PickUp();
        }
    }

    #region remove item
    public static void Event_DestroyItem()
    {
        Item item;

        if (FunctionManager.HasParams())
        {
            string item_name = FunctionManager.GetParam(0);
            item = ItemManager.Instance.FindInWorld(item_name);
        }
        else
        {
            item = FunctionManager.GetCurrentItem();
        }

        if (item == null)
        {
            Debug.LogError("couldn't find item " + FunctionManager.GetParam(0));
            return;
        }

        Item.Destroy(item);
        //Tile.GetCurrent.RemoveItem(item);
    }
    #endregion

    #region add
    public static void Event_CreateInTile()
    {
        int amount = 1;
        if (FunctionManager.HasValue(1))
        {
            amount = FunctionManager.GetValue(1);
        }

        // if the target item starts with '*', getting the value of an other property
        // sprout gets value "vegetableType" pour savoir en quoi elle va pousser
        string item_name = FunctionManager.GetParam(0);
        if (item_name.StartsWith('*'))
        {
            string targetPropertyName = item_name.Remove(0, 1);
            item_name = FunctionManager.GetCurrentItem().GetProperty(targetPropertyName).value;
            Debug.Log("getting " + targetPropertyName + " on " + FunctionManager.GetCurrentItem().debug_name);
        }

        Item item = ItemManager.Instance.CreateInTile(Tile.GetCurrent, item_name);

        for (int i = 1; i < amount; i++)
        {
            ItemManager.Instance.CreateInTile(Tile.GetCurrent, item_name);
        }

        TextManager.Write("tile_addItem", item);

    }
    #endregion

    #region item requirements
    public static void Event_RequireItem()
    {
        // on peut faire en sorte qu'on ait besoin de quelque chose avec un certain PARAMETRE
        // type
        // RequireProp(canCharge) // RequireProp(waterSource) pour les seaux, arrosoir, gourde etc.. 
        // là c'est dans une fonction alors que ça pourrait être dans la case !!!!!!


        if (FunctionManager.HasParam(0))
        {

            string item_name = FunctionManager.GetParam(0);
            Item targetItem = ItemManager.Instance.FindInWorld(item_name);

            if (targetItem == null)
            {
                // found no item in container, inventory or tile
                // break flow of actions
                targetItem = ItemManager.Instance.GetDataItem(item_name);
                TextManager.Write("item_require", targetItem);
                FunctionManager.Break();
                return;
            }

            TextManager.Write("you use &the dog (override)&", targetItem);
            return;
        }


        Debug.Log("action require any item ?");
        // no target item, just ask for a second item
        if ( !FunctionManager.HasItem(1) )
        {
            TextManager.Write("item_noSecondItem", FunctionManager.GetCurrentItem());
            InputInfo.Instance.WaitForItem();
            FunctionManager.Break();
            return;
        }

        

        // s'il a l'objet en question, ne rien faire, juste continuer les actions
    }
    #endregion
}
