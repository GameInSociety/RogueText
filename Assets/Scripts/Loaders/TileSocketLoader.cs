using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSocketLoader : TextParser
{
    public static TileSocketLoader Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);


        if ( rowIndex == 0)
        {
            for (int cellIndex = 1; cellIndex < cells.Count; cellIndex++)
            {
                Socket newSocket = new Socket();
                newSocket.SetPosition(cells[cellIndex]);
                SocketManager.Instance.tileSockets.Add(newSocket);
            }
        }
        else
        {
            int itemIndex = rowIndex - 1;

            int tileSocketIndex = 0;

            for (int cellIndex = 1; cellIndex < cells.Count; cellIndex++)
            {
                if ( cells[cellIndex].Length != 0)
                {
                    if ( itemIndex >= ItemManager.Instance.dataItems.Count)
                    {
                        continue;
                    }

                    Item item = ItemManager.Instance.dataItems[itemIndex];
                    Socket socket = SocketManager.Instance.tileSockets[tileSocketIndex];
                    socket.itemIndexes.Add(itemIndex);
                }

                tileSocketIndex++;
            }
        }
    }
}
