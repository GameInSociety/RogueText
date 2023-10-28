using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemGroup {
    public ItemGroup(int index){
        this.index = index; 
    }
    public int index;
    public List<Item> items = new List<Item>();

    public bool itemsAreDistinct(){
        // check if player asked for a specific number of items
        int numericValueInInput = 1;
        if ( numericValueInInput > 0) {
            Debug.Log($"item {items[0].debug_name} with {numericValueInInput} in input");
            // don't isolate, keep X itms in the list
            numericValueInInput = Mathf.Clamp(numericValueInInput, 0, items.Count);
            items.RemoveAt(numericValueInInput);
            return true;
        }

        // check if the item was plural in input
        if (items[0].numInInput == Word.Number.Plural) {
            Debug.Log($"item {items[0].debug_name} was plural in input (without numeric value), so keep all of them");
            return true;
        }

        // this means a specific item is required, so searching a spec
        Item specificItem = getSpecific();
        if (specificItem != null){
            isolateItem(specificItem);
            return true;
        }

        return false;
    }

    Item getSpecific() {
        // try to find an item spec in the input
        assignOrdinates();
        Spec spec = null;
        string text = ItemParser.GetCurrent.lastInput;
        return items.Find(x => x.textHasSpecs(text, out spec));
    }
    private void assignOrdinates() {
        for (int i = 0; i < items.Count; i++) {
            string ordinal = GetOrdinal(i);
            Spec ordinalSpec = items[i].getKeyInfo("ordinal");
            if (ordinalSpec != null) {
                ordinalSpec.searchValue = ordinal;
                ordinalSpec.displayValue = ordinal;
            } else
                items[i].setSpec(ordinal, ordinal, "ordinal");
        }
    }

    public string GetOrdinal(int i) {
        var ordinals = new string[10]
        {
            "first",
            "second",
            "third",
            "fourth",
            "fifth",
            "sixth",
            "seventh",
            "eighth",
            "ninth",
            "tenth",
        };
        return ordinals[i];
    }

    public void isolateItem(Item item) {
        items.RemoveAll(x => x != item);
    }
}