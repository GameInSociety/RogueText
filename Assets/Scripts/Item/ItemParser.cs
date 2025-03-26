using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

// Features of item parser
// 1 ) Get the verb from the input
// 2 ) Separate parts of input ( the apple /WITH the banana )
// 3 ) 

[System.Serializable]
public class ItemParser {

    public string startText = "";
    public string _text = "";
    public Verb verb;
    public IP_Part[] parts;
    string delayedSequence = "";

    public void Parse(string txt) {
        // assigning seq
        startText = txt.ToLower();
        _text = startText;

        TextManager.Write($"\n=> {_text}\n", Color.magenta);

        // Fetch & Extract verb before splitting input.
        FetchVerb();
        ExtractParts();

        if (!IsInputComplete())
            return;

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

    bool IsInputComplete() {

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
                TextManager.Write($"there's no {part.GetText} around");
                return false;
            }
        }

        // Clear all used text to see if there's unused text
        foreach (var part in parts) {
            part.ClearText();
            if (!string.IsNullOrEmpty(part.finalText)) {
                Debug.LogError($"part : !{startText}! is incomplete ({part.finalText})");
                TextManager.Write($"There's no {part.finalText} {part.MainItem().DebugName}");
                return false;
            }
        }
        

        return true;
    }

    public void TriggerAction() {

        var inputSequence = (Sequence)null;

        // If only a verb is present in the input
        // Get the "Any Item" sequence.
        if ( parts.Length == 0) {
            inputSequence = verb.GetItemSequence(WorldData.anyItem.GetData());
            // Return if the verb doesn't have a default sequence.
            if (inputSequence == null) {
                TextManager.Write($"{verb.question} do you want to {verb.GetFull}");
                return;
            }
            // Create default verb sequence.
            parts = new IP_Part[1];
            parts[0] = new IP_Part("any item : no text", this);
            parts[0].index = 0;
            parts[0].items.Add(WorldData.anyItem);
        }
        // Get verb & item combination
        else {
            var actionItem = GetPart().items[0];
            inputSequence = verb.GetItemSequence(actionItem.GetData());
            if (inputSequence == null)
                inputSequence = verb.GetItemSequence(WorldData.anyItem.GetData());

            if (inputSequence == null) {
                TextManager.Write($"you can't {verb.GetFull} {actionItem.GetText("the dog")}");
                return;
            }
        }

        // Checks if input parts are used or not.
        CheckPartsIntegrity(inputSequence);

        // Display feedback sentence on description
        TextManager.Write($"you start {verb.GetCurrentWord}ing {GetPart(0).items.First().GetText("a dog")}", Color.yellow);

        DisplayInput.Instance.Disable();

        delayedSequence = inputSequence.mContent;
        if (inputSequence.duration > 0) {
            SequenceManager.Instance.onWaitEnd += HandleOnWaitEnd;
            Debug.Log($"Input Sequence has duration...");
            SequenceManager.Instance.Wait(GetPart(0).MainItem(), inputSequence.duration);
        } else {
            Debug.LogError($"Input Sequence has no duration...");
            HandleOnWaitEnd();
        }


    }

    void HandleOnWaitEnd() {
        SequenceManager.Instance.onWaitEnd -= HandleOnWaitEnd;
        // Trigger actions for all items
        foreach (var item in GetPart().items) {
            var parserSequence = new Sequence(item, delayedSequence);
            parserSequence.StartSequence(Sequence.Source.PlayerAction);
        }

        DisplayInput.Instance.Enable();
    }

    private void CheckPartsIntegrity(Sequence sequence) {
        // look at item keys
        var keys = GetItemKeys(sequence.mContent);

        // Set first item used.
        GetPart(0)?.SetUsed();

        // Check for every item call ( !item ) in actionsequence to see if any item is not going to be used.    
        int k = 0;
        for (int i = 1; i < parts.Length; i++) {
            if (parts[i].skip) {
                continue;
            }
            if (k < keys.Count) {
                parts[i].SetUsed();
                ++k;
            }
        }
        foreach (var part in parts) {
            if (!part.used && Regex.IsMatch(part.GetText, @$"\bfrom\b")) {
                part.skip = true;
            }
        }
        foreach (var part in parts) {
            if (!part.used) {
                TextManager.Write($"there's no {GetPart(0).MainItem().DebugName} {part.GetText}");
                return;
            }
        }
    }

    // This return the number of items the sequence refers to.
    // Used if a part is going to be used in the sequence
    // In order to see if a part isn't useless ( jibberish )
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

        return keys;
    }

    #region Parts

    public void ExtractParts() {
        var split = SplitParts();
        parts = new IP_Part[split.Length];
        // Firstly, create the parts
        for (int i = 0; i < parts.Length; i++) {
            parts[i] = new IP_Part(split[i], this);
            parts[i].index = i;
        }

        // Parse them later, to avoir nullreference when accessing InputParser in ItemLink
        foreach (var part in parts) {
            part.Parse();
        }

        // Sort the items with world context
        foreach (var part in parts) {
            part.SortItems();
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
    #endregion

    /// <summary>
    /// this list is use for item link purpuses.
    /// it is flipped because the first items may be props for spec item.
    /// il y avait une raison.
    /// "take apple from the left hand to the back"
    /// sinon il prend left hand avant back.
    /// �a va poser probleme mais on verra peut �tre pas.
    /// </summary>
    /// <returns></returns>


    /// <summary>
    /// EXTRACT VERBS
    /// </summary>
    /// <returns></returns>
    public Verb FetchVerb(){
        if ( !Verb.IsNull(verb) )
            return null;

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
        return verb;

    }

    /// <summary>
    /// Get & Set 
    /// </summary>
    /// <returns></returns>
    public List<Item> GetItems() {
        var _its = new List<Item>();
        foreach (var part in parts)
            _its.AddRange(part.items);
        return _its;
    }

    public IP_Part GetPart(int i = 0) {
        return parts[i];
    }

    private string[] separators = new string[] {
        "in",
        "from",
        "at",
        "on",
        "with",
        "through",
        "with",
    };

    #region singleton
    private static ItemParser _instance;
    public static ItemParser Instance {
        get {
            return _instance;
        }
    }
    public static List<ItemParser> debug_archive = new List<ItemParser>();
    public static void Clear(){
        _instance = new ItemParser();
        debug_archive.Add(_instance);
    }
    #endregion

    ///
    /// DEBUG
    /// 

    public string log;
    public void Log(string message, Color color) {

        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $"\n{txt_color}{message}</color>";
        log += str;
    }
    public void AddLog(string message, Color color) {

        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $" {txt_color}{message}</color>";
        log += str;
    }

}
