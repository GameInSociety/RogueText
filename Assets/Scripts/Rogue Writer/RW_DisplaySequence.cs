using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RW_DisplaySequence : RW_Menu
{
    public TextMeshProUGUI uiText_Name;
    public RectTransform _parent;

    public void Display(Sequence sequence) {

        // debug
        string sequenceName = "";
        foreach (var item in sequence.triggers) {
            sequenceName += $" {item}";
        }
        sequence.name = sequenceName;
        uiText_Name.text = sequenceName;

        foreach (var step in sequence.steps) {
            var stepButton = RW_PoolManager.Instance.Pull("StepButton", _parent) as StepButton;
            stepButton.Display(step);
        }

        UpdateDisplay();
    }

    public override void Hide() {
        base.Hide();
        RW_PoolManager.Instance.Push("DisplaySequence", this);
    }

}
