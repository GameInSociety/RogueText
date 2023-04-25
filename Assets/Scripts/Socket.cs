using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

[System.Serializable]
public class Socket
{
    // LOCAL //
    // makes sens that the socket are unique ( no instances )
    // because they will not change, there will be no copy and nothing added to them
    public List<int> itemIndexes = new List<int>();
    public bool relative = false;
    public string _text;

    public List<ItemGroup> itemGroups = new List<ItemGroup>();
    
    public string GetDescription()
    {
        string itemText = GetDescription(itemGroups);

        // phrases de vision ( vous voyez, remarquez etc... )
        string visionPhrase = PhraseKey.GetPhrase("tile_visionPhrases");
        // phrases de location ( se trouve, se tient etc ... )
        string locationVerb = PhraseKey.GetPhrase("tile_locationPhrases");

        // PHRASE ORDER 

        // le type de la phrase ( noms, verbe de vision et position de l'objet
        // , ou position de l'objet, verbe de location et noms
        // etc... )
        int phraseType = Random.Range(0, 5);

        string text = "";

        Item firstItem = itemGroups[0].item;

        switch (phraseType)
        {
            case 0:
                text = itemText + " " + locationVerb + " " + GetSocketText(firstItem);
                break;
            case 1:
                text = GetSocketText(firstItem) + " " + locationVerb + " " + itemText;
                break;
            case 2:
                text = GetSocketText(firstItem) + ", " + visionPhrase + " " + itemText;

                break;
            case 3:
                text = visionPhrase + " " + GetSocketText(firstItem) + " " + itemText;
                break;
            case 4:
                text = GetSocketText(firstItem) + ", " + itemText;
                break;
            default:
                break;
        }

        // mettre la phrase en majuscule
        return TextManager.WithCaps(text);
    }

    public string GetSocketText(Item item)
    {
        if (relative)
        {
            return item.GetRelaivePosition();
        }
        else
        {
            return _text;
        }
    }

    public struct ItemGroup
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
    }

    public static string GetDescription(List<Socket.ItemGroup> itemGroups)
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
    public static List<Socket> sockets = new List<Socket>();

    public static Socket GetRandomSocket(Item item)
    {
        List<Socket> possibleSockets = new List<Socket>();

        foreach (var socket in sockets)
        {
            foreach (var itemIndex in socket.itemIndexes)
            {
                if (itemIndex == item.index)
                {
                    possibleSockets.Add(socket);
                }
            }
        }

        // item has no assigned sockets ?
        if (possibleSockets.Count == 0)
        {
            if (Tile.GetCurrent.tileItem.sockets.Count > 0)
            {
                return Tile.GetCurrent.tileItem.sockets[Random.Range(0, Tile.GetCurrent.tileItem.sockets.Count)];
            }
            // current tile doesnt have sockets
            else
            {
                // choose generic socket
                return PhraseManager.Instance.genericSockets[Random.Range(0, PhraseManager.Instance.genericSockets.Length)];
            }
        }
        else
        {

            int index = Random.Range(0, possibleSockets.Count);

            if (index >= possibleSockets.Count || index < 0)
            {
                Debug.LogError("index out of range : " + index + " / possible sockets count : " + possibleSockets.Count);
            }

            Socket socket = possibleSockets[index];

            return socket;
        }
    }

}
