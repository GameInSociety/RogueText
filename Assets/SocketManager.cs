using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance;

    public List<Socket> itemSockets = new List<Socket>();
    public List<Socket> tileSockets = new List<Socket>();

    public List<Socket> currentSockets= new List<Socket>();


    private void Awake()
    {
        Instance = this;
    }

    public void SetSockets(Socket socket )
    {
        SetSockets(new List<Socket> { socket });
    }

    public void SetSockets(List<Socket> sockets)
    {
        
    }

    public void SetRandomSockets()
    {

    }

    public void DescribeItems(List<Item> items, Socket targetSocket)
    {
        /// mettre les socjets de côté pour l'instnat
        currentSockets.Clear();


        // get item item AND item counts
        foreach (var item in items)
        {
            // A REMETTRE
            // j'ai mis de côté quand j'ai enlevé la classe "ItemPhrase" et "ItemGroup" qui ne servaient à rien
            // mais important et à remettre
            if (item.stackable)
            {

            }

            Socket socket;
            if (item.HasProperty("direction"))
            {
                // pas normal de le créer à chaque fois
                socket = new Socket();
                socket._text = item.GetRelativePosition();
            }
            else if ( targetSocket == null)
            {
                socket = Socket.GetRandomSocket(item);
            }
            else
            {
                socket = targetSocket;
            }
            

            if (currentSockets.Contains(socket))
            {
                // socket already exists
                socket = currentSockets.Find(x => x == socket);
            }
            else
            {

                // didn't find socket, fetching new
                currentSockets.Add(socket);
                socket.itemGroups.Clear();
            }

            // see if the item is already in the socket
            Socket.ItemGroup itemGroup = socket.itemGroups.Find(x => x.item.dataIndex == item.dataIndex);

            if (itemGroup == null)
            {
                // it isn't, so add it
                itemGroup = new Socket.ItemGroup();

                itemGroup.item = item;

                itemGroup.count = 1;

                socket.itemGroups.Add(itemGroup);
            }
            else
            {

                // is already is, so add to the item group count
                itemGroup.count += 1;

            }
        }

        foreach (var socket in currentSockets)
        {
            // reset hide bool
            socket.hide = false;
        }

        WriteDescription();
    }

    // j'ai commencé un truc j'ai pas fin
    public class SocketGroup
    {
        public List <Socket> sockets;
        public List<Item> items;
    }


    void WriteDescription()
    {
        // Socket Group ?
        // Socket Group has items and sockets. 
        // and the phrase writes it self

        // return the phrases
        foreach (var socket in currentSockets)
        {
            if (socket.hide)
            {
                continue;
            }


            Tile tmpTile = socket.GetItem() as Tile;
            if ( tmpTile != null)
            {
                Debug.Log(socket.GetItem().debug_name + " is a tile");
            }
            else
            {
                Debug.Log(socket.GetItem().debug_name + " is an item");

            }

            string phrase = socket.GetSocketText() + " there's " + socket.GetItemsText();

            // same tile, stackable
                // on the right, the forest continues
            // same tile, not stackable
                // on the left, another forest
            // 

            // check if sockets share same item index
            // not same item, but same item index
            Socket tmpSocket = currentSockets.Find(x => x.itemGroups[0].item.SameTypeAs(socket.itemGroups[0].item) && x != socket);
            // if found a socket which is not the current one
            if (tmpSocket != null )
            {
                Debug.Log("socket " + tmpSocket._text + " also has item " + socket.itemGroups[0].item.debug_name);
                phrase = phrase.Insert(0, tmpSocket.GetSocketText() + " and ");
                tmpSocket.hide = true;
                TextManager.WritePhrase(phrase);
                continue;
            }

            TextManager.WritePhrase(phrase);
        }


        
    }

    public Item GetSocketItemInText(string text)
    {
        Socket socket = GetSocketInText(text);

        if ( socket != null)
        {
            return socket.itemGroups[0].item;
        }

        return null;
    }

    public Socket GetSocketInText(string text)
    {
        foreach (var socket in SocketManager.Instance.currentSockets)
        {
            if (text.Contains(socket._text))
            {
                return socket;
            }
        }

        return null;
    }
}
