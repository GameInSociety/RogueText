using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using static UnityEditor.Progress;
using UnityEngine.UIElements;

[System.Serializable]
public class ItemParser {

    // text entered by the player
    public List<string> inputs= new List<string>();

    // main prms
    public List<Item> potentialItems = new List<Item>();
    public List<Verb> verbs = new List<Verb>();
    public Verb firstVerb => verbs[0];
    public Item firstItem => potentialItems[0];
    public int numericValueInInput;

    public Item parentItem;
    public enum Step {
        none,
        waitingForVerb,
        waitingForItem,
        waitingForSpecificItem,
    }
    public Step step;

    // history
    public List<ItemKey> itemHistory = new List<ItemKey>();


    public string mainInput => inputs[0];
    public string lastInput => inputs[inputs.Count - 1];

    public Item mainItem() {
        return potentialItems.First();
    }

    public void Parse(string _text) {

        // assigning text
        inputs.Add(_text);

        fetchVerbs();
        fetchItems();
        fetchNumericValues();

        foreach (var item in potentialItems) {
            if ( item.numInInput == Word.Number.Plural) {
                Debug.Log(item.debug_name + " is plural");
            }
        }

        // sending feedback if no verbs or item have been detexted
        if (!inputHasVerbAndItems())
            return;
        if (!inputHasDistinctItems())
            return;

        // all items and vebs have been found and specified
        // and triggering the function
        FunctionSequence.TrySequence();
    }

    void fetchVerbs() {
        verbs = Verb.verbs.FindAll(x => x.getIndexInText(mainInput) >= 0);
    }
    void fetchItems() {
        AvailableItems.update();
        potentialItems.AddRange(AvailableItems.Get.list.FindAll(x => x.getIndexInText(mainInput, Word.Number.Singular) >= 0));
        potentialItems.AddRange(AvailableItems.Get.list.FindAll(x => x.getIndexInText(mainInput, Word.Number.Plural) >= 0));
        potentialItems.OrderByDescending(x => x.indexInInput).ToList();
    }

    bool inputHasDistinctItems() {
        List<Item> similarItems = tryGetSimilarItems();
        if (similarItems != null) {
            if ( numericValueInInput > 0) {
                Debug.Log($"item {similarItems[0].debug_name} with {numericValueInInput} in input");
                // don't isolate, keep X items in the list
                numericValueInInput = Mathf.Clamp(numericValueInInput, 0, similarItems.Count);
                similarItems.RemoveAt(numericValueInInput);
                return true;
            }

            if (similarItems[0].numInInput == Word.Number.Plural) {
                Debug.Log($"item {similarItems[0].debug_name} was plural in input (without numeric value), so keep all of them");
                return true;
            }

            Item specificItem = getSpecificFromList(similarItems);
            if (specificItem == null) {
                if (step == Step.waitingForSpecificItem) {
                    TextManager.write($"there is no such {similarItems[0].debug_name} present");
                    clearParser();
                } else {
                    step = Step.waitingForSpecificItem;
                    TextManager.write($"which {similarItems[0].getWord("dog")} would you like to {firstVerb.GetFull}");
                }
                return false;
            }

            // if an item has ben reseting the step for other items specifications
            step = Step.none;
            isolateItem(specificItem);

            List<Item> list2 = tryGetSimilarItems();
            if ( list2 != null) {
                TextManager.write($"which {list2[0].getWord("dog")} would you like to {firstVerb.GetFull}", list2[0]);
                Debug.Log("input still has similar items");
                return false;
            }

            return true;
        }

        return true;
    }

    List<Item> tryGetSimilarItems() {
        foreach (var item in potentialItems) {
            List<Item> similarItems = potentialItems.FindAll(x => x.dataIndex == item.dataIndex);


            if (similarItems.Count > 1) {
                if (!similarItems.TrueForAll(x => x.specMatch(similarItems.First())))
                    return similarItems;
            }
        }
        return null;
    }

    public void isolateItem(Item item) {
        potentialItems.RemoveAll(x => x.dataIndex == item.dataIndex);
        potentialItems.Add(item);
    }
    Item getSpecificFromList(List<Item> targetItems) {
        // try to find an item spec in the input
        assignOrdinates(targetItems);
        Spec spec = null;
        return targetItems.Find(x => x.textHasSpecs(lastInput, out spec));
    }

    private void assignOrdinates(List<Item> list) {
        for (int i = 0; i < list.Count; i++) {
            string ordinal = GetOrdinal(i);
            Spec ordinalSpec = list[i].getKeyInfo("ordinal");
            if (ordinalSpec != null) {
                ordinalSpec.searchValue = ordinal;
                ordinalSpec.displayValue = ordinal;
            } else {
                list[i].setSpec(ordinal, ordinal, "ordinal");
            }
        }
    }

    public bool inputHasVerbAndItems() {
        if (verbs.Count == 0) {
            Debug.Log("no verbs");
            // no verbs, but item
            if (potentialItems.Count > 0) {
                // checking if the input is ALREADY waiting for a verb
                if (step == Step.waitingForVerb) {
                    TextManager.write($"you can't do that with {potentialItems[0].getWord("the dog")}");
                    clearParser();
                } else {
                    TextManager.write($"what do you want to do with {potentialItems[0].getWord("the dog")}");
                    step = Step.waitingForVerb;
                }
            }
            // no verbs or item
            else {
                TextManager.write("!no verb or item");
                clearParser();
            }
            return false;
        } else if (potentialItems.Count == 0) {
            // checking if the verb is autonomus (dig, eat, sleep etc...)
            var verbItem = Item.GetDataItem("no item");
            if (verbs[0].HasCell(verbItem)) {
                potentialItems.Add(verbItem);
                Debug.Log("found general universal for verb");
                return true;
            }

            // no items, but verb
            if (step == Step.waitingForItem) {
                TextManager.write($"... i don't understand want you want to {firstVerb.getWord} {firstVerb.GetPreposition}");
                clearParser();
            } else {
                TextManager.write($"what do you want to {firstVerb.getWord} {firstVerb.GetPreposition}");
                step = Step.waitingForItem;
            }
            return false;
        }
        return true;
    }
    public bool inputContains(string itemName) {
        return potentialItems.Find(x => x.debug_name == itemName) != null;
    }
    public bool allItemsAreIdentical() {
        return potentialItems.TrueForAll(x =>
        // data index is the same
        x.dataIndex == potentialItems.First().dataIndex
        &&
        // item has no specs ( no differenciation )
        !x.HasInfo("dif"));
    }

    /// <summary>
    /// FOR GETTING ITEMS INSIDE A FUNCTION & IN THE WORLD
    /// </summary>
    #region SEARCH
    public enum SearchType {
        First,
        Highest,
        Random,
        Input,
    }
    public enum MatchType {
        Input,
        Info,
        Property,
    }
    public Property SearchPropertyOfItemInInput(string content) {
        string propertyName;
        var item = SearchItemInInput(content, out propertyName);
        if (item == null) {
            Debug.LogError("no property for : " + content);
            return null;
        }
        return item.GetProperty(propertyName);
    }
    public Item SearchItemInInput(string key) {
        return SearchItemInInput(key, out _);
    }
    public Item SearchItemInInput(string key, out string modifiedKey) {

        // the key is the target item or property

        Logue.New("search", "[SEARCH] : " + key, Color.magenta);

        // check if parent item
        // this means the parser will search INSIDE another item
        // ex: player/body part, this will search a body part in the player
        // instead of in all the available items
        if (key.Contains('/')) {
            var parts = key.Split(new char[] { '/' });
            parentItem = SearchItemInInput(parts[0]);
            key = parts[1];
        }

        // get type if not default
        var type = SearchType.First;
        if (key.Contains('.')) {
            var parts = key.Split(new char[] { '.' });
            type = GetType(parts[0]);
            key = parts[1];
        }

        var items = getPotentialItems(key);

        if (items.Count == 0) {
            Logue.Add("no results with key " + key + " in available items");
            modifiedKey = "error";
            return null;
        }

        // before sorting by type, check if the item's name appears in input
        var item = items.Find(x => x.containedInText(lastInput));

        if (item != null) {
            type = SearchType.Input;
            goto Result;
        }

        // if no results, sort by type
        switch (type) {
            case SearchType.First:
                item = items.First();
                break;
            case SearchType.Highest:

                var highestProp = items.First().GetProperty(key);
                var i = 0;
                for (var itIndex = 1; itIndex < items.Count; itIndex++) {
                    var it = items[itIndex];
                    var prop = it.GetProperty(key);

                    if (prop == null) {
                        Debug.LogError("no property " + key + " in " + it.debug_name);
                        break;
                    }

                    if (prop.GetInt() > highestProp.GetInt()) {
                        i = itIndex;
                        highestProp = prop;
                    }
                }

                item = items[i];
                break;
            case SearchType.Random:
                item = items[UnityEngine.Random.Range(0, items.Count)];
                break;
            default:
                item = items[0];
                break;
        }

        Result:
        itemHistory.Add(new ItemKey(key, item));

        Logue.Add($"[{type}]{key} : {item.debug_name}");
        modifiedKey = key;
        return item;
    }

    static SearchType GetType(string str) {
        switch (str) {
            case "h":
                return SearchType.Highest;
            case "r":
                return SearchType.Random;
            default:
                return SearchType.First;
        }
    }

    private List<Item> getPotentialItems(string content) {
        // first search in history
        // IMPORTANT for target ... and target/body part
        // so that the target is the same and not searched twice
        var key = itemHistory.Find(x => x.key == content);

        if (key != null) {
            Logue.Add($"[HISTORY] : {key.item.debug_name}");
            return new List<Item> { key.item };
        }

        var matches = new List<Predicate<Item>>()
        {
            x => x.debug_name == content,
            x => x.HasInfo(content),
            x => x.hasProperty(content)
        };

        var list = new List<Item>();
        for (var matchIndex = 0; matchIndex < 3; matchIndex++) {
            var matchType = (MatchType)matchIndex;
            var match = matches[matchIndex];
            list = getPotentialItems(content, match);

            if (list != null && list.Count > 0) {
                Logue.Add($"[{matchType}] : " + TextManager.listItems(list));
                parentItem = null;
                return list;
            }
        }

        parentItem = null;
        Logue.Add($"[NO MATCH]{key}");
        return list;
    }

    private List<Item> getPotentialItems(string content, System.Predicate<Item> match) {

        AvailableItems.update();

        var tmp_items = new List<Item>();

        // search for item in pending items
        if (parentItem != null) {
            Logue.Add("searching in " + parentItem.debug_name);
            tmp_items.AddRange(parentItem.getRecursive(3).FindAll(match));
            return tmp_items;
        }

        // add available items
        tmp_items.AddRange(AvailableItems.Get.list.FindAll(match));
        if (tmp_items.Count > 0) {
            Logue.Add("results in available items");
            return tmp_items;
        }

        return null;
    }
    #endregion

    #region history
    public class ItemKey {
        public ItemKey(string c, Item it) {
            item = it;
            key = c;
        }

        public string key;
        public Item item;
    }
    #endregion

    // est-ce vraiment perspicate ? le truc devrait être dans la fonction 

    #region numerical value
    public bool hasNumericValue() {
        return numericValueInInput > 0;
    }
    private void fetchNumericValues() {

        if (Regex.IsMatch(mainInput, @$"\ball\b")) {
            numericValueInInput = potentialItems.Count;
            return;
        }

        foreach (var part in mainInput.Split(' ')) {
            if (part.All(char.IsDigit) && int.TryParse(part, out numericValueInInput))
                break;
        }
    }
    #endregion

    #region singleton
    private static ItemParser _curr;
    public static ItemParser GetCurrent {
        get {
            if (_curr == null)
                _curr = new ItemParser();
            return _curr;
        }
    }
    public static List<ItemParser> history = new List<ItemParser>();
    public static void clearParser() {
        Debug.Log("NEW PARSER");
        history.Add(_curr);
        _curr = new ItemParser();
    }
    #endregion

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
}
