using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGroup
{
    public int count = 0;

    public Item item;

    public string GetWordGroup()
    {
        if (count > 5)
        {
            return "a lot " + item.word.GetContent("of dogs");
        }
        else if (count > 3)
        {
            return "a few " + item.word.GetContent("dogs");
        }
        else if (count > 1)
        {
            return count + " " + item.word.GetContent("dogs");
        }
        else
        {
            if (!item.stackable)
            {
                return item.word.GetContent("a good dog");
            }
            else
            {
                return item.word.GetContent("a dog");
            }
        }
    }

    public static List<ItemGroup> GetItemGroups(List<Item> items)
    {
        List<ItemGroup> itemGroups = new List<ItemGroup>();

        foreach (var item in items)
        {
            ItemGroup itemGroup = itemGroups.Find(x => x.item.index == item.index && item.stackable);
            //ItemSocket itemSocket = null;

            if (itemGroup == null)
            {
                itemGroup = new ItemGroup();

                itemGroup.item = item;

                itemGroup.count = 1;

                itemGroups.Add(itemGroup);
            }
            else
            {
                itemGroup.count++;
            }

        }

        return itemGroups;
    }

    public static string GetRelativeItemPositionPhrase(Item item)
    {
        string itemPosition = "";

        Player.Orientation fac = Player.Orientation.None;

        switch (item.GetProperty("direction").GetPart(1))
        {
            case "to north":
                fac = Player.Instance.GetOrientation(Cardinal.North);
                break;
            case "to south":
                fac = Player.Instance.GetOrientation(Cardinal.South);
                break;
            case "to east":
                fac = Player.Instance.GetOrientation(Cardinal.East);
                break;
            case "to west":
                fac = Player.Instance.GetOrientation(Cardinal.West);
                break;
            default:
                break;
        }

        switch (fac)
        {
            case Player.Orientation.Front:
                return PhraseKey.GetPhrase("position_front");
            case Player.Orientation.Right:
                return PhraseKey.GetPhrase("position_right");
            case Player.Orientation.Back:
                return PhraseKey.GetPhrase("position_back");
            case Player.Orientation.Left:
                return PhraseKey.GetPhrase("position_left");
            default:
                break;
        }

        return itemPosition;
    }
}