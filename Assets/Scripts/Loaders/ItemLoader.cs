using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemLoader : TextParser {

    public static ItemLoader Instance;

    int itemIndex = 0;

    public Item[] items_debug;

    private void Awake()
    {
        Instance = this;
    }

    public override void FinishLoading()
    {
        base.FinishLoading();

        items_debug = Item.items.ToArray();

        foreach (var item in items_debug)
        {
            item.debug_name = item.word.text;
        }
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);

        if (rowIndex < 1)
        {
            return;
        }

        if (cells.Count > 0 && string.IsNullOrEmpty(cells[0]))
        {
            return;
        }

        // create new item
        Item newItem = new Item();

        // new word
        Word itemWord = new Word();

        itemWord.SetText(cells[0]);

        // word
        newItem.word = itemWord;
        //newItem.index = rowIndex-1;
        newItem.index = itemIndex;
        itemWord.UpdateGenre(cells[1]);

        // weight
        newItem.word.adjectiveType = cells[2].ToLower();

        if ( cells[3].Length > 1)
        {
            newItem.word.SetLocationPrep(cells[3]);
        }

        newItem.stackable = cells[4] == "TRUE";

        // property 
        string[] property_lines = cells[5].Split('\n');

        if (string.IsNullOrEmpty(property_lines[0]))
        {

        }
        else
        {
            foreach (var property_line in property_lines)
            {
                Item.Property newProperty = new Item.Property();

                string[] propertyLine_parts = property_line.Split(':');

                newProperty.name = propertyLine_parts[0];

                newProperty.SetValue(propertyLine_parts[1]);

                if (propertyLine_parts.Length > 2)
                {
                    string part = propertyLine_parts[2];
                    newProperty.param = part;
                }

                // !!! reel probleme ici, pourquoi mes trucs se mettent en copie ? !!!
                newItem.AddProperty(newProperty);
            }
        }
        //

        // add to item list
        Item.items.Add(newItem);

        // actions
        int verbIndex = 0;

        for (int cellIndex = 6; cellIndex < cells.Count; cellIndex++)
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
                newCombination.itemIndex = newItem.index;

                verb.AddCombination(newCombination);

            }

            ++verbIndex;

        }

        ++itemIndex;
    }
}