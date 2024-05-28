using JetBrains.Annotations;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class WorldAction {

    public enum Source {
        PlayerAction,
        Event,
    }

    // Static
    public static List<WorldAction> debug_list = new List<WorldAction>();

    public List<Line> lines = new List<Line>();
    public static List<ItemLink.ItemHistory> history = new List<ItemLink.ItemHistory>();

    public static WorldAction parent;
    public bool origin = false;
    public static WorldAction active;

    public List<WorldAction> children = new List<WorldAction>();

    public Source source;
    private Item item;

    public static int breakCount = 20;
    static int currentBreak = 0;
    public static WorldAction nextTimeAction = null;
    public static bool finishedAllSequences = true;

    // debug
    public bool debug_selected = false;
    public int debug_count = 1;
    public bool debug_skipped = false;
    public bool interupted = false;


    /// <summary>
    /// SEQUNCES
    /// </summary>
    public int sequenceIndex = 0;
    public string content;
    public bool stopped = false;
    public bool failed = false;
    public bool IsTimeAction {
        get {
            return TargetItem().debug_name == "time";
        }
    }
    public string debug_additionalInfo;
    public int lineIndex;
    static int globalIndex;
    public int index;

    public string Name {
        get {
            Color c = lines.Find(x => x.failed) != null ? Color.red : IsTimeAction ? Color.cyan : source == WorldAction.Source.PlayerAction ? Color.yellow: Color.white;

            if ( index == 0) {
                ++globalIndex;
                index = globalIndex;
            }

            string name = $" [<color={c}>{TargetItem().debug_name} ({index})</color>] : {debug_additionalInfo}";
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
    public void StartSequence() {
        StartSequence(Source.Event);
    }
    public void StartSequence(Source source = Source.Event) {

        if (finishedAllSequences) {
            DisplayInput.Instance.Disable();
            finishedAllSequences = false;
        }
        

        nextTimeAction = null;
        if ( active != null) {
            // debug
            active.lines[active.lineIndex].content += $" <color=magenta>(Call : {Name})</color>";

            // stack
            if (IsTimeAction) {
                nextTimeAction = this;
                return;
            }
        } else {
            origin = true;
            parent = this;
        }

        if (origin)
            AddDebug();
        else
            parent.children.Add(this);

        // special
        this.source = source;
        
        // get & clear items
        AvailableItems.UpdateItems();
        history.Clear();

        CallFirstLine();
    }

    void AddDebug() {
        if (debug_list.Count > 0 && debug_list.Last().IsTimeAction && !debug_list.Last().interupted) {
            debug_list.Last().debug_count++;
            debug_additionalInfo = debug_list.Last().debug_count.ToString();
            debug_skipped = true;
        } else {
            debug_list.Add(this);
        }
        return;
    }

    void CallFirstLine() {
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
        line.Parse(this);

        // break sequence
        if (line.failed) {
            if (source == WorldAction.Source.PlayerAction)
                TextManager.Write(line.fail_feedback, Color.red);
            else
                Debug.LogError($"EVENT FAIL : {line.fail_feedback}");
            return;
        }

        // call next line
        CallNextLine();
    }

    void CallNextLine() {
        if (lineIndex + 1 == lines.Count) {
            EndSequence();
        } else {
            ++lineIndex;
            CallLine();
        }
    }

    void EndSequence() {
        // remove active
        active = null;
        
        // only try another sequence if active is main
        if (!origin)
            return;

        if (nextTimeAction != null) {
            NextTimeAction();
        } else
            EndSequences();
    }

    void NextTimeAction() {

        if (WorldActionManager.Instance.interuptNextSequence) {
            WorldActionManager.Instance.interuptNextSequence = false;
            interupted = true;
        }

        if (interupted) {
            // debug
            if (debug_skipped) {
                debug_list.Last().debug_count--;
                debug_list.Add(this);
            }
            currentBreak = 0;
            WorldActionManager.Instance.PauseSequence(nextTimeAction);
        } else {
            WorldActionManager.Instance.NextSequence(nextTimeAction);
        }
    }

    public void EndSequences() {
        active = null;
        finishedAllSequences = true;
        if (ItemDescription.DescriptionPending())
            ItemDescription.StartDescription();

        DisplayInput.Instance.Enable();
        Debug.Log($"end of all sequences !");
    }

    public void Error(string message) {

    }
    #endregion
}
