using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Rendering;

public class CombinationLoader : TextParser
{
    public static CombinationLoader Instance;

    int itemIndex = 0;


    private void Awake()
    {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);

        if  ( rowIndex < 1)
        {
            return;
        }

        // actions
        int verbIndex = 0;

        if ( itemIndex >= ItemManager.Instance.dataItems.Count)
        {
            //Debug.LogError("COMBINATION LOADER : item index outside data items list : " + itemIndex);
            return;
        }

        Item item = ItemManager.Instance.dataItems[itemIndex];

        for (int cellIndex = 1; cellIndex < cells.Count; cellIndex++)
        {
            if (verbIndex >= Verb.GetVerbs.Count)
            {
                //Debug.LogError(verbIndex + " / " + Verb.GetVerbs.Count);
                continue;
            }

            Verb verb = Verb.GetVerbs[verbIndex];

            string cell = cells[cellIndex];

            if (cell.Length >= 2)
            {
                // verb out of range
                Combination newCombination = new Combination();
                newCombination.content = cell;
                newCombination.itemIndex = item.dataIndex;

                verb.AddCombination(newCombination);
            }

            ++verbIndex;

        }

        ++itemIndex;
    }
}
