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

    public ItemGroup itemGroup;
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
        itemGroup = new ItemGroup(0, Word.Number.Singular);
        itemGroup.debug_name = item.debug_name;
        itemGroup.items = new List<Item> { item };
        this.tileInfo = info;
        this.sequence = sequence;
    }
    public WorldAction(ItemGroup itemGroup, Tile.Info info, string sequence) {
        debug_name = $"{itemGroup.first.debug_name})";
        this.itemGroup = itemGroup;
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
            var next = stack[0];
            stack.RemoveAt(0);
            next.Call(next.source);
        } else {
            PropertyDescription.Describe();
        }
    }

    void CallLine(int i) {
        var seq = sequences[seqIndex];
        var lines = seq.Split('\n');
        feedback = "";
        failed = false;
        Function.TryCall(this, lines[i]);
        if (failed) {
            // try next sequence
            ++seqIndex;
            // if it's the last one, show feedback
            if (seqIndex == sequences.Length) {
                Debug.Log($"[WORLD ACTION]: {feedback}");
                Debug.Log($"[LINE] : {lines[i]}");
                TextManager.Write(feedback);
                return;
            }
            CallLine(0);
            return;
        }
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
