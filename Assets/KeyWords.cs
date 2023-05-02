using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyWords
{
    public enum KeyWord {
        CONTAINER_LIST,
        ITEM_DESCRIPTION,
        ITEM_VERBS,
        ITEM_PROPERTIES,
        VERB_NAME,
        VERB_QUESTION,
        TILE_CURRENT_DESCRIPTION,
        TILE_ITEM_DESCRIPTIONS,
        TIME_OF_DAY,

        TARGET_ORIENTATION
    }

    public static string ReplaceKeyWords(string str)
    {
        foreach (KeyWord keyWord in Enum.GetValues(typeof(KeyWord)))
        {
            if (str.Contains(keyWord.ToString()))
            {
                str = str.Replace(keyWord.ToString(), GetKeyWord(keyWord));
            }
        }

        return str;
    }

    public static string GetKeyWord(KeyWord keyWord)
    {
        switch (keyWord)
        {
            case KeyWord.CONTAINER_LIST:
                return Item.ItemListString(Item.OpenedItem.GetContainedItems, Item.ListSeparator.Commas, true);
            case KeyWord.TILE_ITEM_DESCRIPTIONS:
                return Tile.GetCurrent.GetItemDescriptions();
            case KeyWord.TIME_OF_DAY:
                return TimeManager.GetInstance().GetTimeOfDayDescription();
            case KeyWord.TARGET_ORIENTATION:
                return Coords.GetOrientationText(TextManager.GetOverrideOrientations());
            case KeyWord.ITEM_DESCRIPTION:
                InputInfo.Instance.GetItem(0).WritePropertiesDescription();
                return "";
            case KeyWord.VERB_NAME:
                return InputInfo.Instance.verb.names[0];
            case KeyWord.VERB_QUESTION:
                return InputInfo.Instance.verb.question;

            default:
                Debug.LogError("no text for KEY WORD " + keyWord.ToString());
                return keyWord.ToString();
        }
    }
}
