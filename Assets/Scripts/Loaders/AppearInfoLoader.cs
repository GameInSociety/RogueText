using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AppearInfoLoader : TextParser
{
    public static AppearInfoLoader Instance;

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

        if (rowIndex < 2)
        {
            return;
        }

        if (string.IsNullOrEmpty(cells[0]))
        {
            return;
        }

        AppearInfo appearInfo = new AppearInfo();
        appearInfo.name = cells[0];

        ItemManager.Instance.appearInfos.Add(appearInfo);

        if (cells[1] == "TRUE")
        {
            appearInfo.usableAnytime = true;
            return;
        }

        for (int i = 2; i < cells.Count; i++)
        {

            string cell = cells[i];

            if (string.IsNullOrEmpty(cell))
            {
                break;
            }

            string[] lines = cell.Split(new char[] { '\n' });
            foreach (var line in lines)
            {
                AppearInfo.ItemInfo itemInfo = new AppearInfo.ItemInfo();

                string[] parts = line.Split(", ");
                string itemName = parts[0];
                Item targetItem = ItemManager.Instance.GetDataItem(itemName);
                itemInfo.itemIndex = targetItem.dataIndex;
                itemInfo.name = targetItem.debug_name;

                itemInfo.chanceAppear = 100;

                if ( parts.Length > 1 )
                {
                    // chance appear

                    string chanceAppear_str = parts[1].Remove(parts[1].Length - 1);
                    if (!int.TryParse(chanceAppear_str, out itemInfo.chanceAppear))
                    {
                        Debug.LogError("APPEAR INFO LOAD : couldn't parse chance appear : " + chanceAppear_str);
                    }
                }

                itemInfo.amount = 1;

                if (parts.Length > 2)
                {
                    string amount_str = parts[2].Remove(0, 1);
                    // amount
                    if (!int.TryParse(amount_str, out itemInfo.amount))
                    {
                        Debug.LogError("APPEAR INFO LOAD : couldn't parse amount : " + amount_str);
                    }
                }

                appearInfo.itemInfos.Add(itemInfo);


            }
        }
    }
}
