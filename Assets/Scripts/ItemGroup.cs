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
            return "beaucoup " + item.word.GetContent("de chiens");
        }
        else if (count > 3)
        {
            return "quelques " + item.word.GetContent("chiens");
        }
        else if (count > 1)
        {
            return count + " " + item.word.GetContent("chiens");
        }
        else
        {
            if (!item.stackable)
            {
                return item.word.GetContent("un chien sage");
            }
            else
            {
                return item.word.GetContent("un chien");
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

        switch (item.GetProperty("direction").GetValue())
        {
            case "to north":
                fac = Player.Instance.GetFacing(Direction.North);
                break;
            case "to south":
                fac = Player.Instance.GetFacing(Direction.South);
                break;
            case "to east":
                fac = Player.Instance.GetFacing(Direction.East);
                break;
            case "to west":
                fac = Player.Instance.GetFacing(Direction.West);
                break;
            default:
                break;
        }

        switch (fac)
        {
            case Player.Orientation.Front:
                itemPosition = "devant vous";
                break;
            case Player.Orientation.Right:
                itemPosition = "à droite";
                break;
            case Player.Orientation.Back:
                itemPosition = "derrière vous";
                break;
            case Player.Orientation.Left:
                itemPosition = "à gauche";
                break;
            default:
                break;
        }

        return itemPosition;
    }
}