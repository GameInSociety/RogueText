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
        ActionManager.onAction += HandleOnAction;

        items.Add( Item.FindByName("boussole") );
    }

	void HandleOnAction (Action action)
	{
		switch (action.type) {
		case Action.Type.Take:
			PickUpCurrentItem ();
			break; 
		case Action.Type.DisplayInventory:
			GetDescription ();
			break;
        case Action.Type.CloseInventory:
            CloseInventory();
            break;
        case Action.Type.AddToInventory:
			AddToInventoryFromString ();
			break;
		case Action.Type.RemoveFromInventory:
			RemoveFromInventoryFromString ();
			break;
        case Action.Type.AddToTile:
            AddToTile();
            break;
        case Action.Type.RemoveFromTile:
            RemoveCurrentItemFromTile();
            break;
        case Action.Type.Require:
			CheckRequire ();
			break;
        case Action.Type.Throw:
            ThrowCurrentItem();
            break;
        case Action.Type.OpenContainer:
            OpenContainer();
            break;
        case Action.Type.CloseContainer:
            CloseContainer();
            break;
        case Action.Type.RemoveLastItem:
            RemoveLastItem();
            break;
        case Action.Type.ReplaceItem:
            ReplaceCurrentItem();
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
            DisplayFeedback.Instance.Display("Vous n'avez pas " + InputInfo.GetCurrent.MainItem.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Undefined, Word.Preposition.De, Word.Number.None));
            return;
        }

        RemoveItem(item);

        Tile.current.AddItem(InputInfo.GetCurrent.MainItem);

        DisplayFeedback.Instance.Display("Vous posez " + InputInfo.GetCurrent.MainItem.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.None) + " par terre");

    }

    void PickUpCurrentItem()
    {
        Debug.Log("picking up item");
        List<Item> targetItems = new List<Item>();

        if ( InputInfo.GetCurrent.actionOnAll)
        {
            targetItems = Tile.current.items.FindAll(x => x.word.text == InputInfo.GetCurrent.MainItem.word.text);

            Debug.Log("found " + targetItems.Count + " items named " + InputInfo.GetCurrent.MainItem.word.text);
        }
        else
        {
            targetItems.Add(InputInfo.GetCurrent.MainItem);
        }

        foreach (var item in targetItems)
        {
            if (weight + item.weight > maxWeight)
            {
                DisplayFeedback.Instance.Display(InputInfo.GetCurrent.MainItem.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.Singular) + " est trop lourd pour le sac, il ne rentre pas");
                return;
            }

            Item.Remove(item);

            AddItem(item);

            DisplayFeedback.Instance.Display("Vous avez pris : " + item.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.Singular));
        }

        DisplayDescription.Instance.UpdateDescription();
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
		Item item = items.Find ( x => x.word.text.ToLower() == Action.GetCurrent.contents[0].ToLower() );

		if (item == null) {
			Debug.LogError ("couldn't find item " + Action.GetCurrent.contents[0] + " in inventory");
			return;
		}

		RemoveItem (item);
	}
    void RemoveCurrentItemFromTile()
    {
        Item item = Tile.current.items.Find(x => x.word.text.ToLower() == Action.GetCurrent.contents[0].ToLower());

        if (item == null)
        {
            Debug.LogError("couldn't find item " + Action.GetCurrent.contents[0] + " in inventory");
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

        item.SetAdjective(InputInfo.GetCurrent.MainItem.GetAdjective);

        Item.Remove(InputInfo.GetCurrent.MainItem);

    }

    void AddToInventoryFromString ()
	{
		Item item = Item.items.Find ( x => x.word.text.ToLower() == Action.GetCurrent.contents[0].ToLower() );

		if (item == null) {
			Debug.LogError ("couldn't find item " + Action.GetCurrent.contents[0] + " in item list");
			return;
		}

        int amount = 1;
        if (Action.GetCurrent.ints.Count > 0)
        {
            amount = Action.GetCurrent.ints[0];
        }

        for (int i = 0; i < amount; i++)
        {
            AddItem(item);
        }
    }
    Item AddToTile()
    {
		Item item = Item.items.Find ( x => x.word.text.ToLower() == Action.GetCurrent.contents[0].ToLower() );

        if (item == null)
        {
            Debug.LogError("couldn't find item " + Action.GetCurrent.contents[0] + " in item list");
            return null;
        }

        int amount = 1;
        if (Action.GetCurrent.ints.Count > 0)
        {
            amount = Action.GetCurrent.ints[0];
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
		bool hasOnOfTheItems = false;

        foreach (var content in Action.GetCurrent.contents) {

            /*Item item = Item.GetInWord(content);

			if ( item != null ){
                Debug.Log("found required item : " + item.word.name);
				hasOnOfTheItems = true;
				break;
			}*/

            if (InputInfo.GetCurrent.items.Count > 1)
            {
                if (content.StartsWith(InputInfo.GetCurrent.items[1].word.text))
                {
                    Debug.Log("found required item : " + InputInfo.GetCurrent.items[1].word.text);
                    hasOnOfTheItems = true;
                    break;
                }
            }
        }

		if ( hasOnOfTheItems == false ) {

			ActionManager.Instance.BreakAction ();

            /*string phrase = "Peut pas : besoin de : ";

			foreach (var content in Action.last.contents) {
				phrase += content + ", ";
			}*/

            //DisplayFeedback.Instance.Display("Vous ne pouvez pas " + Action.last.verb.names[0] + " " + Action.last.primaryItem.word.GetDescription(Word.Def.Defined));
            //DisplayFeedback.Instance.Display("Vous avez besoin : " + Action.last.contents[0]);
        }
    }

    #region open / close inventory
    public bool opened = false;
    public string GetDescription ()
	{
		if ( items.Count == 0 ) {
			return "Vous n'avez rien dans votre sac";
		}

        opened = true;

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
        Debug.Log("opening container");

        Item item = InputInfo.GetCurrent.MainItem;

        item.GenerateItems();
        Container.opened = true;
        Container.openedItem = item;
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
