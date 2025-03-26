using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sequence {
    public struct Param {
        public Param(string k, string v) {
            key = k; value = v;
        }
        public string key;

        public string value;
    }

    // Static
    public static List<ItemLink.ItemHistory> history = new List<ItemLink.ItemHistory>();
    public static Sequence  parent;
    public static Sequence  active;
    public static bool      finishedAllSequences = true;

    // Params
    public string       name;                       // the name is given in the Writer by the editor, only for rangement.
    public string[]     triggers;                   // triggers can be verbs or events. a sequence with no triggers can be called with the trigger() step
    public int          duration = 0;               // time in seconds the sequence will pass. immediate if 0. can be ralonged par des sub-sequences.
    public bool         origin = false;             // is the sequence triggered withing another sequence.
    public List<Step>   steps = new List<Step>();   // all actions in sequence.
    public bool         skipping = false;           // will the sequence be skipped (?) pas sur
    public List<Param>  parameters;                 // string and numeral parameters ( change with Writer )
    public Source       source;                     // Where the sequence is initiated
    private Item        root_item;                  // the item from where the sequence is initiated

    /// <summary>
    /// sequence
    /// </summary>
    public string mContent; // the textual content of the sequence
    public int step_index; // the progression of the sequence in its actions.
    static int globalIndex; // ?
    public int index; // ?

    // flow 
    public State state; // reports errors or passes.
    public static int breakCount = 20;
    public string stop_feedback; 
    public string error_feedback;
    static int currentBreak = 0;

    // Only for debug display
    public List<Sequence> children = new List<Sequence>(); // only displayed, soon obsolete
    public static List<Sequence> debug_list = new List<Sequence>();
    public string debug_additionalInfo; // 
    public bool debug_selected = false;
    public int debug_count = 1;
    public bool debug_skipped = false;

    // Basic Constructor
    // THERE WONT BE CONTENT ANYMORE : no textual anything. ça veut dire les séquence seront toutes pretes à l'emploi.

    public Sequence(string content) {
        mContent = content; // assigning raw text (ob.)

        // spliting / parsing steps (ob.)
        steps = new List<Step>();
        foreach (var line in mContent.Split('\n')) {
            if (string.IsNullOrEmpty(line) || line.StartsWith('/'))
                continue;
            var newStep = new Step(line, this);
            steps.Add(newStep);
        }

    }

    // Old Constructor still used everywhere. Obsolete because holds item.
    public Sequence(Item item, string content) {
        this.root_item = item;
        mContent = content;

        // spliting / parsing steps (temp)
        steps = new List<Step>();
        foreach (var s in mContent.Split('\n')) {
            if (string.IsNullOrEmpty(s) || s.StartsWith('/'))
                continue;
            steps.Add(new Step(s, this));
        }
    }

    #region call
    public void InvokeSequence() {
        SequenceManager.Instance.InvokeSequence(this);
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
        // boot sequence
        step_index = 0;
        CallCurrentStep();
    }

    public void CallCurrentStep() {

        // set world action every line to keep track of current
        active = this;

        // parse current line
        var step = steps[step_index];

        if (step._content.StartsWith('-')) {
            step.state = Step.State.Skipped;
            if (step._content.StartsWith("--")) {
                EndSkipping();
                CallNextStep();
                return;
            } else {
                skipping = !skipping;
            }
            CallNextStep();
            return;
        }
        if (skipping) {
            step.state = Step.State.Skipped;
            CallNextStep();
            return;
        }

        step.Call();

        // unity error
        if ( step.state == Step.State.Error) {
            TextManager.Write($"ERROR : {step.error_feedback}");
            return;
        }

        // break sequence
        if (step.state == Step.State.Broken ) {
            if (step._continueOnFail) {
                step.state = Step.State.Skipped;
                CallNextStep();
                return;
            }
            state = State.Broken;
            stop_feedback = step.stop_feedback;

            if (source == Sequence.Source.PlayerAction) {
                TextManager.Write(step.error_feedback, Color.red);
            }
            return;
        }

        // call next line
        CallNextStep();
    }

    void CallNextStep() {
        if (steps[step_index].state == Step.State.None) { steps[step_index].state = Step.State.Done; }
        if (step_index + 1 == steps.Count) {
            EndSequence();
        } else {
            ++step_index;
            CallCurrentStep();
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

    public string Name {
        get {
            if (index == 0) {
                ++globalIndex;
                index = globalIndex;
            }

            //string name = $" [{TargetItem()._debugName} ({index})] : {debug_additionalInfo}";
            string name = $"<color=magenta>{TargetItem()._debugName}</color>";
            return $"{name}";
        }
    }


    public Item TargetItem() {
        return root_item;
    }

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
}
