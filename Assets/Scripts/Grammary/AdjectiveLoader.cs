using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjectiveLoader : TextParser {

    public string[] maleTerminaisons;
    public string[] femaleTerminaisons;

    public static AdjectiveLoader Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);

        if ( rowIndex == 0)
        {
            foreach (var cell in cells)
            {
                AdjectiveGroup adjectiveGroup = new AdjectiveGroup();
                adjectiveGroup.name = cell.ToLower();

                Adjective.adjectiveGroups.Add(adjectiveGroup);
            }
        }
        else
        {
            for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                Adjective newAdjective = new Adjective();

                newAdjective._text = cells[cellIndex];
                if (newAdjective._text.Contains("("))
                {
                    newAdjective.beforeWord = true;
                    newAdjective._text = newAdjective._text.Replace("(", "");
                }

                Adjective.adjectiveGroups[cellIndex].adjectives.Add(newAdjective);
            }
        }
    }
}
