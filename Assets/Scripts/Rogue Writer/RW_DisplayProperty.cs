using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RW_DisplayProperty : RW_Menu
{
    public TextMeshProUGUI uiText_Name;
    public Transform group_Parts;
    public Transform group_Sequences;

    public void Display(Property property) {

        uiText_Name.text = property.name;

        group_Parts.gameObject.SetActive(property.parts.Count > 0);
        group_Sequences.gameObject.SetActive(property.sequences.Count > 0);

        foreach (var part in property.parts) {
            var partButton = RW_PoolManager.Instance.Pull("PartButton", group_Parts) as RW_PartButton;
            partButton.Display(part);
        }

        foreach (var seq in property.sequences) {
            var sequenceButton = RW_PoolManager.Instance.Pull("SequenceButton", group_Sequences) as RW_DisplaySequence;
            sequenceButton.Display(seq);
        }

        UpdateDisplay();
    }

    public override void Hide() {
        base.Hide();
        RW_PoolManager.Instance.Push("DisplayProperty", this);
    }
}
