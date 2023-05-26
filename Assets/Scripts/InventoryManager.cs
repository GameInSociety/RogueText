using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public string[] startItems;

    void Start()
    {
        foreach (var itemName in startItems)
        {
            Item newItem = ItemManager.Instance.CreateFromData(itemName);
            Inventory.Instance.AddItem(newItem);
        }

        Invoke("StartDelay", 0f);
    }

    void StartDelay()
    {
        DebugManager.Instance.inventory = Inventory.Instance;
    }

    public static void Event_Throw()
    {
        Item item = Inventory.Instance.GetItem(InputInfo.Instance.GetItem(0).word.text);

        if (item == null)
        {
            TextManager.Write("inventory_throw_nothing");
            return;
        }

        // remove && add
        Inventory.Instance.RemoveItem(item);
        Tile.GetCurrent.AddItem(InputInfo.Instance.GetItem(0));

        TextManager.Write("inventory_throw_sucess", InputInfo.Instance.GetItem(0));
    }

    public static void Event_PickUp()
    {
        List<Item> targetItems = new List<Item>();

        if (InputInfo.Instance.actionOnAll)
        {
            Debug.Log("trying action on all items");
            targetItems = Tile.GetCurrent.GetContainedItems.FindAll(x => x.word.text == InputInfo.Instance.GetItem(0).word.text);
        }
        else
        {
            targetItems.Add(InputInfo.Instance.GetItem(0));
        }

        foreach (var item in targetItems)
        {
            if (Inventory.Instance.HasItem(item))
            {
                TextManager.Write("inventory_pickUp_already", InputInfo.Instance.GetItem(0));
            }
            else
            {
                item.PickUp();
            }

        }
    }

    #region remove item
    public static void Event_DestroyItem()
    {
        Item item;

        if (CellEvent.HasContent())
        {
            string item_name = CellEvent.GetContent(0);
            item = ItemManager.Instance.FindInWorld(item_name);
        }
        else
        {
            item = InputInfo.Instance.GetItem(0);
        }

        if (item == null)
        {
            Debug.LogError("couldn't find item " + CellEvent.GetContent(0));
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
        if (CellEvent.HasValue(1))
        {
            amount = CellEvent.GetValue(1);
        }

        string item_name = CellEvent.GetContent(0);

        for (int i = 0; i < amount; i++)
        {
            Item item = ItemManager.Instance.CreateInTile(Tile.GetCurrent, item_name);
            TextManager.Write("tile_addItem", item);
        }

    }
    #endregion

    #region item requirements
    public static void Event_RequireItem()
    {
        // on peut faire en sorte qu'on ait besoin de quelque chose avec un certain PARAMETRE
        // type
        // RequireProp(canCharge) // RequireProp(waterSource) pour les seaux, arrosoir, gourde etc.. 
        // là c'est dans une fonction alors que ça pourrait être dans la case !!!!!!


        if (CellEvent.HasContent(0))
        {

            string item_name = CellEvent.GetContent(0);
            Item targetItem = ItemManager.Instance.FindInWorld(item_name);

            if (targetItem == null)
            {
                // found no item in container, inventory or tile
                // break flow of actions
                targetItem = ItemManager.Instance.GetDataItem(item_name);
                TextManager.Write("item_require", targetItem);
                CellEvent.Break();
                return;
            }

            TextManager.Write("you use &the dog (override)&", targetItem);
            return;
        }


        Debug.Log("action require any item ?");
        // no target item, just ask for a second item
        if ( !InputInfo.Instance.HasItem(1) )
        {
            TextManager.Write("item_noSecondItem", InputInfo.Instance.GetItem(0));
            InputInfo.Instance.sustainVerb = true;
            InputInfo.Instance.sustainItem = true;
            CellEvent.Break();
            return;
        }

        

        // s'il a l'objet en question, ne rien faire, juste continuer les actions
    }
    #endregion
}
