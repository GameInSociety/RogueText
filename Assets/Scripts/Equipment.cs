using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment {

    public static Equipment Instance;

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

        ActionManager.onAction += HandleOnAction;

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

    void HandleOnAction(Action action)
    {
        switch (action.type)
        {
            case Action.Type.Equip:
                Action_Equip();
                break;
            case Action.Type.Unequip:
                Action_Unequip();
                break;
            default:
                break;
        }
    }

    void Action_Equip()
    {
        Part part = GetPartFromString(Action.GetCurrent.contents[0]);

        if ( GetEquipement(part) != null)
        {
            Inventory.Instance.AddItem(GetEquipement(part));
        }

        SetEquipment(part, InputInfo.GetCurrent.MainItem);

        Item.Remove(InputInfo.GetCurrent.MainItem);


        DisplayFeedback.Instance.Display("Vous avez équipé " + InputInfo.GetCurrent.MainItem.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Undefined , Word.Preposition.None, Word.Number.Singular) + " à " + part.ToString());

    }

    void Action_Unequip()
    {
        Part part = GetPartFromString(Action.GetCurrent.contents[0]);

        if ( GetEquipement(part) != InputInfo.GetCurrent.MainItem)
        {
            DisplayFeedback.Instance.Display("Vous n'avez pas " + InputInfo.GetCurrent.MainItem.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.De, Word.Number.Singular) + " sur vous");
            return;
        }

        Inventory.Instance.AddItem(InputInfo.GetCurrent.MainItem);

        DisplayFeedback.Instance.Display("Vous enlevez " + InputInfo.GetCurrent.MainItem.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.None) );

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
