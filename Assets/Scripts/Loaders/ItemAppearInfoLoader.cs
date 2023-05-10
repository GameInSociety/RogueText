using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAppearInfoLoader : TextParser
{
    public static ItemAppearInfoLoader Instance;

    public bool debug = false;

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
            for (int i = 2; i < cells.Count; i++)
            {
                AppearInfo appearInfo = new AppearInfo();
                appearInfo.name = cells[i];

                ItemManager.Instance.appearInfos.Add(appearInfo);
            }
            return;
        }

        if (string.IsNullOrEmpty(cells[0]))
        {
            return;
        }

        int itemToAddIndex = rowIndex - 1;

        if ( itemToAddIndex >= ItemManager.Instance.dataItems.Count)
        {
            Debug.LogError("fuck :" +
                "bug de quand la longeur de la liste d'objets et des autres listes ne sont pas les memes" +
                "il y a peut être un trou dans une des listes ( un trou est considéré comme une case vide /nulle donc raccourci la liste )");
            return;
        }

        Item itemToAdd = ItemManager.Instance.dataItems[itemToAddIndex];

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

            int itemIndex = cellIndex - 2;
            AppearInfo appearInfo = AppearInfo.GetAppearInfo(itemIndex);


            AppearInfo.ItemInfo itemInfo = new AppearInfo.ItemInfo();

            itemInfo.name = itemToAdd.debug_name;
            // id 
            itemInfo.itemIndex = itemToAddIndex;


            // amount
            string amount_str = string.Empty;
            itemInfo.amount = 1; // default appear amount
            if (cell_str.Contains("*"))
            {
                rate_str = cell_str.Split('*')[0];
                amount_str = cell_str.Split('*')[1];

                int amount = 0;
                if (!int.TryParse(amount_str, out amount))
                {
                    Debug.LogError("couldn't parse amount (" + amount_str + ") in cell " + GetCellName(rowIndex, cellIndex));
                }
                else
                {
                    itemInfo.amount = amount;
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

            itemInfo.chanceAppear = appearRate;

            // add to item 
            appearInfo.itemInfos.Add(itemInfo);

            if (debug)
            {
                Debug.Log(itemInfo.chanceAppear + " chance to find " + ItemManager.Instance.dataItems[itemInfo.itemIndex].debug_name + " in " + ItemManager.Instance.dataItems[itemIndex].debug_name);

            }

        }
    }
}
