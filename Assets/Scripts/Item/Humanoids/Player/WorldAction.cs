using DG.Tweening;
using DG.Tweening.Core.Easing;
using JetBrains.Annotations;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.Search;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class WorldAction {

    public static List<WorldAction> stack = new List<WorldAction>();
    public string debug_name;

    private List<Item> _items = new List<Item>();
    public void SetItems(List<Item> its) {
        _items = its;
        foreach (Item it in its ) {
            Debug.Log( it.debug_name );
        }
    }
    public void AddItem(Item it) {
        _items.Add(it);
    }
    public void RemoveItem(Item it) {
        _items.Remove(it);
    }
    public List<Item> GetItems() {
        return _items;
    }
    public Item TargetItem() {
        return _items.First();
    }
    public Tile.Info tileInfo;
    public Tile tile => tileInfo.GetTile();
    public string sequence;

    public Source source;
    public enum Source {
        PlayerAction,
        Event,
    }

    public enum State {
        Suceed,
        Next,
        Fail
    }

    public static WorldAction current;

    // only fail one function, move to next
    public bool failed = false;
    // fail all functions
    public bool stopped = false;
    string[] sequences;
    // feedback
    public string feedback;

    int seqIndex = 0;

    public WorldAction(Item item, Tile.Info info, string sequence) {
        debug_name = $"{item.debug_name})";
        AddItem(item);
        tileInfo = info;
        this.sequence = sequence;
    }
    public WorldAction(List<Item> items, Tile.Info info, string sequence) {
        debug_name = $"{items[0].debug_name})";
        foreach (var item in items) {
            AddItem(item);
        }
        this.tileInfo = info;
        this.sequence = sequence;
    }

    #region call
    static bool onGoing = false;
    public void Call(Source source = Source.Event) {

        this.source = source;

        if (onGoing) {
            stack.Add(this);
            return;
        }

        current = this;
        onGoing = true;
        seqIndex = 0;
        sequences = sequence.Split("\n%\n");
        ItemLink.ClearHistory();
        CallLine(0);


        // call next world actions
        onGoing = false;
        if ( stack.Count > 0) {
            Debug.Log($"call next world action");
            var next = stack[0];
            stack.RemoveAt(0);
            next.Call(next.source);
        } else {

            Debug.Log($"Finshed world actions, delayed items count : {ItemDescription.delayedItems.Count}");
            PropertyDescription.Describe();
            if ( ItemDescription.delayedItems.Count > 0) {
                string endDescription = ItemDescription.NewDescription(ItemDescription.delayedItems, "");
                TextManager.Write(endDescription);
                ItemDescription.delayedItems.Clear();

            }
        }
    }

    void CallLine(int i) {
        var seq = sequences[seqIndex];
        var lines = seq.Split('\n');
        if (string.IsNullOrEmpty(lines[i])) {
            Debug.Log($"skip line");
            goto Skip;

        }
        bool AllFailed = true;
        feedback = "";
        Debug.Log($"world action items count : {_items.Count}");
        foreach (var item in _items) {
            Debug.Log($"try func : {item.debug_name}");
            failed = false;
            Function.TryCall(lines[i], item);
            if (!failed)
                AllFailed = false;
        }
        if (AllFailed) {
            Debug.Log($"all failed");
            // try next sequence
            ++seqIndex;
            // if it's the last one, show feedback
            if (seqIndex == sequences.Length) {
                Debug.Log($"[WORLD ACTION]: {feedback}");
                Debug.Log($"[LINE] : {lines[i]}");
                TextManager.Write(feedback, Color.red);
                return;
            }
            Debug.Log($"calling next sequence");
            CallLine(0);
            return;
        }

        Skip:
        // end of sequence
        if (i + 1 == lines.Length)
            return;
        // call next line
        CallLine(i + 1);
    }

    public void Fail(string message) {
        failed = true;
        feedback = message;
    }
    public void Stop(string message) {
        Debug.LogError($"STOP : {message}");
        stopped = true;
        feedback = message;
    }
    #endregion

}
