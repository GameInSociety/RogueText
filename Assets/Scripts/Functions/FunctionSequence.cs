using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Search;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

[System.Serializable]
public class FunctionSequence {

    #region static declaration
    // a function sequence is a list of functions provided in a cell

    // the ongoing sequence
    public static FunctionSequence current;
    // a paused sequence... why wouldn't it be the same ?
    public static FunctionSequence pausedSequence;
    // list of sequence to be read if a sequence is launched when an ongoing one is unfinished
    public static List<FunctionSequence> list = new List<FunctionSequence>();

    // end event at the end of all sequences
    public delegate void OnFinishSequences();
    public static OnFinishSequences onFinishSequences;
    #endregion
    
    // the cell lines of the sequences. parsed each line
    public string[] lines;
    // the target item of the function.
    // could in the input parser,
    // but also in an event, or inside a cell
    public Item item;
    // the tile where the function takes place
    public Tile tile;

    // meh
    public bool stopped = false;
    public bool _nextNode = false;

    // je comprends pas vraiment pourquoi �a peutp pas simplement �tre "current != null" mais soite.
    public static bool SequenceFinished => current != null;

    public static void TrySequence() {
        // check if the sequence has been pause
        // if there's an item confusion mid function (ex: use key => on which door would you like to use de key)
        if (pausedSequence != null) {

            Debug.Log("calling paused sequence");
            pausedSequence.Call();
            pausedSequence = null;
            return;
        }

        var item = ItemParser.GetCurrent.mainItem();
        var sequence = ItemParser.GetCurrent.getVerb.GetSequence(ItemParser.GetCurrent.mainItem());
        if (sequence == null) {
            TextManager.write($"you can't {ItemParser.GetCurrent.getVerb.GetFull} {item.getWord("the dog")}");
            return;
        }

        // putting aside mutiple items in function sequence because it may be obsolete with the new item search thing
        // need to try and make plates work after

        newSequence(
            sequence.content,
            ItemParser.GetCurrent.potentialItems[0],
            Tile.GetCurrent
            );

        // !!!!!!!!!!!!!!! //
        // this should be called at the end of all the function sequences.
        ItemEvent.callEvent("subAction");
    }

    public static FunctionSequence newSequence(
        string _lines,
        Item targetItem,
        Tile tile) {

        if (tile == null) Debug.LogError("tile is null for : " + targetItem.debug_name);

        var sequence = new FunctionSequence();
        sequence.Parse(_lines);
        sequence.tile = tile;
        sequence.item = targetItem;
        sequence.Call();
        return sequence;
    }

    public void Call() {
        stopped = false;

        // adding the sequence to the list if one is already unfinished
        if (current != null) {
            list.Add(this);
            return;
        }

        current = this;

        // debug log
        Logue.New("func", $"[FUNCTION] {item.debug_name} {Tile.GetCurrent.debug_name}", Color.cyan);

        // separate all actions
        foreach (var line in lines) {
            var _line = line;
            if (string.IsNullOrEmpty(_line)) { continue; }

            if (_line.StartsWith("else ")) {

                // check if the line starts with '*'
                if (_nextNode) {
                    // remove * and call function
                    _line = line.Remove(0, 5);
                    _nextNode = false;
                } else {
                    Debug.Log("* in line " + line + ", finishing sequence");
                    goto End;
                }

            } else if (_nextNode) continue;

            if (_line.StartsWith("> ")) {
                JumpToSequence(_line);
                goto End;
            }

            CallFunction(line);

            // an element has stopped the sequence when it's going
            // (example : player doesn't have key => stop)
            if (stopped) goto End;
        }

        End:


        // before ending the sequences, 
        if (list.Count > 0) {
            var nextSequence = list[0];
            list.RemoveAt(0);
            nextSequence.Call();
            return;
        }

        // !!!!!!!!!!!! //
        // end of all sequences...
        ItemParser.NewParser();
        ItemParser.GetCurrent.itemHistory.Clear();
        TextManager.Return();
        PropertyDescription.Describe();
        current = null;

        if (onFinishSequences != null)
            onFinishSequences();
    }


    void CallFunction(string line) {

        // c'est ici qu'une DEUXIEME passes d'input se fait.
        // � priori, c'est comme un deuxi�me 
        if (line.StartsWith('*')) {
            line = GetNewItem(line);
        }
        // get function class 
        var functionName = Function.GetName(line);
        functionName = TextUtils.FirstLetterCap(functionName);
        var objectType = Type.GetType("Function_" + functionName);
        if (objectType == null) {
            Debug.LogError("function " + functionName + " doesn't exist");
            return;
        }

        var function = Activator.CreateInstance(objectType) as Function;

        function.InitParams(line);
        function.TryCall((Item)item);
    }

    string GetNewItem(string line) {
        // the function will have effect on the player item instead of the one in the function

        // crop the line between **
        // ex: *player* => player
        line = line.Remove(0, 1);
        line = line.Remove(line.IndexOf('*'));
        //

        // search for a new item 
        // this will search for a new item in the input.
        // it will override the current one, so consider creating a new one.
        // � voir
        item = ItemParser.GetCurrent.SearchItemInInput(line);
        // the parser did not find the item in the input
        if (item == null) {
            Stop();
            return line;
        }
        if (ItemParser.GetCurrent.onHold) {
            Pause();
            return line;
        }

        return line.Remove(0, line.Length + 3);
    }

    void JumpToSequence(string line) {
        line = line.Remove(0, 2);
        //var verb = ItemParser.getVerbInInput(line);
        var verb = ItemParser.GetCurrent.getVerb;
        if (verb != null) {
            Debug.Log("found verb : " + line);
        } else {
            Debug.Log("did not find verb in " + line);
        }

        var item = AvailableItems.Get.findInTargetText(line);
        if (item != null) {
            Debug.Log("found item : " + item.debug_name);
        } else {
            Debug.Log("didn't find item in " + line);
        }

        var sequence = verb.GetSequence(item);

        _ = newSequence(
            sequence.content,
            item,
            Tile.GetCurrent
            );

        Debug.Log("go to other sequence");
    }

    public void Parse(string cell) {
        lines = cell.Split('\n');
    }

    #region sequence
    public void GoToNextNode() {
        Debug.Log("skip to next node");
        _nextNode = true;
    }

    public void Pause() {
        Logue.Add("Sequence Paused");
        pausedSequence = this;
        clear();
    }

    // break
    public void Break(string message) {
        TextManager.write(message);
        Stop();
    }
    public void Stop() {
        Logue.Add("Sequence Stopped");
        clear();

    }
    public void clear() {
        current = null;
        stopped = true;
    }
    #endregion
}