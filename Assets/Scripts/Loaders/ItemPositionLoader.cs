using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPositionLoader : TextParser
{
    public static ItemPositionLoader Instance;

    public List<string> phrases = new List<string>();

    private void Awake()
    {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);

        if (rowIndex == 0)
        {
            for (int cellIndex = 1; cellIndex < cells.Count; cellIndex++)
            {
                Socket newSocket = new Socket();
                if (cells[cellIndex].StartsWith("relative"))
                {
                    newSocket.relative = true;
                }
                else
                {
                    newSocket._text = cells[cellIndex];
                }

                Socket.sockets.Add(newSocket);

                //phrases.Add(cells[cellIndex]);
            }
        }
        else
        {
            int itemIndex = rowIndex - 1;

            if (itemIndex >= Item.dataItems.Count)
            {
                return;
            }


            int socketIndex = 0;

            for (int cellIndex = 1; cellIndex < cells.Count; cellIndex++)
            {
                if (cells[cellIndex].Length != 0)
                {
                    Item item = Item.dataItems[itemIndex];

                    Socket.sockets[socketIndex].itemIndexes.Add(item.index);
                    //Debug.Log("adding item : " + item.word.text + " (index:"+item.index+") in socket " + Socket.sockets[socketIndex]._text);
                }

                socketIndex++;
            }
        }
    }
}
