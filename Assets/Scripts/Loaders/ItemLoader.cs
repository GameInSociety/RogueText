using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemLoader : TextParser {

    // singleton
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

        // debug list to explore items
        items_debug = Item.dataItems.ToArray();
        foreach (var item in items_debug)
        {
            item.debug_name = item.word.text;
        }
        //
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);

        // name row
        if (rowIndex < 1)
        {
            return;
        }

        // skip empty
        if (cells.Count > 0 && string.IsNullOrEmpty(cells[0]))
        {
            return;
        }

        // ? 
        if ( cells[0][0] == '*')
        {
            
        }

        string nameCell = cells[0];
        string[] names = nameCell.Split('\n');
        for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
        {

            // create new item
            Item newItem = new Item();

            string itemText = names[nameIndex];

            // new word
            Word itemWord = new Word();
            itemWord.SetText(itemText);

            // word
            newItem.word = itemWord;
            //newItem.index = rowIndex-1;
            newItem.index = itemIndex;
            itemWord.UpdateNumber(cells[1]);

            // weight
            newItem.word.adjectiveType = cells[2].ToLower();

            if (cells[3].Length > 1)
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
                    newItem.AddProperty(property_line);
                }
            }
            //

            // add to item list
            Item.dataItems.Add(newItem);

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
}