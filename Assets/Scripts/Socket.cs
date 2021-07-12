using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Socket
{
    // STATIC //
    public static List<Socket> sockets = new List<Socket>();

    public static Socket GetRandomSocket ( Item item)
    {
        List<Socket> possibleSockets = new List<Socket>();

        foreach (var socket in sockets)
        {
            foreach (var itemIndex in socket.itemIndexes)
            {
                if ( itemIndex == item.index)
                {
                    possibleSockets.Add(socket);
                }
            }
        }

        // item has no assigned sockets ?
        if ( possibleSockets.Count == 0)
        {
            if (Tile.current.tileItem.sockets.Count > 0)
            {
                return Tile.current.tileItem.sockets[Random.Range(0, Tile.current.tileItem.sockets.Count)];
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

            if ( index >= possibleSockets.Count || index < 0)
            {
                Debug.LogError("index out of range : " + index + " / possible sockets count : " + possibleSockets.Count);
            }

            Socket socket = possibleSockets[index];

            return socket;
        }
    }

    // LOCAL //

    public List<int> itemIndexes = new List<int>();
    public bool relative = false;
    public string _text;
    
    public string GetText(Item item)
    {
        if (relative)
        {
            return GetRelativeText(item);
        }
        else
        {
            return _text;
        }
    }

    public string GetRelativeText(Item item)
    {
        Debug.Log("GETTING RELATIVE POSITION");
        string text = ItemGroup.GetRelativeItemPositionPhrase(item.word.text);
        Debug.LogError("text à probleme ? " + text );

        return text;
    }

}
