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

        items.Add( Item.FindByName("boussole") );
    }

	void HandleOnAction (PlayerAction action)
	{
		switch (action.type) {
		case PlayerAction.Type.Take:
			PickUpCurrentItem ();
			break;
            case PlayerAction.Type.DisplayInventory:
			Describe ();
			break;
        case PlayerAction.Type.CloseInventory:
            CloseInventory();
            break;
        case PlayerAction.Type.AddToInventory:
			AddToInventoryFromString ();
			break;
		case PlayerAction.Type.RemoveFromInventory:
			RemoveFromInventoryFromString ();
			break;
        case PlayerAction.Type.AddToTile:
            AddToTile();
            break;
        case PlayerAction.Type.RemoveFromTile:
            RemoveCurrentItemFromTile();
            break;
        case PlayerAction.Type.Require:
			CheckRequire ();
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
        case PlayerAction.Type.RemoveLastItem:
            RemoveLastItem();
            break;
        case PlayerAction.Type.ReplaceItem:
            ReplaceCurrentItem();
            break;
            case PlayerAction.Type.Fill:
            FillItem();
            break;
            default:
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
            DisplayFeedback.Instance.Display("Vous n'avez pas " + InputInfo.GetCurrent.MainItem.word.GetContent("de chien"));
            return;
        }

        RemoveItem(item);

        Tile.current.AddItem(InputInfo.GetCurrent.MainItem);

        DisplayFeedback.Instance.Display("Vous posez " + InputInfo.GetCurrent.MainItem.word.GetContent("le chien") + " par terre");

    }

    void PickUpCurrentItem()
    {
        Item item = InputInfo.GetCurrent.MainItem;

        List<Item> targetItems = new List<Item>();

        if (InputInfo.GetCurrent.actionOnAll)
        {
            targetItems = Tile.current.items.FindAll(x => x.word.text == InputInfo.GetCurrent.MainItem.word.text);
        }
        else
        {
            targetItems.Add(InputInfo.GetCurrent.MainItem);
        }

        if (weight + item.weight > maxWeight)
        {
            DisplayFeedback.Instance.Display(InputInfo.GetCurrent.MainItem.word.GetContent("le chien") + " est trop lourd pour le sac, il ne rentre pas");
            return;
        }

        Item.Remove(item);

        AddItem(item);

        DisplayFeedback.Instance.Display("Vous avez pris : " + item.word.GetContent("le chien"));

    }

    void FillItem()
    {

        Item item = InputInfo.GetCurrent.MainItem;

        if (InputInfo.GetCurrent.items.Count < 2)
        {
            DisplayFeedback.Instance.Display("Ou voulez vous remplir &le chapeau chic&");
            return;
        }

        // solution crasseuse et pas pratique :
        // ce qu'il faut faire :
        // rajouter une property "fillArea" ou "usedToFill" tu vois le genre.
        // les properties ça va te changer la vie je pense.
        List<string> itemNames = new List<string>();
        itemNames.Add("évier");
        itemNames.Add("douche");
        itemNames.Add("cuvette");
        itemNames.Add("lavabo");

        if (InputInfo.GetCurrent.items.Find(x => itemNames.Contains(x.word.text)) == null)
        {
            DisplayFeedback.Instance.Display("Vous vous ne pouvez pas remplir &le chapeau chic& dans &le chapeau chic2&");
        }

        if (!Tile.current.HasItem(itemNames.ToArray()))
        {
            DisplayFeedback.Instance.Display("Vous n'avez nulle part ou remplir &le chapeau chic&");
            return;
        }

        if ( item.GetProperty("full").GetValue() == "true")
        {
            DisplayFeedback.Instance.Display("&le chapeau chic& est déjà plein");
            return;
        }
        else
        {
            DisplayFeedback.Instance.Display("Vous remplissez ITEM...");
            item.GetProperty("full").SetValue("true");
        }
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
    void RemoveLastItem()
    {
        Item.Remove(InputInfo.GetCurrent.MainItem);
    }

	void RemoveFromInventoryFromString ()
	{
		Item item = items.Find ( x => x.word.text.ToLower() == PlayerAction.GetCurrent.contents[0].ToLower() );

		if (item == null) {
			Debug.LogError ("couldn't find item " + PlayerAction.GetCurrent.contents[0] + " in inventory");
			return;
		}

		RemoveItem (item);
	}
    void RemoveCurrentItemFromTile()
    {
        Item item = Tile.current.items.Find(x => x.word.text.ToLower() == PlayerAction.GetCurrent.contents[0].ToLower());

        if (item == null)
        {
            Debug.LogError("couldn't find item " + PlayerAction.GetCurrent.contents[0] + " in inventory");
            return;
        }

        Tile.current.RemoveItem(item);
    }
	#endregion

	#region add
	public void AddItem ( Item item ) {
        weight += item.weight;
        items.Add(item);
    }

    void ReplaceCurrentItem()
    {
        Item item = AddToTile();

        item.word.SetAdjective(InputInfo.GetCurrent.MainItem.word.GetAdjective);

        Item.Remove(InputInfo.GetCurrent.MainItem);

    }

    void AddToInventoryFromString ()
	{
		Item item = Item.items.Find ( x => x.word.text.ToLower() == PlayerAction.GetCurrent.contents[0].ToLower() );

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
    }
    public Item AddToTile()
    {
		Item item = Item.items.Find ( x => x.word.text.ToLower() == PlayerAction.GetCurrent.contents[0].ToLower() );

        if (item == null)
        {
            Debug.LogError("couldn't find item " + PlayerAction.GetCurrent.contents[0] + " in item list");
            return null;
        }

        int amount = 1;
        if (PlayerAction.GetCurrent.values.Count > 0)
        {
            amount = PlayerAction.GetCurrent.values[0];
        }

        for (int i = 0; i < amount; i++)
        {
            Tile.current.AddItem(item);
        }

        return item;
    }
    #endregion

    void CheckRequire ()
	{
        /*if (InputInfo.GetCurrent.items.Count > 1)

		bool hasOnOfTheItems = false;

        foreach (var content in PlayerAction.GetCurrent.contents) {

            if (InputInfo.GetCurrent.items.Count > 1)
            {
                if (content.StartsWith(InputInfo.GetCurrent.items[1].word.text))
                {
                    Debug.Log("found required item : " + InputInfo.GetCurrent.items[1].word.text);
                    break;
                }
            }
        }

		if ( hasOnOfTheItems == false ) {

			PlayerActionManager.Instance.BreakAction ();
        }*/
    }


    #region open / close inventory
    public bool opened = false;
    public void Describe ()
	{
        opened = true;

        DisplayDescription.Instance.AddToDescription(GetDescription());
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

        DisplayDescription.Instance.UpdateDescription();
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

        DisplayDescription.Instance.UpdateDescription();
    }
    #endregion
}
