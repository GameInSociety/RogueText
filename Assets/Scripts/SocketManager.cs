using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance;

    public List<Socket> itemSockets = new List<Socket>();
    public List<Socket> tileSockets = new List<Socket>();



    public List<SocketGroup> socketGroups = new List<SocketGroup>();
    [System.Serializable]
    public class SocketGroup
    {
        public List<Socket> sockets = new List<Socket>();
    }


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
        SocketGroup socketGroup = new SocketGroup();

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
                // look for socket that share the same item, to put them in the same socket
                // only for direction now, because would also be like "on the counter and on the floor, 3 apples" which doesnt make sense
                socket = socketGroup.sockets.Find(x => x.GetItem().SameTypeAs(item));

                // if found a socket which is not the current one
                if (socket != null)
                {
                    socket.AddPosition(item.GetRelativePosition());
                    socket.AddItem(item, true);
                    continue;
                }

                socket = new Socket();
                socket.SetPosition(item.GetRelativePosition());
            }
            else if ( targetSocket == null)
            {
                socket = GetRandomSocket(item);
            }
            else
            {
                socket = targetSocket;
            }
            

            if (socketGroup.sockets.Contains(socket))
            {
                // socket already exists
                socket = socketGroup.sockets.Find(x => x == socket);
            }
            else
            {

                // didn't find socket, fetching new
                socketGroup.sockets.Add(socket);
                socket.itemGroups.Clear();
            }

            socket.AddItem(item);
        }


        // WRITE DESCRIPTION
        foreach (var socket in socketGroup.sockets)
        {
            KeyWords.socket = socket;
            TextManager.WritePhrase(GetTextType(socket));
        }

        socketGroups.Add(socketGroup);
    }


    void WriteDescription()
    {
        // Socket Group ?
        // Socket Group has items and sockets. 
        // and the phrase writes it self

        // return the phrases
        
        
    }
    string GetTextType(Socket socket)
    {
        Item targetItem = socket.GetItem();

        Tile tmpTile = targetItem as Tile;

        if (tmpTile != null)
        {

            if (Tile.GetCurrent.SameTypeAs(targetItem))
            {
                if (Tile.GetCurrent.stackable)
                {
                    // tu es dans une forêt, la forêt continue
                    return "socket_tile_continue";
                }
                else
                {
                    if (Interior.InsideInterior())
                    {

                        // tu es dans la cuisine, et tu vois LE couloir ( dans un intérieur, les articles définis ont plus de sens )
                        return "socket_tile_visited";
                    }
                    else
                    {
                        // tu es près d'une maison, tu vois une maison que tu connais pas
                        return "socket_tile_discover";
                    }
                }
            }
            else
            {
                // ici
                if (((Tile)targetItem).used)
                {
                    // tu vois es près d'une maison
                    return "socket_tile_visited";
                }
                else
                {
                    return "socket_tile_discover";
                }
            }

        }
        else
        {
            return "socket_default";
        }

        
    }


    // STATIC //
    public Socket GetRandomSocket(Item item)
    {
        List<Socket> possibleSockets = new List<Socket>();

        // has the item a socket ?
        foreach (var socket in itemSockets)
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
            foreach (var socket in tileSockets)
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

        Socket tmpSocket;

        if (possibleSockets.Count == 0)
        {
            tmpSocket = new Socket();
            tmpSocket.SetPosition("&on the dog (tile)&");

            return tmpSocket;
        }



        tmpSocket = possibleSockets[Random.Range(0, possibleSockets.Count)];


        return tmpSocket;
    }

    public Item GetSocketItemInText(string text)
    {
        foreach (var socketGroup in socketGroups)
        {
            foreach (var socket in socketGroup.sockets)
            {
                for (int i = 0; i < socket._positions.Count; i++)
                {
                    if (text.Contains(socket._positions[i]))
                    {
                        return socket.GetItem(i);
                    }
                }

            }
        }

        return null;
    }

}
