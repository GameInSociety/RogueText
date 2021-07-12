using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionsInItemLoader : TextParser
{
    public static PositionsInItemLoader Instance;

    public List<string> phrases = new List<string>();
    List<Socket> tmpSockets = new List<Socket>();

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
                newSocket._text = cells[cellIndex];
                tmpSockets.Add(newSocket);
            }
        }
        else
        {
            int itemIndex = rowIndex - 1;

            int socketIndex = 0;

            for (int cellIndex = 1; cellIndex < cells.Count; cellIndex++)
            {
                if ( cells[cellIndex].Length != 0)
                {
                    if ( itemIndex >= Item.items.Count)
                    {
                        continue;
                    }

                    Item item = Item.items[itemIndex];
                    Socket socket = tmpSockets[socketIndex];

                    item.sockets.Add(socket);
                }

                socketIndex++;
            }
        }
    }
}
