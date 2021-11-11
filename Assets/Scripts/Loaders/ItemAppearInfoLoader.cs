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
            string cell = cells[cellIndex];


            string amount_str = "";

            if ( string.IsNullOrEmpty(cell))
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
                Debug.LogError("Appear Rates : la cellule (" + cell + ") ne peut pas être parsée");
                Debug.LogError("cell : " + GetCellName(rowIndex, cellIndex));
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
            if ( itemIndex < Item.dataItems.Count)
            {
                Item targetItem = Item.dataItems[itemIndex];

                targetItem.appearInfos.Add(newAppearInfo);
            }
            
        }
    }
}
