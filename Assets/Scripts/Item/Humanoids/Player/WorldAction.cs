using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class WorldAction {
    public bool skip = false;
    public enum Source {
        PlayerAction,
        Event,
    }

    // Static
    public static List<WorldAction> debug_list = new List<WorldAction>();

    public List<Line> lines = new List<Line>();
    public bool debug_selected = false;
    public static List<ItemLink.ItemHistory> history = new List<ItemLink.ItemHistory>();

    public static WorldAction current;
    public List<WorldAction> nestedActions = new List<WorldAction>();

    public Source source;
    private Item item;

    public static int breakCount = 20;
    static int currentBreak = 0;
    public static WorldAction pendingAction = null;
    public static bool finishedAllSequences = true;


    /// <summary>
    /// SEQUNCES
    /// </summary>
    public int sequenceIndex = 0;
    public string content;
    public bool stopped = false;
    public bool failed = false;
    public bool timeAction = false;
    public string debug_additionalInfo;
    public bool origin = false;
    public int lineIndex;
    static int globalIndex;
    public int index;

    public string Name {
        get {
            Color c = lines.Find(x => x.failed) != null ? Color.red : timeAction ? Color.cyan : source == WorldAction.Source.PlayerAction ? Color.yellow: Color.white;

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

        pendingAction = null;
        // debug
        if ( current != null) {
            current.lines[current.lineIndex].content += $" <color=magenta>(Call : {Name})</color>";

            // stack
            if (TargetItem().debug_name == "time") {
                pendingAction = this;
                timeAction = true;
                return;
            }
        } else {
            origin = true;
        }


        debug_list.Add(this);

        // special
        this.source = source;
        
        // get & clear items
        AvailableItems.UpdateItems();
        history.Clear();

        CallFirstLine();
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
        current = this;

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
        current = null;

        if (origin) {

            if (pendingAction != null) {
                if (ItemDescription.DescriptionPending()) {
                    skip = false;
                    currentBreak = 0;
                    ItemDescription.StartDescription();
                    WorldActionManager.Instance.DelaySequence(pendingAction);
                } else {
                    WorldActionManager.Instance.QuickDelaySequence(pendingAction);
                    pendingAction.skip = true;
                    if (timeAction) skip = true;
                }

                var secondsLeft = WorldData.GetGlobalItem("time").GetProp("seconds passed").GetNumValue();
                DisplayInput.Instance.DisplayFeedback($"time left : {secondsLeft}");

            } else {

                finishedAllSequences = true;
                if (ItemDescription.DescriptionPending()) {
                    ItemDescription.StartDescription();
                }

                DisplayInput.Instance.Enable();
            }

            

        }

        
    }

    public void Error(string message) {

    }
    #endregion
}
