using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemLoader : TextParser {

    public static ItemLoader Instance;

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

        // create new item
        Item newItem = new Item();

        // new word
        Word itemWord = new Word();

        itemWord.SetText(cells[0]);

        // word
        newItem.word = itemWord;
        newItem.index = rowIndex-1;
        itemWord.UpdateGenre(cells[1]);

        // weight
        newItem.word.adjectiveType = cells[2].ToLower();
        itemWord.SetRandomAdj();

        /*if (cells[2].Length > 0)
        {
            int weight = 1;

            if (int.TryParse(cells[2], out weight) == false)
            {

                //Debug.Log("item weight : " + cells[2] + " does not parse");
            }
            else
            {
            }

            newItem.weight = weight;
        }*/

        if ( cells[3].Length > 1)
        {
            newItem.word.SetLocationPrep(cells[3]);
        }

        newItem.stackable = cells[4] == "stackable";
        
        if (cells[4] == "unique")
        {
            newItem.unique = true;
        }
        else
        {
            newItem.unique = false;
        }

        // value 
        int param = -1;

        if (cells[5].Length > 0)
        {
            if (int.TryParse(cells[5], out param) == false)
            {
                Debug.Log("item parameter : " + cells[5] + " does not parse");
            }
        }
        newItem.value = param;

        Item.items.Add(newItem);

        //Debug.Log("loading item : " + newItem.GetWord());

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

            if (cell .Length >= 2)
            {
                // verb out of range
                Combination newCombination = new Combination();
                newCombination.content = cell;
                newCombination.itemIndex = newItem.index;

                verb.AddCombination(newCombination);

            }

            ++verbIndex;

        }
    }
}