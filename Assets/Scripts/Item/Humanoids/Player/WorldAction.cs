using JetBrains.Annotations;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class WorldAction {

    public enum Source {
        PlayerAction,
        Event,
    }
    public enum State {
        None,
        Done,
        Broken,
        Paused,
        Error,
    }
    public bool skipping = false;
    public struct Param {
        public Param(string k, string v) {
            key = k; value = v;
        }
        public string key;

        public string value;
    }

    // Static

    public List<Line> lines = new List<Line>();
    public static List<ItemLink.ItemHistory> history = new List<ItemLink.ItemHistory>();

    public static WorldAction parent;
    public bool origin = false;
    public static WorldAction active;

    // Only for debug display
    public List<WorldAction> children = new List<WorldAction>();
    
    // parameters
    public List<Param> parameters;
   

    // world action source
    public Source source;

    // target item
    private Item item;

    // flow 
    public State state;
    public static int breakCount = 20;
    public string stop_feedback;
    public string error_feedback;
    static int currentBreak = 0;
    public static bool finishedAllSequences = true;

    // debug
    public static List<WorldAction> debug_list = new List<WorldAction>();
    public bool debug_selected = false;
    public int debug_count = 1;
    public bool debug_skipped = false;


    /// <summary>
    /// SEQUNCES
    /// </summary>
    public int sequenceIndex = 0;
    public string content;
    public string debug_additionalInfo;
    public int lineIndex;
    static int globalIndex;
    public int index;

    public string Name {
        get {
            if ( index == 0) {
                ++globalIndex;
                index = globalIndex;
            }

            //string name = $" [{TargetItem()._debugName} ({index})] : {debug_additionalInfo}";
            string name = $"<color=magenta>{TargetItem()._debugName}</color> {debug_additionalInfo}";
            return $"{name}";
        }
    }


    public Item TargetItem() {
        return item;
    }

    public WorldAction(Item item, string content, string additionalinfo) {
        this.item = item;
        this.content = content;
        debug_additionalInfo = additionalinfo;
    }

    #region call
    public void InvokeSequence() {
        WorldActionManager.Instance.InvokeSequence(this);
    }
    public void StartSequence(Source source = Source.Event) {

        if (finishedAllSequences)
            finishedAllSequences = false;

        this.source = source;
        if ( active != null) {
            // debug
            active.children.Add(this);
        } else {
            origin = true;
            debug_list.Add(this);
            parent = this;
        }


        // get & clear items
        history.Clear();

        CallFirstLine();
    }

    void AddDebug() {
        
    }

    void CallFirstLine() {

        ItemLink.history.Clear();
        // creating lines
        lines = new List<Line>();
        foreach (var s in content.Split('\n')) {
            if (string.IsNullOrEmpty(s) || s.StartsWith('/'))
                continue;
            lines.Add(new Line(s, this));
        }
        // boot sequence
        lineIndex = 0;
        CallLine();
    }

    public void CallLine() {

        // set world action every line to keep track of current
        active = this;

        // parse current line
        var line = lines[lineIndex];

        if (line.content.StartsWith('-')) {
            line.state = Line.State.Skipped;
            if (line.content.StartsWith("--")) {
                EndSkipping();
                CallNextLine();
                return;
            } else {
                skipping = !skipping;
            }
            CallNextLine();
            return;
        }
        if (skipping) {
            line.state = Line.State.Skipped;
            CallNextLine();
            return;
        }

        line.Parse(this);

        // unity error
        if ( line.state == Line.State.Error) {
            TextManager.Write($"ERROR : {line.error_feedback}");
            return;
        }

        // break sequence
        if (line.state == Line.State.Broken ) {
            if (line.continueOnFail) {
                line.state = Line.State.Skipped;
                CallNextLine();
                return;
            }
            state = State.Broken;
            stop_feedback = line.stop_feedback;

            if (source == WorldAction.Source.PlayerAction) {
                TextManager.Write(line.error_feedback, Color.red);
            }
            return;
        }

        // call next line
        CallNextLine();
    }

    void CallNextLine() {
        if (lines[lineIndex].state == Line.State.None) { lines[lineIndex].state = Line.State.Done; }
        if (lineIndex + 1 == lines.Count) {
            EndSequence();
        } else {
            ++lineIndex;
            CallLine();
        }
    }

    void EndSequence() {
        // remove active
        state = State.Done;

        // only try another sequence if active is main
        if (!origin) {
            return;
        }

        EndSequences();
    }

    public void EndSequences() {
        active = null;
        finishedAllSequences = true;
    }

    public void Error(string message) {
        state = State.Error;
    }

    public void StartSkipping() {
        skipping = true;
    }
    public void EndSkipping() {
        skipping = false;
    }
    #endregion
}
