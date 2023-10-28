using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;
using System.IO;

[System.Serializable]
public class ItemParser {
    // text entered by the player

    // obsolete with group system
    // main prms
    public List<Item> potentialItems = new List<Item>();
    public List<ItemGroup> itemGroups = new List<ItemGroup>();

    private Verb verb;
    public Verb getVerb => verb;
    public Item firstItem => potentialItems[0];

    public int numericValueInInput;

    public bool onHold = false;
    
    // input
    public List<string> inputs = new List<string>();
    public string mainInput => inputs[0];
    public string lastInput => inputs[inputs.Count - 1];

    public Item mainItem() {
        return potentialItems.First();
    }
    //

    // history
    public List<ItemParser> history = new List<ItemParser>();

    public List<ItemKey> itemHistory = new List<ItemKey>();
    public Item parentItem;

    public void Parse(string _text) {

        // assigning text
        inputs.Add(_text);

        tryFetchVerbs();
        tryFetchItems();
        tryFetchNumericValues();

        // sending feedback if no verbs or item have been detexted
        if (!inputHasVerbAndItems())
            return;
        onHold = false;
        foreach (var itmGrp in itemGroups){
            if (!itmGrp.itemsAreDistinct()){
                if (onHold){
                    TextManager.write($"there is no such {itmGrp.items[0].debug_name} present");
                    NewParser();
                } else {
                    TextManager.write($"which {itmGrp.items[0].getWord("dog")} would you like to {verb.GetFull}");
                    onHold = true;
                }
                return;
            }
            onHold = false;
        }

        // all itms and vebs have been found and specified
        // and triggering the function
        FunctionSequence.TrySequence();
    }

    void tryFetchItems(){
        AvailableItems.update();
        var itms = getItemsFromText(lastInput);
        if (itms.Count == 0 && verb != null){
            var verbItem = Item.GetDataItem("no item");
            if (verb.HasCell(verbItem)) {
                itms.Add(verbItem);
                Debug.Log($"found general universal for verb {verb.getWord}");
            } else {
                Debug.Log($"no items were found for {mainInput}");
            }
            return;
        }
        potentialItems.AddRange(itms);

        // create item groups here
        for(int i = 0; i < itms.Count; ++i){
            var itmGroup = itemGroups.Find(x=>x.index == itms[i].indexInInput);
            if ( itmGroup == null){
                Debug.Log($"no with index {itms[i].indexInInput}");
                foreach (var itmGrp in itemGroups)
                Debug.Log($"itm grp index {itmGrp.index}");
                itmGroup = new ItemGroup(itms[i].indexInInput);
                itemGroups.Add(itmGroup);
            }

            itmGroup.items.Add(itms[i]);
        }

        itemGroups.OrderByDescending(x => x.index);
    }


    List<Item> getItemsFromText (string text) {
        var itms = new List<Item>();
        itms.AddRange(AvailableItems.Get.list.FindAll(x => x.getIndexInText(text, Word.Number.Singular) >= 0));
        itms.AddRange(AvailableItems.Get.list.FindAll(x => x.getIndexInText(text, Word.Number.Plural) >= 0));
        return itms.OrderByDescending(x => x.indexInInput).ToList();
    }

    void tryFetchVerbs(){
        if ( verb != null)
            return;
        List<Verb> verbs = Verb.verbs.FindAll(x => x.getIndexInText(lastInput) >= 0);
        if (verbs.Count > 0)
            verb = verbs[0];
    }

    public bool inputHasVerbAndItems() {
        if (verb == null) {
            // no verbs, but item
            if (potentialItems.Count > 0) {
                // checking if the input is ALREADY waiting for a verb
                if (onHold) {
                    TextManager.write($"you can't do that with {potentialItems[0].getWord("the dog")}");
                    NewParser();
                } else {
                    TextManager.write($"what do you want to do with {potentialItems[0].getWord("the dog")}");
                    onHold = true;
                }
            }
            // no verbs or item
            else {
                TextManager.write("!no verb or item");
                NewParser();
            }
            return false;
        } else if (potentialItems.Count == 0) {
            // checking if the verb is autonomus (dig, eat, sleep etc...)
            var verbItem = Item.GetDataItem("no item");
            if (verb.HasCell(verbItem)) {
                potentialItems.Add(verbItem);
                Debug.Log("found general universal for verb");
                return true;
            }

            // no itms, but verb
            if (onHold) {
                TextManager.write($"... i don't understand want you want to {verb.getWord} {verb.GetPreposition}");
                NewParser();
            } else {
                TextManager.write($"what do you want to {verb.getWord} {verb.GetPreposition}");
                onHold = true;
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
        // ex: player/body group, this will search a body group in the player
        // instead of in all the available itms
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

        var itms = getPotentialItems(key);

        if (itms.Count == 0) {
            Logue.Add("no results with key " + key + " in available itms");
            modifiedKey = "error";
            return null;
        }

        // before sorting by type, check if the item's name appears in input
        var item = itms.Find(x => x.containedInText(lastInput));

        if (item != null) {
            type = SearchType.Input;
            goto Result;
        }

        // if no results, sort by type
        switch (type) {
            case SearchType.First:
                item = itms.First();
                break;
            case SearchType.Highest:

                var highestProp = itms.First().GetProperty(key);
                var i = 0;
                for (var itIndex = 1; itIndex < itms.Count; itIndex++) {
                    var it = itms[itIndex];
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

                item = itms[i];
                break;
            case SearchType.Random:
                item = itms[UnityEngine.Random.Range(0, itms.Count)];
                break;
            default:
                item = itms[0];
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
        // IMPORTANT for target ... and target/body group
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

        // search for item in pending itms
        if (parentItem != null) {
            Logue.Add("searching in " + parentItem.debug_name);
            tmp_items.AddRange(parentItem.getRecursive(3).FindAll(match));
            return tmp_items;
        }

        // add available itms
        tmp_items.AddRange(AvailableItems.Get.list.FindAll(match));
        if (tmp_items.Count > 0) {
            Logue.Add("results in available itms");
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

    // est-ce vraiment perspicate ? le truc devrait �tre dans la fonction 

    #region numerical value
    public bool hasNumericValue() {
        return numericValueInInput > 0;
    }
    private void tryFetchNumericValues() {

        if (Regex.IsMatch(mainInput, @$"\ball\b")) {
            numericValueInInput = potentialItems.Count;
            return;
        }

        foreach (var group in mainInput.Split(' ')) {
            if (group.All(char.IsDigit) && int.TryParse(group, out numericValueInInput))
                break;
        }
    }
    #endregion

    #region singleton
    private static ItemParser _prev;
    private static ItemParser _curr;
    public static ItemParser GetPrevious{
        get{
            return _prev;
        }
    }
    public static ItemParser GetCurrent {
        get {
            return _curr;
        }
    }
    public static void NewParser(){
        if (_curr != null){
            DebugManager.Instance.previousParser = _prev;
            _prev = _curr;
        }
        _curr = new ItemParser();
        DebugManager.Instance.currentParser = _curr;
    }
    #endregion

    
}
