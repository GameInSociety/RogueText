using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSocketLoader : TextParser
{
    public static ItemSocketLoader Instance;

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
                newSocket.SetPosition(cells[cellIndex]);

                //SocketManager.Instance.itemSockets.Add(newSocket);

                //phrases.Add(cells[cellIndex]);
            }
        }
        else
        {
            int itemIndex = rowIndex - 1;

            if (itemIndex >= ItemManager.Instance.dataItems.Count)
            {
                return;
            }


            int socketIndex = 0;

            for (int cellIndex = 1; cellIndex < cells.Count; cellIndex++)
            {
                if (cells[cellIndex].Length != 0)
                {
                    Item item = ItemManager.Instance.dataItems[itemIndex];

                    //SocketManager.Instance.itemSockets[socketIndex].itemIndexes.Add(item.dataIndex);
                    //Debug.Log("adding item : " + item.word.text + " (index:"+item.dataIndex+") in socket " + Socket.itemSockets[socketIndex]._text);
                }

                socketIndex++;
            }
        }
    }
}
