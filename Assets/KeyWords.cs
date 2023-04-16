using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyWords
{
    public enum KeyWord {
        SURROURING_TILE_ORIENTATION,
        CONTAINER_LIST,
        ITEM_PROPERTIES,
        VERB_NAME,
        VERB_QUESTION,
        ITEM_VERBS,
        TILE_CURRENT_DESCRIPTION,
        TILE_SURROUNDING_DESCRIPTION,
        TILE_ITEM_DESCRIPTION,
        TIME_OF_DAY,
        PLAYER_ORIENTATION_FROM_NORTH,
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
            case KeyWord.SURROURING_TILE_ORIENTATION:
                return Coords.GetOrientationText(SurroundingTileManager.currentSurroundingTile.orientations);
            case KeyWord.CONTAINER_LIST:
                return Item.ItemListString(Container.CurrentItem.containedItems, Item.ListSeparator.Commas, true);
            case KeyWord.ITEM_PROPERTIES:
                return InputInfo.GetCurrent.MainItem.GetPropertiesDescription();
            case KeyWord.VERB_NAME:
                return InputInfo.GetCurrent.verb.names[0];
            case KeyWord.VERB_QUESTION:
                return InputInfo.GetCurrent.verb.question;
            case KeyWord.ITEM_VERBS:
                return InputInfo.GetCurrent.MainItem.GetVerbsDescription();
            case KeyWord.TILE_CURRENT_DESCRIPTION:
                return Tile.GetCurrent.GetDescription();
            case KeyWord.TILE_SURROUNDING_DESCRIPTION:
                return SurroundingTileManager.GetSurroundingTilesDescription();
            case KeyWord.TILE_ITEM_DESCRIPTION:
                return Tile.GetCurrent.GetItemDescriptions();
            case KeyWord.TIME_OF_DAY:
                return TimeManager.GetInstance().GetTimeOfDayDescription();
            case KeyWord.PLAYER_ORIENTATION_FROM_NORTH:
                return Coords.GetOrientationText(Coords.GetOrientationFromNorth(Player.Instance.currentCarnidal));

            default:
                Debug.LogError("no text for KEY WORD " + keyWord.ToString());
                return keyWord.ToString();
        }
    }
}
