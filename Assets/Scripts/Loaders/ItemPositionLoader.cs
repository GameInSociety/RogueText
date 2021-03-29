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
                phrases.Add(cells[cellIndex]);
            }
        }
        else
        {
            int itemIndex = rowIndex - 1;

            if (itemIndex >= Item.items.Count)
            {
                return;
            }

            Item item = Item.items[itemIndex];

            int phraseIndex = 0;

            for (int cellIndex = 1; cellIndex < cells.Count; cellIndex++)
            {
                if (cells[cellIndex].Length != 0)
                {
                    

                    item.itemPositions.Add(phrases[phraseIndex]);

                    //Debug.Log("adding item position phrase " + phrases[phraseIndex] + " to item : " + item.word.text);
                }

                phraseIndex++;
            }
        }
    }
}
