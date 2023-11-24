using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Security.Policy;

[System.Serializable]
public class ItemParser {
    // text entered by the player

    // obsolete with group system
    // main prms
    public List<ItemGroup> itemGroups = new List<ItemGroup>();

    public Verb verb;
    public Item first => itemGroups[0].items.First();

    public bool[] holds = new bool[4];

    // input
    public List<string> inputs = new List<string>();
    public string mainInput => inputs[0];
    public string lastInput => inputs[inputs.Count - 1];
    //
    public void Parse(string _text) {

        // assigning text
        inputs.Add(_text);
        TextManager.Write($"<color=red>  {_text} </color>");

        FetchVerb();
        FetchItems();
        // sending feedback if no verbs or item have been detexted
        if (!InputHasItemsAndVerbs())
            return;

        foreach (var itmGrp in itemGroups){
            if (!itmGrp.TryInit()) {
                string hold = $"which {itmGrp.items[0].GetText("dog")} would you like to {verb.GetFull}";
                string fail = $"there is no such {itmGrp.items[0].GetText("dog")} present";
                HoldParser(hold, fail, 1);
                return;
            }
            Confirm(1);
        }

        // t'as fait ce truc qui est un peu débile mais PAS TANT QUE ça.
        // parce que d'ici tu peux check TOUS les items avec lesquels le verb peut intéragir.

        var sequence = verb.GetItemSequence(first.GetData());
        TriggerAction(sequence);
        
    }

    public void TriggerAction(string sequence) {
        if (sequence == null) {
            TextManager.Write($"you can't {verb.GetFull} {first.GetText("the dog")}");
            NewParser();
            return;
        }

        Debug.Log($"sequence {sequence}");

        WorldAction newEvent = new WorldAction(itemGroups.First(), Tile.GetCurrent.coords, Player.Instance.wordIndex, sequence);
        newEvent.Call();

        NewParser();
    }

    void FetchItems(){
        if (itemGroups.Count > 0) {
            return;
        }
        AvailableItems.updateItems();
        itemGroups = ItemGroup.GetGroups(AvailableItems.currItems, lastInput);
    }

    void FetchVerb(){
        if ( !Verb.IsNull(verb) ) {
            Debug.Log($"the verb {verb.GetCurrentWord} is already in the input");
            return;
        }
        List<Verb> verbs = Verb.verbs.FindAll(x => x.getIndexInText(lastInput) >= 0);
        if (verbs.Count > 0)
            verb = verbs[0];
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
            if (itemGroups.Count > 0) {
                // checking if the input is ALREADY waiting for a verb
                string fail = $"you can't do that with {itemGroups[0].first.GetText("the dog")}";
                string hold = $"what do you want to do with {itemGroups[0].first.GetText("the dog")}";
                HoldParser(hold, fail, 0);
                return false;
            }
            // no verbs or item
            TextManager.Write("!your phrase needs a verb and a surrounding item!");
            NewParser();
            return false;

        } else if (itemGroups.Count == 0) {

            var sequence = verb.GetItemSequence(ItemData.GetItemData("no item"), false);
            if ( sequence != null) {
                itemGroups.Add(new ItemGroup(0, Word.Number.Singular));
                itemGroups.First().items.Add(AvailableItems.NoItem());
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
