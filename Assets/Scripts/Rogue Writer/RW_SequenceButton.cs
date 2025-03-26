using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RW_SequenceButton : RW_Draggable, IPointerClickHandler {

    public TextMeshProUGUI ui_Name;
    private Sequence m_sequence;

    public Sequence Sequence {
        get => m_sequence;
    }

    public void Display(Sequence sequence) {
        Show();
        m_sequence = sequence;
        ui_Name.text = "";
        foreach (var item in Sequence.triggers)
            ui_Name.text += $"{Sequence.triggers[0]}";
    }

    public override void Copy() {
        base.Copy();
        DraggableManager.Instance.Copy(this, m_sequence.name);
    }
    private void OnDisable() {
        RW_PoolManager.Instance.Push("SequenceButton", this);
        Hide();
    }

    public override void Select() {
        base.Select();
        var displaySequence = RW_PoolManager.Instance.Pull("DisplaySequence") as RW_DisplaySequence;
        displaySequence.Display(Sequence);
    }
}
