using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSocket
{
    public int count = 0;

    public Item item;

    public string GetWordGroup()
    {
        if (count > 5)
        {
            return "beaucoup " + item.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Undefined, Word.Preposition.De, Word.Number.Plural);
        }
        else if (count > 3)
        {
            return "quelques " + item.word.GetContent(Word.ContentType.JustWord, Word.Definition.Undefined, Word.Preposition.None, Word.Number.Plural);
        }
        else if (count > 1)
        {
            return count + " " + item.word.GetContent(Word.ContentType.JustWord, Word.Definition.Undefined, Word.Preposition.De, Word.Number.Plural);
        }
        else
        {
            if (item.unique)
            {
                return item.word.GetContent(Word.ContentType.FullGroup, Word.Definition.Undefined, Word.Preposition.None, Word.Number.Singular);
            }
            else
            {
                return item.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Undefined, Word.Preposition.None, Word.Number.Singular);
            }
        }
    }

    public static List<ItemSocket> GetItemSockets(List<Item> items)
    {
        List<ItemSocket> itemSockets = new List<ItemSocket>();

        foreach (var item in items)
        {
            ItemSocket itemSocket = itemSockets.Find(x => x.item.index == item.index && item.stackable);
            //ItemSocket itemSocket = null;

            if (itemSocket == null)
            {
                itemSocket = new ItemSocket();

                itemSocket.item = item;

                itemSocket.count = 1;

                itemSockets.Add(itemSocket);

            }
            else
            {
                itemSocket.count++;
            }
        }

        return itemSockets;
    }

    public static string GetRelativeItemPositionPhrase(string itemName)
    {
        string itemPosition = "";
        char dirChar = itemName[itemName.Length - 2];

        Player.Orientation fac = Player.Orientation.None;

        switch (dirChar)
        {
            case 'n':
                fac = Player.Instance.GetFacing(Direction.North);
                break;
            case 'e':
                fac = Player.Instance.GetFacing(Direction.East);
                break;
            case 's':
                fac = Player.Instance.GetFacing(Direction.South);
                break;
            case 'w':
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