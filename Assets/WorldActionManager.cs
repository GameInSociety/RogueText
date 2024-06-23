using DG.Tweening;
using System;
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
    public int breakLimit = 10;
    public bool finishedSequence = false;
    public bool onGoing = false;
    public bool skipLines = false;

    public bool interuptNextSequence = false;

    // fail
    public bool failed = false;
    public string feedback = "";

    // sequence
    string[] sequences;
    string currentSequence;
    int sequenceIndex = 0;

    public WorldAction nextSequence = null;

    public List<WorldAction> delayedSequences = new List<WorldAction>();

    private void Awake() {
        Instance = this;
    }

    public bool waitingForInput = false;

    private void Update() {
        if (waitingForInput) {
            if (Input.GetKeyDown(KeyCode.Y)) {
                ContinueSequence();
            }
            if (Input.GetKeyDown(KeyCode.N)) {
                ResumeGame();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Interup();
        }

    }

    public void InvokeSequence(WorldAction action) {
        if ( delayedSequences.Count == 0) {
            Invoke("InvokeSequenceDelay", 0f);
        }

        delayedSequences.Add(action);
        
    }

    void InvokeSequenceDelay() {
        foreach (var s in delayedSequences) {
            s.StartSequence(WorldAction.Source.Event);
        }
        delayedSequences.Clear();
    }

    public void NextSequence(WorldAction sequence) {

        nextSequence = sequence;

        var secondsLeft = WorldData.GetGlobalItem("time").GetProp("seconds passed").GetNumValue();
        if ( secondsLeft > 3600) {
            breakLimit = 30;
        } else if (secondsLeft > 60) {
            breakLimit = 5;
        } else { breakLimit = 0; }

        DisplayTime();
        Invoke("ContinueSequence", 0f);

        /*if ( breakIndex >= breakLimit) {
            PauseSequence(nextSequence);
            breakIndex = 0;
        } else {
            ContinueSequence();
        }*/
        ++breakIndex;


    }

    public void PendSequence() {
        DisplayTime();
        ItemDescription.StartDescription();
            Debug.Log($"pending");
        Invoke("ContinueSequence", 1f);
    }

    void Interup() {
        interuptNextSequence =true;
    }

    public void PauseSequence(WorldAction sequence) {


        DisplayTime();
        ItemDescription.StartDescription();

        TimeDebug.Instance.DisplayText("Interupted !");

        nextSequence = sequence;
        DisplayDescription.Instance.onTypeExit += Delay;
    }

    void DisplayTime() {
        var secondsLeft = WorldData.GetGlobalItem("time").GetProp("seconds passed").GetNumValue();
        TimeSpan t = TimeSpan.FromSeconds(secondsLeft);
        TimeDebug.Instance.Push(secondsLeft);

        string time_str = string.Format("{0:D2}h, {1:D2}m, {2:D2}s",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds);
        TimeDebug.Instance.DisplayText(time_str);

    }

    void Delay() {
        DisplayDescription.Instance.onTypeExit -= Delay;
        TimeDebug.Instance.DisplayText("Interupted ! continue ? ( y/n )");

        waitingForInput = true;

    }

    void ResumeGame() {
        DisplayInput.Instance.Enable();
    }

    void ContinueSequence() {
        nextSequence.StartSequence();
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
