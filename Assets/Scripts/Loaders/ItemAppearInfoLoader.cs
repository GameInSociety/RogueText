using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAppearInfoLoader : TextParser
{
    public static ItemAppearInfoLoader Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);

        if ( cells.Count == 0)
        {
            return;
        }

        if (rowIndex < 1)
        {
            return;
        }

        if (string.IsNullOrEmpty(cells[0]))
        {
            return;
        }

        int itemToAddIndex = rowIndex - 1;

        if ( itemToAddIndex >= Item.dataItems.Count)
        {
            Debug.LogError("fuck :" +
                "bug de quand la longeur de la liste d'objets et des autres listes ne sont pas les memes" +
                "il y a peut être un trou dans une des listes ( un trou est considéré comme une case vide /nulle donc raccourci la liste )");
            return;
        }

        Item itemToAdd = Item.dataItems[itemToAddIndex];

        if ( cells[1] == "TRUE")
        {
            itemToAdd.usableAnytime = true;  
            return;
        }

        for (int cellIndex = 2; cellIndex < cells.Count; cellIndex++)
        {
            string cell_str = cells[cellIndex];
            string rate_str;

            if ( string.IsNullOrEmpty(cell_str))
            {
                continue;
            }

            string amount_str = string.Empty;

            Item.AppearInfo newAppearInfo = new Item.AppearInfo();

            // id 
            newAppearInfo.itemIndex = itemToAddIndex;


            // amount
            newAppearInfo.amount = 1; // default appear amount
            if (cell_str.Contains("*"))
            {
                rate_str = cell_str.Split('*')[0];
                amount_str = cell_str.Split('*')[1];

                if (string.IsNullOrEmpty(amount_str))
                {
                    int amount = 0;
                    if (!int.TryParse(amount_str, out amount))
                    {
                        Debug.LogError("couldn't parse amount (" + amount_str + ") in cell " + GetCellName(rowIndex, cellIndex));
                    }
                    else
                    {
                        newAppearInfo.amount = amount;
                    }
                }
            }
            else
            {
                rate_str = cell_str;
            }

            // rate
            int appearRate = 0;
            if (!int.TryParse(rate_str, out appearRate))
            {
                Debug.LogError("Appear Rates : la cellule (" + rate_str + ") ne peut pas être parsée");
                Debug.LogError("cell : " + GetCellName(rowIndex, cellIndex));
            }

            newAppearInfo.rate = appearRate;

            // add to item 
            int itemIndex = cellIndex - 2;
            if ( itemIndex < Item.dataItems.Count)
            {
                Item targetItem = Item.dataItems[itemIndex];

                //Debug.Log(newAppearInfo.rate + " to find " + Item.dataItems[newAppearInfo.itemIndex].debug_name + " in " + targetItem.debug_name);
                targetItem.appearInfos.Add(newAppearInfo);
            }
            
        }
    }
}
