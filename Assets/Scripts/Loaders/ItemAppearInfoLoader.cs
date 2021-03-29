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

        if (rowIndex < 1)
        {
            return;
        }

        int itemToAddIndex = rowIndex - 1;

        if ( itemToAddIndex >= Item.items.Count)
        {
            Debug.Log(" item index : " +itemToAddIndex + " out of item range : " + Item.items.Count );
            return;
        }


        Item itemToAdd = Item.items[itemToAddIndex];

        if ( cells[1] == "TRUE")
        {
            itemToAdd.usableAnytime = true;  
            return;
        }

        for (int cellIndex = 2; cellIndex < cells.Count; cellIndex++)
        {
            string cell = cells[cellIndex];

            string amount_str = "";

            if ( cell.Length == 0)
            {
                continue;
            }

            if (cell.Contains("*"))
            {
                amount_str = cell.Split('*')[0];
                cell = cell.Split('*')[1];
            }

            int appearRate = 0;
            if (!int.TryParse(cell, out appearRate))
            {
                Debug.LogError("Appear Rates : la cellule : " + cell + " ne peut pas être parsée");
            }

            Item.AppearInfo newAppearInfo = new Item.AppearInfo();

            // rate
            newAppearInfo.rate = appearRate;

            // id 
            newAppearInfo.itemIndex = itemToAddIndex;

            // amount
            if (amount_str.Length > 0)
            {
                int a = int.Parse(amount_str);

                newAppearInfo.amount = a;
            }
            else
            {
                newAppearInfo.amount = 1;
            }

            int itemIndex = cellIndex - 2;
            Item targetItem = Item.items[itemIndex];

            targetItem.appearInfos.Add(newAppearInfo);

            //Debug.Log("adding : " + Item.items[newAppearInfo.itemIndex].GetWord() + " to tile " + targetItem.GetWord());
        }
    }
}
