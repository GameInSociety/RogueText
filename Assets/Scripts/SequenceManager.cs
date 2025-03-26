using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{
    public static SequenceManager Instance;

    public List<Sequence> stack = new List<Sequence>();

    public List<Sequence> debug_list = new List<Sequence>();
    public int breakIndex;
    public int breakLimit = 10;
    public bool finishedSequence = false;
    public bool onGoing = false;
    public bool skipLines = false;
    int _startSeconds;

    public bool interuptNextSequence = false;

    // fail
    public bool failed = false;
    public string feedback = "";

    public Sequence nextSequence = null;
    public bool waitingForInput = false;
    public List<Sequence> delayedSequences = new List<Sequence>();

    public delegate void OnWaitEnd();
    public OnWaitEnd onWaitEnd;

    private void Awake() {
        Instance = this;
    }


    private void Update() {

        if (DescriptionManager.Instance.DescriptionPending())
            DescriptionManager.Instance.StartDescription();

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

    #region waiting
    public void Wait(Item item, int secs) {
        _startSeconds = secs;
        TimeDebug.Instance.currentMax = _startSeconds;
        StartCoroutine(WaitCoroutine(item, secs));
    }

    IEnumerator WaitCoroutine(Item item, int secs) {
        Debug.Log($"Waiting : {secs} seconds");
        for (int i = 0; i < secs; i++) {
            DisplayTime(i);
            yield return new WaitForSeconds(1f);
            var timeSeq = $"add(!second>seconds, ${1})\ntriggerEvent(OnSeconds)";
            var timeAction = new Sequence(item, timeSeq);
            timeAction.StartSequence(Sequence.Source.Event);
        }
        TimeDebug.Instance.Hide();
        if (onWaitEnd != null)
            onWaitEnd();
    }
    #endregion

    public void InvokeSequence(Sequence action) {
        if ( delayedSequences.Count == 0)
            Invoke("InvokeSequenceDelay", 0f);

        delayedSequences.Add(action);
        
    }

    void InvokeSequenceDelay() {
        foreach (var s in delayedSequences) {
            s.StartSequence(Sequence.Source.Event);
        }
        delayedSequences.Clear();
    }

    public void NextSequence(Sequence sequence) {

        nextSequence = sequence;

        var secondsLeft = WorldData.GetGlobalItem("time").GetProp("seconds passed").GetNumValue();
        if ( secondsLeft > 3600) {
            breakLimit = 30;
        } else if (secondsLeft > 60) {
            breakLimit = 5;
        } else { breakLimit = 0; }

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
        Invoke("ContinueSequence", 1f);
    }

    void Interup() {
        interuptNextSequence =true;
    }

    public void PauseSequence(Sequence sequence) {
        TimeDebug.Instance.DisplayText("Interupted !");

        nextSequence = sequence;
        DisplayDescription.Instance.onTypeExit += Delay;
    }

    #region time display
    void DisplayTime(int secs) {
        TimeSpan t = TimeSpan.FromSeconds(secs);
        TimeDebug.Instance.Push(secs);

        string time_str = string.Format("{0:D2}h, {1:D2}m, {2:D2}s",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds);
        TimeDebug.Instance.DisplayText(time_str);
    }
    #endregion

    void Delay() {
        DisplayDescription.Instance.onTypeExit -= Delay;
        TimeDebug.Instance.DisplayText("Interupted ! continue ? ( y/n )");

        waitingForInput = true;

    }

    void ResumeGame() {
        //DisplayInput.Instance.Enable();
    }

    void ContinueSequence() {
        nextSequence.StartSequence(Sequence.Source.Event);
    }

}
