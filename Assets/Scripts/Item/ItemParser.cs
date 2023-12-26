using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ItemParser {
    // seq entered by the player

    // obsolete with group system
    // main prms
    public List<ItemGroup> mainGroups = new List<ItemGroup>();
    public List<ItemGroup> otherGroups = new List<ItemGroup>();
    public List<Property> properties = new List<Property>();

    public Verb verb;
    public Item GetMainItem(int i= 0) {
        return mainGroups[i].first;
    }

    public bool[] holds = new bool[4];

    // input
    public List<string> inputs = new List<string>();
    public string mainInput => inputs[0];
    public string lastInput => inputs[inputs.Count - 1];
    public int integer;
    //
    public void Parse(string _text) {

        // assigning seq
        inputs.Add(_text);
        /*var str = $"parts text : ";
        foreach (var part in _text.Split(' ')) {
            str += $"({part})";
        }
        Debug.Log(str);*/
        TextManager.Write($"\n=> {_text}\n");

        FetchIntegers();
        FetchVerb();
        FetchItems();
        
        // sending feedback if no verbs or item have been detexted
        if (!InputHasItemsAndVerbs())
            return;

        // no doing this anymore for "other groups_deub", putting this aside.
        if (!CanDistinguishItems())
            return;

        // t'as fait ce truc qui est un peu d�bile mais PAS TANT QUE �a.
        // parce que d'ici tu peux check TOUS les items avec lesquels le verb peut int�ragir.

        TriggerAction();
    }

    void FetchIntegers() {
        integer = -1;
        string str = Regex.Match(lastInput, @"\d+").Value;
        if (string.IsNullOrEmpty(str)) return;
        integer = int.Parse(str);
    }

    public bool CanDistinguishItems() {
        foreach (var itmGrp in mainGroups) {
            if (!itmGrp.TryInit()) {
                string hold = $"which {itmGrp.items[0].GetText("dog")} would you like to {verb.GetFull}";
                string fail = $"there is no such {itmGrp.items[0].GetText("dog")} present";
                HoldParser(hold, fail, 1);
                return false;
            }
            Confirm(1);
        }
        // not distinguishing other groups
        return true;
    }

    public void TriggerAction() {
        for (int i = 0; i < mainGroups.Count; i++) {
            var sequence = verb.GetItemSequence(GetMainItem(i).GetData());

            if (sequence == null) continue;

            WorldAction newEvent = new WorldAction(mainGroups[i], Tile.GetCurrent.TileInfo, sequence);
            newEvent.Call(WorldAction.Source.PlayerAction);
            NewParser();
            return;
        }

        TextManager.Write($"you can't {verb.GetFull} {GetMainItem(0).GetText("the dog")}");
        NewParser();
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
    public bool HasSecondItem() {
        return otherGroups.Count > 0;
    }
    public List<Item> GetOptionalItems() {
        var tmps = new List<Item>();
        for (int i = otherGroups.Count-1; i >= 0; i--)
            tmps.AddRange(otherGroups[i].items);
        return tmps;
    }

    void FetchItems(){
        string text = lastInput;
        if (!Verb.IsNull(verb))
            text = text.Replace(verb.GetCurrentWord, "");
        
        AvailableItems.UpdateItems();

        var groups = new List<ItemGroup>();
        foreach (var item in AvailableItems.currItems) {
            Word.Number num = Word.Number.None;
            var indexInInput = item.GetIndexInText(text, out num);
            if (indexInInput < 0) continue;
            var group = groups.Find(x => x.index == indexInInput && x.first.dataIndex == item.dataIndex);
            if (group == null) {
                group = new ItemGroup(indexInInput, num);
                group.debug_name = $"{item.debug_name} (from item)";
                group.text = text;
                groups.Add(group);
            }
            group.items.Add(item);
            var prop = item.GetPropInText(text, out indexInInput);
            if ( prop != null) {
                group.linkedProps.Add(prop);
            }
        }

        if ( groups.Count == 0) {
            foreach (var item in AvailableItems.currItems) {
                Word.Number num = Word.Number.None;
                var indexInInput = 0;
                var prop = item.GetPropInText(text, out indexInInput);
                if (prop == null) continue;
                var group = groups.Find(x => x.index == indexInInput && x.first.dataIndex == item.dataIndex);
                if (group == null) {
                    group = new ItemGroup(indexInInput, num);
                    group.debug_name = $"{item.debug_name} (from property : {prop.name})";
                    group.text = text;
                    groups.Add(group);
                }
                group.items.Add(item);
                group.linkedProps.Add(prop);
            }

            if (groups.Count == 0)
                return;
        }

        groups.Sort((a, b) => a.index.CompareTo(b.index));
        mainGroups.Add(groups[0]);
        for (int i = 1; i < groups.Count; i++) {
            if (groups[i].index == mainGroups[0].index)
                mainGroups.Add(groups[i]);
            else
                otherGroups.Add(groups[i]);
        }
    }

    void FetchVerb(){
        if ( !Verb.IsNull(verb) ) {
            return;
        }
        List<Verb> verbs = Verb.verbs.FindAll(x => x.GetIndexInText(lastInput) >= 0);
        if (verbs.Count == 0)
            return;

        var smallestIndex = 0;
        foreach (var item in verbs) {
            if ( item.indexInText < smallestIndex) {
                smallestIndex = item.indexInText;
            }
        }
        verbs = verbs.FindAll(x => x.indexInText == smallestIndex);
        if (verbs.Count == 0)
            return;
        // get longest verb
        Verb longestVerb = verbs[0];
        foreach (var item in verbs) {
            if (item.GetCurrentWord.Length > longestVerb.GetCurrentWord.Length)
                longestVerb = item;
        }
        verb = longestVerb;

    }

    public void Confirm(int id) {
        holds[id] = false;
    }
    public void HoldParser(string hold, string fail, int id) {
        TextManager.Write(holds[id] ? fail : hold);
        if (holds[id])
            NewParser();
        else
            holds[id] = true;
    }

    public bool InputHasItemsAndVerbs() {
        if (Verb.IsNull(verb)) {
            // no verbs, but item
            if (mainGroups.Count > 0) {
                // checking if the input is ALREADY waiting for a verb
                string fail = $"you can't do that with {GetMainItem(0).GetText("the dog")}";
                string hold = $"what do you want to do with {GetMainItem(0).GetText("the dog")}";
                HoldParser(hold, fail, 0);
                return false;
            }
            // no verbs or item
            TextManager.Write("!your phrase needs a verb and a surrounding item!");
            NewParser();
            return false;

        } else if (mainGroups.Count == 0) {

            var sequence = verb.GetItemSequence(ItemData.GetItemData("no item"), false);
            if ( sequence != null) {
                mainGroups.Add(new ItemGroup(0, Word.Number.Singular));
                mainGroups.First().items.Add(AvailableItems.NoItem());
                return true;
            }

            // no itms, but verb
            string fail = $"... i don't understand want you want to {verb.GetCurrentWord} {verb.GetPreposition}";
            string hold = $"what do you want to {verb.GetCurrentWord} {verb.GetPreposition}";
            HoldParser(hold, fail, 0);
            return false;
        }
        Confirm(0);
        return true;
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
