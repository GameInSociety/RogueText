using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public string[] startItems;

    void Start()
    {
        Item item = ItemManager.Instance.GetDataItem("inventory");

        var serializedParent = JsonConvert.SerializeObject(item);
        Inventory.Instance = JsonConvert.DeserializeObject<Inventory>(serializedParent);

        PlayerActionManager.onPlayerAction += HandleOnAction;

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
    void HandleOnAction(PlayerAction action)
    {
        switch (action.type)
        {
            case PlayerAction.Type.PickUp:
                Action_PickUp();
                break;
            case PlayerAction.Type.CreateInTile:
                CreateInTile();
                break;
            case PlayerAction.Type.DestroyItem:
                Action_DesctroyItem();
                break;
            case PlayerAction.Type.RequireItem:
                Action_RequireItem();
                break;
            case PlayerAction.Type.Throw:
                Action_Throw();
                break;
            case PlayerAction.Type.OpenContainer:
                InputInfo.Instance.GetItem(0).Open();
                break;
            case PlayerAction.Type.CloseContainer:
                InputInfo.Instance.GetItem(0).Close();
                break;
        }
    }



    private void Action_Throw()
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

    void Action_PickUp()
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
    void Action_DesctroyItem()
    {
        Item item;

        if (PlayerAction.GetCurrent.HasContent())
        {
            string item_name = PlayerAction.GetCurrent.GetContent(0);
            item = ItemManager.Instance.FindInWorld(item_name);
        }
        else
        {
            item = InputInfo.Instance.GetItem(0);
        }

        if (item == null)
        {
            Debug.LogError("couldn't find item " + PlayerAction.GetCurrent.GetContent(0));
            return;
        }

        Item.Destroy(item);
        //Tile.GetCurrent.RemoveItem(item);
    }
    #endregion

    #region add

    public void CreateInTile()
    {
        int amount = 1;
        if (PlayerAction.GetCurrent.HasValue(1))
        {
            amount = PlayerAction.GetCurrent.GetValue(1);
        }

        string item_name = PlayerAction.GetCurrent.GetContent(0);

        for (int i = 0; i < amount; i++)
        {
            Item item = ItemManager.Instance.CreateInTile(Tile.GetCurrent, item_name);
            TextManager.Write("tile_addItem", item);
        }

    }
    #endregion

    #region item requirements
    void Action_RequireItem()
    {
        // on peut faire en sorte qu'on ait besoin de quelque chose avec un certain PARAMETRE
        // type
        // RequireProp(canCharge) // RequireProp(waterSource) pour les seaux, arrosoir, gourde etc.. 
        // là c'est dans une fonction alors que ça pourrait être dans la case !!!!!!


        if (PlayerAction.GetCurrent.HasContent(0))
        {

            string item_name = PlayerAction.GetCurrent.GetContent(0);
            Item targetItem = ItemManager.Instance.FindInWorld(item_name);

            if (targetItem == null)
            {
                // found no item in container, inventory or tile
                // break flow of actions
                targetItem = ItemManager.Instance.GetDataItem(item_name);
                TextManager.Write("item_require", targetItem);
                PlayerActionManager.Instance.BreakAction();
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
            PlayerActionManager.Instance.BreakAction();
            return;
        }

        

        // s'il a l'objet en question, ne rien faire, juste continuer les actions
    }
    #endregion
}
