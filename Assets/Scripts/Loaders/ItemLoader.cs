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

        items_debug = Item.dataItems.ToArray();

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

                string[] propertyLine_parts = property_line.Split('/');


                Item.Property.Type type = (Item.Property.Type)System.Enum.Parse(typeof(Item.Property.Type), propertyLine_parts[0]);
                newProperty.type = type;

                newProperty.name = propertyLine_parts[1];

                newProperty.SetContent(propertyLine_parts[2]);

                newProperty.param = propertyLine_parts[3];

                /// s'il y avait plusieurs parametres ///
                /*for (int i = 2; i < propertyLine_parts.Length; i++)
                {
                    string param = propertyLine_parts[i];
                    Debug.Log("adding param : " + param + " to item " + newItem.word.text);
                    newProperty.parameters.Add(param);
                }*/

                // !!! reel probleme ici, pourquoi mes trucs se mettent en copie ? !!!
                // pas sûr que ce soit d'actualité à vérifier
                newItem.AddProperty(newProperty);
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