using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WorldAction {

    public static List<WorldAction> stack = new List<WorldAction>();
    public string debug_name;

    public DataDownloader[] dataDownloaders;

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
    public bool goToNextPart = false;
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
        Function.LOG($"\n\n\nWorld Action:[{TargetItem().debug_name}] {source}", Color.cyan);
        AvailableItems.UpdateItems();

        current = this;
        onGoing = true;
        seqIndex = 0;
        sequences = sequence.Split("\n%\n");
        CallLine(0);

        onGoing = false;
        if ( stack.Count > 0) {
            var next = stack[0];
            stack.RemoveAt(0);
            Function.LOG($"World Action Succeeded.", Color.green);
            next.Call(next.source);
        } else {
            Function.LOG($"World Action Finished.", Color.white);
            PropertyDescription.Describe();
            if ( ItemDescription.delayedItems.Count > 0) {
                string endDescription = ItemDescription.DescribeItems(ItemDescription.delayedItems);
                TextManager.Write($"{ItemDescription.delayedSetup}{endDescription}");
                ItemDescription.delayedSetup = "";
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
        if (lines[i].StartsWith('~')) {
            Debug.Log($"reached ~~");
            goToNextPart = false;
            goto Skip;
        }
        if (goToNextPart) {
            Debug.Log($"going to next ~~");
            goto Skip;
        }

        bool tryNextSequence = false;
        feedback = "";
        foreach (var item in _items) {
            failed = false;
            Function.TryCall(lines[i], item);

            if (failed) {
                Function.LOG($"World Action Failed.", Color.red);
                tryNextSequence = true;
                break;
            }
        }
        if (tryNextSequence) {
            // try next sequence
            ++seqIndex;
            // if it's the last one, show feedback
            if (seqIndex == sequences.Length) {
                TextManager.Write(feedback, Color.red);
                return;
            }
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
