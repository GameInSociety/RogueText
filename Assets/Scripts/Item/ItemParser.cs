using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class ItemParser {

    public List<string> separators = new List<string>() {
        "in",
        "from",
        "at",
        "on",
        "with",
        "through",
    };

    public Verb verb;
    public Part[] parts;
    string _text = "";
    public int currentState = 0;

    public string log;
    [HideInInspector] public ItemParser parser;
    public void Log(string message, Color color) {

        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $"\n{txt_color}{message}</color>";
        log += str;
    }

    public Part GetPart(int i = 0) {
        return parts[i];
    }

    [System.Serializable]
    public class Part {
        public int partIndex;
        public bool complete;
        public bool needsDistinction;
        public bool skip;
        public string problem;
        public bool used;
        public string text;
        public int number = -1;
        public List<Item> items = new List<Item>();
        public List<Property> properties = new List<Property>();
        private ItemParser parser;

        public bool HasItems() {
            return items.Count > 0;
        }
        public Item MainItem() {
            return items.First();
        }
        public void SetUsed() {
            used = true;
        }
        void Log(string message, Color color) {
            parser.Log(message, color);
        }

        public Part(string _part, ItemParser parser) {
            text = _part;
            this.parser = parser;

        }

        public void Parse() {

            Log($"[{text}]", Color.yellow);

            GetNumber();
            GetItems();
        }

        public void GetNumber() {
            string str = Regex.Match(text, @"\d+").Value;
            if (string.IsNullOrEmpty(str)){
                return;
            }
            number = int.Parse(str);
            Log($"parsed number : {number}", Color.grey);
        }

        public void GetProperties() {
            if (items.Count == 0)
                return;
            var item = items[0];
            var prop = item.GetPropInText(text);
            if (prop != null) {
                properties.Add(prop);
                Log($"found prop : {prop.name}", Color.magenta);
            } else {
                Log("no properties", Color.grey);
            }

        }

        public void GetItems() {
            items = AvailableItems.currItems.FindAll(x => x.ContainedInText(text));
            if (items.Count == 0) {
                items = AvailableItems.currItems.FindAll(x => x.GetPropInText(text) != null);
                if (items.Count > 0)
                    Log("from props", Color.white);
            } else {
                Log("from items", Color.white);
                // only search properties if the item has been found through an item name (no property)
                GetProperties();

            }
            if (items.Count == 0) {
                Log("no items", Color.red);
                return;
            }

            var itemText = "";
            for (int i = 0; i < items.Count; i++)
                itemText += $"{items[i].debug_name} {TextUtils.GetSpaces(i, items.Count)}";
            Log(itemText, Color.green);
        }

        public void SortItems() {

            Log("!sorting!", Color.magenta);
            if (items.Count == 1) {
                Log("only one item, no sorting", Color.magenta);
                return;
            }

            if (Regex.IsMatch(text, @$"\ball\b")) {
                Log("found 'all' in input, no sorting", Color.magenta);
                return;
            }

            var first = items[0];

            // check some 
            if (Regex.IsMatch(text, @$"\bsome\b")) {
                float f = (float)items.Count / 2;
                int half = (int)Mathf.Clamp(Mathf.Round(f), 1, items.Count);
                if (half < items.Count)
                    items.RemoveRange(half, items.Count - half);
                Log($"found 'some' in input, returning {half} items", Color.magenta);
                return;
            }

            // check digit
            if (number >= 0) {
                if (number >= items.Count)
                    return;
                for (int i = number; i < items.Count; i++)
                    items.RemoveAt(i);
                Log($"found number in part, return {number} items", Color.magenta);
                return;
            }


            // plural
            if (MainItem().GetWord().currentNumber == Word.Number.Plural) {
                Log($"word was found plural, returning all", Color.magenta);
                return;
            }

            AssignOrdinalProps(items);

            var item = (Item)null;
            // look for specific item
            if (partIndex == 0 && parser.parts.Length > 1) {
                var nextPart = parser.parts[partIndex + 1];
                item = items.Find(x => x.GetPropInText(nextPart.text) != null);
                if ( item != null) {
                    var nextProp = item.GetPropInText(nextPart.text);
                    nextPart.SetUsed();
                    Log($"found prop {nextProp.name} in part {nextPart.text} for item {item.debug_name}", Color.magenta);
                    items.RemoveAll(x=> x!= item);
                    return;
                }

            }

            item = items.Find(x => x.GetPropInText(text) != null);
            if (item == null) {
                if (first.HasProp("dif")) {
                    Log($"item {items[0].debug_name} must be  disinct, but passing anyways", Color.red);
                    needsDistinction = true;
                }
                Log($"item {items[0].debug_name} does not need distinction, return first", Color.magenta);

                // check if items are differents ( "knife and key" ) ( "apple ( from 5 ) and knife" ), then return one of each

                List<Item> otherItems = items.FindAll(x=> x.dataIndex != items.First().dataIndex);
                foreach (var other in otherItems) {
                    Debug.Log($"also found {other.debug_name}");
                }
                
                items = new List<Item>() { items[0] };
                foreach (var i in otherItems)
                    items.Add(i);
                return;
            }

            // narrowing an item because a prop was found
            items.RemoveAll(x => x != item);
            var prop = item.GetPropInText(text);
            string propText = $"{prop.name}:{prop.GetDescription()}{prop.GetPart("search")?.content}";
            Log($"found prop {propText} for item {item.debug_name}", Color.magenta);
        }

        void AssignOrdinalProps(List<Item> items) {
            for (int i = 0; i < items.Count; i++) {
                string ordinal = GetOrdinal(i);
                var ordinal_prop = items[i].GetProp("ordinal");
                if (ordinal_prop == null) {
                    ordinal_prop = new Property();
                    ordinal_prop.name = "ordinal";
                    ordinal_prop.AddPart("search", ordinal);
                }
                items[i].SetProp($"ordinal / search:{ordinal}");
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
            if ( i >= ordinals.Length) {
                return "other";
            }
            return ordinals[i];
        }
    }
    public void Parse(string txt) {

        // assigning seq
        _text = txt.ToLower();
        Log(_text, Color.cyan);

        TextManager.Write($"\n=> {_text}\n", Color.magenta);

        // verb
        FetchVerb();
        FetchItems();

        if (!FoundAllElements()) {
            return; 
        }

        // t'as fait ce truc qui est un peu d�bile mais PAS TANT QUE �a.
        // parce que d'ici tu peux check TOUS les items avec lesquels le verb peut int�ragir.
        TriggerAction();

        // check parts
        /*var unusedParts = System.Array.FindAll(parts, x => !x.used);
        if ( unusedParts.Length > 0) {
            foreach (var part in unusedParts) {
                Log($"part {part.text} was not used", Color.red);
            }
            return;
        }
        Log($"all parts were used", Color.green);*/

    }

    bool FoundAllElements() {

        if (Verb.IsNull(verb)) {
            if (GetPart(0).HasItems()) {
                TextManager.Write($"what do yo want to do with {GetPart().MainItem().GetText("the dog")}\n( le verbe existe pas encore ou est pas correct )");
            } else {
                TextManager.Write("write a verb, then something to interact with \b( il a rien compris )");
            }
            return false;
        } else {
            /*if (parts.Length == 0)
                TextManager.Write($"what do you want to {verb.GetCurrentWord} {verb.GetPreposition}");
            return false;*/
        }

        foreach (var part in parts) {
            if (!part.HasItems()) {
                TextManager.Write($"there's no {part.text} around");
                NewParser();
                return false;
            }
            if (part.needsDistinction) {
                // for now forget it
                //TextManager.Write($"which {part.MainItem().debug_name} do you want to {verb.GetCurrentWord}");
                //return false;
            }
        }


        return true;
    }

    public void TriggerAction() {

        string sequence = "";
        if ( parts.Length == 0) {
            sequence = verb.GetItemSequence(WorldData.anyItem.GetData());
            if (sequence == null) {
                TextManager.Write($"you can't {verb.GetFull} anything");
                Log($"no action OR any item item for {verb.GetCurrentWord}", Color.red);
                NewParser();
                return;
            }
            parts = new Part[1];
            parts[0] = new Part("any item : no text", this);
            parts[0].partIndex = 0;
            parts[0].items.Add(WorldData.anyItem);
        } else {
            var actionItem = GetPart().items[0];
            sequence = verb.GetItemSequence(actionItem.GetData());
            if (sequence == null) {
                sequence = verb.GetItemSequence(WorldData.anyItem.GetData());
            }

            if (sequence == null) {
                TextManager.Write($"you can't {verb.GetFull} {actionItem.GetText("the dog")}");
                Log($"no action for {verb.GetCurrentWord} and {GetPart().items[0].debug_name}", Color.red);
                NewParser();
                return;
            }
        }


        // look at item keys
        Log($"Checking Items in links", Color.white);
        var keys = GetItemKeys(sequence);
        GetPart(0)?.SetUsed();
        int k = 0;
        for (int i = 1; i < parts.Length; i++) {
            if (parts[i].skip) {
                Log($"skipping : {parts[i].text}", Color.gray);
                continue;
            }
            if ( k < keys.Count) {
                parts[i].SetUsed();
                Log($"setting : {parts[i].text} to used", Color.gray);
                ++k; 
            }
        }
        foreach (var part in parts) {
            if (!part.used && Regex.IsMatch(part.text, @$"\bfrom\b")) {
                part.skip = true;
                Log($"removing {part.text} from available part, it's only a info", Color.cyan);
            }
        }
        foreach (var part in parts) {
            if (!part.used ) {
                Log($"part : {part.text} has not been used", Color.red);
                TextManager.Write($"there's no {GetPart(0).MainItem().debug_name} {part.text}");
                NewParser();
                return;
            }
        }

        // trigger action
        WorldAction parserAction = new WorldAction(GetPart().items, Tile.GetCurrent.TileInfo, sequence);
        parserAction.Call(WorldAction.Source.PlayerAction);
        NewParser();
    }

    List<string> GetItemKeys(string sequence) {
        // check sequence items
        var tmpSequence = new string(sequence);
        int startIndex = tmpSequence.IndexOf('!');
        var b = 0;

        var keys = new List<string>();
        while (startIndex >= 0) {
            // delete till '!'
            tmpSequence = tmpSequence.Remove(0, startIndex);
            // get exit index
            var chars = new char[] { ',', ')', '\n', '>' };
            int index = int.MaxValue;
            foreach (var c in chars) {
                int i = tmpSequence.IndexOf(c);
                if (i < index && i >= 0)
                    index = i;
            }
            // make key
            if (index < 0) {
                Debug.LogError($"[ITEM PARSER] propbleme probleme finding item keys in sequence : {tmpSequence}, index : {index}");
                return null;
            }
            var key = tmpSequence.Remove(index);
            if (key.Contains('>'))
                key = key.Remove(key.IndexOf('>'));

            // delete key
            tmpSequence = tmpSequence.Remove(0, index + 1);
            // restart index
            startIndex = tmpSequence.IndexOf('!');
            if (!keys.Contains(key))
                keys.Add(key);

            ++b;
            if (b > 100) {
                Debug.Log($"broke");
                break;
            }
        }

        foreach (var key in keys) {
            Log($"item key {key}", Color.grey);
        }
        if (keys.Count == 0)
            Log("no item key", Color.grey);

        return keys;
    }

    /// <summary>
    /// this list is use for item link purpuses.
    /// it is flipped because the first items may be props for spec item.
    /// il y avait une raison.
    /// "take apple from the left hand to the back"
    /// sinon il prend left hand avant back.
    /// �a va poser probleme mais on verra peut �tre pas.
    /// </summary>
    /// <returns></returns>

    void FetchItems() {
        AvailableItems.UpdateItems();

        var split = SplitParts();
        parts = new Part[split.Length];
        for (int i = 0; i < parts.Length; i++) {
            parts[i] = new Part(split[i], this);
            parts[i].partIndex = i;
            parts[i].Parse();
        }
        foreach(var part in parts) {
            if (!part.HasItems()) continue;
            part.SortItems();
            var itemText = "";
            for (int i = 0; i < part.items.Count; i++)
                itemText += $"{part.items[i].debug_name} {TextUtils.GetSpaces(i, part.items.Count)}";
            Log(itemText, Color.green);
        }

        

    }
    string[] SplitParts() {

        var pattern = "";
        foreach (var sep in separators) {
            pattern += @$"(\b{sep}\b)|";
        }
        pattern = pattern.Remove(pattern.Length-1);

        var reg = Regex.Split(_text, pattern);
        bool word = true;
        var _split = new List<string>();
        for (int i = reg.Length - 1; i >= 0; i--) {
            if (word) {
                _split.Add(reg[i]);
            } else {
                _split[_split.Count - 1] = reg[i] + _split[_split.Count - 1];
            }
            word = !word;
        }
        _split.RemoveAll(x => string.IsNullOrEmpty(x) && x.Length < 1);
        _split.Reverse();
        return _split.ToArray();
    }


    public Verb FetchVerb(){
        if ( !Verb.IsNull(verb) ) {
            return null;
        }
        List<Verb> verbs = Verb.verbs.FindAll(x => x.GetIndexInText(_text) >= 0);
        if (verbs.Count == 0)
            return null;

        var smallestIndex = 0;
        foreach (var item in verbs) {
            if ( item.indexInText < smallestIndex)
                smallestIndex = item.indexInText;
        }
        verbs = verbs.FindAll(x => x.indexInText == smallestIndex);
        if (verbs.Count == 0)
            return null;

        // get longest verb
        Verb longestVerb = verbs[0];
        foreach (var item in verbs) {
            if (item.GetCurrentWord.Length > longestVerb.GetCurrentWord.Length)
                longestVerb = item;
        }
        verb = longestVerb;
        _text = _text.Remove(0, _text.IndexOf(verb.GetCurrentWord)+ verb.GetCurrentWord.Length);
        _text = _text.Trim(' ');
        Log($"found verb : {verb.GetCurrentWord},\nnew text:{_text}", Color.blue);
        return verb;

    }

    public void Break(string message_hold, string message_fail, int state) {
        var str = currentState == state ? message_fail : message_hold;
        TextManager.Write(str);
        if (currentState == state) {
            NewParser();
            return;
        }
        currentState = state;
    }

    public List<Item> SearchForItemsInParts(string key) {
        foreach (var part in parts) {
            if (part.skip) {
                Function.LOG($"skipping part : {part.text}", Color.gray);
                continue;
            }
            var its = ItemLink.SearchItemsInRange(key, part.items);
            if (its.Count > 0)
                return its;
            else
                Function.LOG($"no item : {key} in part : {part.text} / {part.items.First()}", Color.gray);
        }
        return null;
    }

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
            _prev = _curr;
        DebugManager.Instance.previousParser = _prev;
        _curr = new ItemParser();
        DebugManager.Instance.currentParser = _curr;
    }
    #endregion

    
}
