using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class WorldActionManager : MonoBehaviour
{
    public static WorldActionManager Instance;

    public List<WorldAction> stack = new List<WorldAction>();
    public List<WorldAction> debug_list = new List<WorldAction>();
    public int breakIndex;
    public bool finishedSequence = false;
    public bool onGoing = false;
    public bool skipLines = false;

    // fail
    public bool failed = false;
    public string feedback = "";

    // sequence
    string[] sequences;
    string currentSequence;
    int sequenceIndex = 0;

    public WorldAction nextSequence = null;

    private void Awake() {
        Instance = this;
    }

    public void QuickDelaySequence(WorldAction sequence) {
        nextSequence = sequence;
        Invoke("Continue", 0f);
    }
    public void DelaySequence(WorldAction sequence) {
        nextSequence = sequence;
        DisplayDescription.Instance.onTypeExit += Delay;
    }

    void Delay() {
        DisplayDescription.Instance.onTypeExit -= Delay;
        DisplayInput.Instance.DisplayFeedback("continue?");
        DisplayDescription.Instance.onPressReturn += Continue;
    }

    void Continue() {
        DisplayDescription.Instance.onPressReturn = null;
        nextSequence.StartSequence();
    }

    public bool PendingDescription() {
        return ItemDescription.itemDescriptions.Count > 0;
    }

    public void CallNextAction() {
        var next = stack[0];
        stack.RemoveAt(0);
        next.StartSequence(next.source);
    }


    public void StackWorldAction(WorldAction worldAction) {
        stack.Add(worldAction);
    }

}
