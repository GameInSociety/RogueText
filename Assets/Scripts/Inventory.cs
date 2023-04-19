using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

	public static Inventory Instance;

    public int maxWeight = 15;

    public Item bag_Item;

    //public List<Item> items = new List<Item>();

    /// <summary>
    /// container
    /// </summary>

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
	void Start () {
        PlayerActionManager.onPlayerAction += HandleOnAction;

        bag_Item.AddItem(Item.GetDataItem("compas"));
    }

	void HandleOnAction (PlayerAction action)
	{
		switch (action.type) {
		case PlayerAction.Type.PickUp:
			PickUp ();
			break;
        case PlayerAction.Type.AddToTile:
            AddToTile();
            break;
        case PlayerAction.Type.RemoveItem:
            RemoveItem();
            break;
        case PlayerAction.Type.RequireItem:
			RequireItem ();
			break;
        case PlayerAction.Type.Throw:
            ThrowCurrentItem();
            break;
        case PlayerAction.Type.OpenContainer:
            InputInfo.GetCurrent.MainItem.Open();
            break;
        case PlayerAction.Type.CloseContainer:
            InputInfo.GetCurrent.MainItem.Close();
            break;
		}
	}

    public Item GetItem (string itemName)
    {
        return bag_Item.GetItem(itemName);
    }

    public bool HasItem (string item_name)
    {
        return GetItem(item_name) != null;
    }

    private void ThrowCurrentItem()
    {
        Item item = GetItem(InputInfo.GetCurrent.MainItem.word.text);

        if (item == null)
        {
            PhraseKey.WritePhrase("inventory_throw_nothing");
            return;
        }

        RemoveItem(item);

        Tile.GetCurrent.AddItem(InputInfo.GetCurrent.MainItem);

        PhraseKey.WritePhrase("inventory_throw_sucess");
    }

    void PickUp()
    {
        List<Item> targetItems = new List<Item>();

        if (InputInfo.GetCurrent.actionOnAll)
        {
            Debug.Log("trying action on all items");
            targetItems = Tile.GetCurrent.items.FindAll(x => x.word.text == InputInfo.GetCurrent.MainItem.word.text);
        }
        else
        {
            targetItems.Add(InputInfo.GetCurrent.MainItem);
        }

        foreach (var item in targetItems)
        {
            if (bag_Item.ContainsItem(item))
            {
                PhraseKey.WritePhrase("inventory_pickUp_already");
            }
            else
            {
                item.PickUp();
            }

        }
    }

    #region remove item
    // no sense : crafting system is weird
    public void RemoveItem(int itemRow)
    {
        bag_Item.RemoveItem(itemRow);
    }
    public void RemoveItem ( Item item ) {

        bag_Item.RemoveItem(item);
	}
    void RemoveItem()
    {
        Item item;

        if (PlayerAction.GetCurrent.HasContent())
        {
            string item_name = PlayerAction.GetCurrent.GetContent(0);
            item = Item.FindInWorld(item_name);
        }
        else
        {
            item = InputInfo.GetCurrent.MainItem;
        }

        if (item == null)
        {
            Debug.LogError("couldn't find item " + PlayerAction.GetCurrent.GetContent(0));
            return;
        }

        Item.Remove(item);
        //Tile.GetCurrent.RemoveItem(item);
    }
	#endregion

	#region add
	public void AddItem ( Item item ) {
        bag_Item.AddItem(item);
    }

    public Item FindItem(string str)
    {
        return bag_Item.FindItem(str);
    }

    public void AddToTile()
    {
        string item_name = PlayerAction.GetCurrent.GetContent(0);
        Item item = Item.CreateNew(item_name);

        if (item == null)
        {
            Debug.LogError("couldn't find item " + PlayerAction.GetCurrent.GetContent(0) + " in item list");
        }

        int amount = 1;
        if (PlayerAction.GetCurrent.HasValue(1))
        {
            amount = PlayerAction.GetCurrent.GetValue(1);
        }

        for (int i = 0; i < amount; i++)
        {
            Tile.GetCurrent.AddItem(item);
        }

        PhraseKey.WritePhrase("tile_addItem", item);
    }
    #endregion

    public bool CanSee()
    {
        Item item = Inventory.Instance.GetItem("torchlight");
        if ( item != null && item.GetProperty("battery").GetValue() > 0)
        {
            return true;
        }

        return false;
    }

    #region item requirements
    void RequireItem()
    {
        // on peut faire en sorte qu'on ait besoin de quelque chose avec un certain PARAMETRE
        // type
        // RequireProp(canCharge) // RequireProp(waterSource) pour les seaux, arrosoir, gourde etc.. 
        // là c'est dans une fonction alors que ça pourrait être dans la case !!!!!!

        string item_name = PlayerAction.GetCurrent.GetContent(0);
        Item targetItem = Item.FindInWorld(item_name);

        if (targetItem == null)
        {
            // found no item in container, inventory or tile
            // break flow of actions
            targetItem = Item.GetDataItem(item_name);
            PhraseKey.WritePhrase("item_require", targetItem);
            PlayerActionManager.Instance.BreakAction();
            return;
        }

        PhraseKey.WritePhrase("item_use", targetItem);

        // s'il a l'objet en question, ne rien faire, juste continuer les actions
    }
    void RequireProp()
    {
        if (!InputInfo.GetCurrent.HasSecondItem())
        {
            Debug.LogError("no second item");
            PhraseKey.WritePhrase("item_noSecondItem");
            return;
        }

        Debug.LogError("he has a second item ? what ?");

        string prop_name = PlayerAction.GetCurrent.GetContent(0);

        if (!InputInfo.GetCurrent.GetSecondItem.HasProperty(prop_name))
        {
            PhraseKey.WritePhrase("item_noProperty");
            return;
        }

        if (InputInfo.GetCurrent.GetSecondItem.GetProperty(prop_name).GetValue() == 0)
        {
            // aller chercher prop names
            // est-ce que c'est la meme valeur partout ?
            // est-ce que c'est le meme texte qui apparait partout ?
            // une nouvelle fonction ? CheckIfThereStillSomethingLeft()
            Debug.LogError("ici y'a un truc à changer ça veut rien dire");
            PhraseKey.WritePhrase("item_NoMoreValue");
            return;
        }

        Debug.Log("prop : " + prop_name + " bien présent, on passe à la suite");
    }
    #endregion

    #region open / close inventory
    public bool IsOpened
    {
        get
        {
            return bag_Item.opened;
        }
    }
    #endregion

}
