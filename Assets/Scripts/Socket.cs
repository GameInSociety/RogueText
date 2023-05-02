using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor.UIElements;
using UnityEngine;

[System.Serializable]
public class Socket
{
    // LOCAL //
    // makes sens that the socket are unique ( no instances )
    // because they will not change, there will be no copy and nothing added to them
    public string _text;
    public List<int> itemIndexes = new List<int>();
    public bool relative = false;

    public List<ItemGroup> itemGroups = new List<ItemGroup>();

    public string GetDescription()
    {
        string itemText = GetItemText();

        // phrases de vision ( vous voyez, remarquez etc... )
        string visionPhrase = TextManager.GetPhrase("tile_visionPhrases");
        // phrases de location ( se trouve, se tient etc ... )
        string locationVerb = TextManager.GetPhrase("tile_locationPhrases");

        // PHRASE ORDER 

        // le type de la phrase ( noms, verbe de vision et position de l'objet
        // , ou position de l'objet, verbe de location et noms
        // etc... )
        int phraseType = Random.Range(0, 5);

        string text = GetSocketText() + " there's " + itemText;

        /*switch (phraseType)
        {
            case 0:
                text = itemText + " " + locationVerb + " " + GetSocketText();
                break;
            case 1:
                text = GetSocketText() + " " + locationVerb + " " + itemText;
                break;
            case 2:
                text = GetSocketText() + ", " + visionPhrase + " " + itemText;

                break;
            case 3:
                text = visionPhrase + " " + GetSocketText() + " " + itemText;
                break;
            case 4:
                text = GetSocketText() + ", " + itemText;
                break;
            default:
                break;
        }*/

        // mettre la phrase en majuscule
        return TextUtils.FirstLetterCap(text);
    }

    public string GetSocketText()
    {
        if (relative)
        {
            return itemGroups[0].item.GetRelativePosition();
        }
        else
        {
            return _text;
        }
    }

    [System.Serializable]
    public class ItemGroup
    {
        public Item item;
        public int count;
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
                return item.word.GetContent("a dog");
                /*if (!item.stackable)
                {
                    return item.word.GetContent("a good dog");
                }
                else
                {
                    return item.word.GetContent("a dog");
                }*/
            }
        }
    }

    public string GetItemText()
    {
        string text = "";

        int i = 0;

        foreach (var itemGroup in itemGroups)
        {

            text += itemGroup.GetWordGroup();

            if (itemGroups.Count > 1 && i < itemGroups.Count - 1)
            {
                if (itemGroups.Count > 2)
                {
                    if (i == itemGroups.Count - 2)
                    {
                        text += " and ";
                    }
                    else
                    {
                        text += ", ";
                    }
                }
                else
                {
                    text += " and ";
                }

            }

            ++i;
        }

        return text;
    }

    // STATIC //

    public static Socket GetRandomSocket(Item item)
    {
        List<Socket> possibleSockets = new List<Socket>();

        // has the item a socket ?
        foreach (var socket in SocketManager.Instance.itemSockets)
        {
            foreach (var itemIndex in socket.itemIndexes)
            {
                if (itemIndex == item.dataIndex)
                {
                    possibleSockets.Add(socket);
                }
            }
        }

        // does the tile have any socket ?
        if (possibleSockets.Count == 0)
        {
            foreach (var socket in SocketManager.Instance.tileSockets)
            {
                foreach (var itemIndex in socket.itemIndexes)
                {
                    if (itemIndex == Tile.GetCurrent.dataIndex)
                    {
                        possibleSockets.Add(socket);
                    }
                }
            }
        }

        if ( possibleSockets.Count == 0)
        {

            return PhraseManager.Instance.genericSockets[Random.Range(0, PhraseManager.Instance.genericSockets.Length)];
        }

        return possibleSockets[Random.Range(0, possibleSockets.Count)];
    }

}
