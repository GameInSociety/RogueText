using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

	public static Inventory Instance;

    public int maxWeight = 15;

    public int weight = 0;

    public List<Item> items = new List<Item>();

    /// <summary>
    /// container
    /// </summary>

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
	void Start () {
        PlayerActionManager.onPlayerAction += HandleOnAction;

        items.Add( Item.GetDataItem("boussole") );
    }

	void HandleOnAction (PlayerAction action)
	{
		switch (action.type) {
		case PlayerAction.Type.PickUp:
			PickUp ();
			break;
            case PlayerAction.Type.DisplayInventory:
			WriteDescription ();
			break;
        case PlayerAction.Type.CloseInventory:
            CloseInventory();
            break;
        case PlayerAction.Type.AddToTile:
            AddToTile();
            break;
        case PlayerAction.Type.Remove:
            RemoveItem();
            break;
        case PlayerAction.Type.RequireItem:
			RequireItem ();
			break;
        case PlayerAction.Type.Throw:
            ThrowCurrentItem();
            break;
        case PlayerAction.Type.OpenContainer:
            OpenContainer();
            break;
        case PlayerAction.Type.CloseContainer:
            CloseContainer();
            break;
			break;
		}
	}

    public Item GetItem (string itemName)
    {
        return items.Find(x => x.word.text == itemName);
    }

    private void ThrowCurrentItem()
    {
        Item item = GetItem(InputInfo.GetCurrent.MainItem.word.text);

        if (item == null)
        {
            Phrase.Write("Vous n'avez pas &de chien (main item)&");
            return;
        }

        RemoveItem(item);

        Tile.GetCurrent.AddItem(InputInfo.GetCurrent.MainItem);

        string str = "Vous posez &le chien (main item)& par terre";
        Phrase.Write(str);
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
            item.PickUp();
        }
    }

    // fill item with water 
    void FillItem()
    {
        if (!InputInfo.GetCurrent.HasSecondItem())
        {
            DisplayFeedback.Instance.Display("Ou voulez vous remplir &le chien (main item)&");
            return;
        }

        if (!InputInfo.GetCurrent.GetSecondItem.HasProperty("water"))
        {
            DisplayFeedback.Instance.Display("Vous vous ne pouvez pas remplir &le chien (main item)& dans &le chien (second item)&");
            return;
        }

        if (InputInfo.GetCurrent.GetSecondItem.GetProperty( "water" ).GetValue() == 0)
        {
            DisplayFeedback.Instance.Display("il n'y a plus d'eau dans &le chien (main item)&");
            return;
        }

        if (InputInfo.GetCurrent.MainItem.GetProperty("full").GetContent() == "true")
        {
            DisplayFeedback.Instance.Display("&le chien (main item)& est déjà plein");
            return;
        }

        DisplayFeedback.Instance.Display("&le chien (main item)& est maintenant rempli");
        InputInfo.GetCurrent.MainItem.GetProperty("full").SetContent("true");
        InputInfo.GetCurrent.GetSecondItem.GetProperty("water").Remove(1);
    }

    #region remove item
    public void RemoveItem(int itemRow)
    {
        RemoveItem(items.Find(x => x.index == itemRow));
    }
    public void RemoveItem ( Item item ) {

        weight -= item.weight;
        items.Remove(item);
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
        weight += item.weight;
        items.Add(item);
    }

    /*void AddToInventoryFromString ()
	{
		Item item = Item.dataItems.Find ( x => x.word.text.ToLower() == PlayerAction.GetCurrent.contents[0].ToLower() );

		if (item == null) {
			Debug.LogError ("couldn't find item " + PlayerAction.GetCurrent.contents[0] + " in item list");
			return;
		}

        int amount = 1;
        if (PlayerAction.GetCurrent.values.Count > 0)
        {
            amount = PlayerAction.GetCurrent.values[0];
        }

        for (int i = 0; i < amount; i++)
        {
            AddItem(item);
        }
    }*/
    public Item AddToTile()
    {
        Item item = Item.GetDataItem(PlayerAction.GetCurrent.GetContent(0));

        if (item == null)
        {
            Debug.LogError("couldn't find item " + PlayerAction.GetCurrent.GetContent(0) + " in item list");
            return null;
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

        return item;
    }
    #endregion

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
            Phrase.SetOverrideItem(targetItem);
            Phrase.Write("Vous n'avez pas &de chien(override item)&");
            PlayerActionManager.Instance.BreakAction();
            return;
        }

        Phrase.SetOverrideItem(targetItem);
        Phrase.Write("Vous utilisez &le chien(override item)&");

        // s'il a l'objet en question, ne rien faire, juste continuer les actions
    }
    void RequireProp()
    {
        if (!InputInfo.GetCurrent.HasSecondItem())
        {
            DisplayFeedback.Instance.Display(InputInfo.GetCurrent.verb.question + " voulez vous " + InputInfo.GetCurrent.verb.names[0] + "&le chien (main item)&");
            return;
        }

        string prop_name = PlayerAction.GetCurrent.GetContent(0);

        if (!InputInfo.GetCurrent.GetSecondItem.HasProperty(prop_name))
        {
            DisplayFeedback.Instance.Display("Vous ne pouvez pas " + InputInfo.GetCurrent.verb.names[0] + "&le chien (second item)&");
            return;
        }

        if (InputInfo.GetCurrent.GetSecondItem.GetProperty(prop_name).GetValue() == 0)
        {
            // aller chercher prop names
            // est-ce que c'est la meme valeur partout ?
            // est-ce que c'est le meme texte qui apparait partout ?
            // une nouvelle fonction ? CheckIfThereStillSomethingLeft()
            Debug.LogError("ici y'a un truc à changer ça veut rien dire");
            DisplayFeedback.Instance.Display("il n'y a plus d'eau dans &le chien (main item)&");
            return;
        }

        // ici, une autre fonction
        if (InputInfo.GetCurrent.MainItem.GetProperty("full").GetContent() == "true")
        {
            DisplayFeedback.Instance.Display("&le chien (main item)& est déjà plein");
            return;
        }

        DisplayFeedback.Instance.Display("&le chien (main item)& est maintenant rempli");
        InputInfo.GetCurrent.MainItem.GetProperty("full").SetContent("true");
        InputInfo.GetCurrent.GetSecondItem.GetProperty("water").Remove(1);
    }
    #endregion

    #region open / close inventory
    public bool opened = false;
    public void WriteDescription ()
	{
        opened = true;
        Phrase.Renew();
        Phrase.Write(GetDescription());
    }

    public string GetDescription()
    {
        if (items.Count == 0)
        {
            return "Vous n'avez rien dans votre sac";
        }

        string str = "Dans votre sac :\n\n";

        str += Item.ItemListString(items, true, true);

        str += "\n\nFermer le sac ?";

        return str;
    }
    void CloseInventory()
    {
        opened = false;

        Phrase.Write("Vous fermez votre sac...");
        //DisplayDescription.Instance.UpdateDescription();
    }
    #endregion

    #region container
    private void OpenContainer()
    {
        InputInfo.GetCurrent.MainItem.Open();
    }

    private void CloseContainer()
    {
        if (opened)
        {
            CloseInventory();
            return;
        }

        Container.opened = false;

        // de retour dans la tile, est-ce qu'on a vraiment besoin d'une description totale ?*
        //DisplayDescription.Instance.UpdateDescription();

        Phrase.Write("Vous fermez &le chien (main item)&");
    }
    #endregion
}
