using NUnit.Compatibility;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RW_DragClone : RW_Draggable
{
    public TextMeshProUGUI uiText;

    public void Display(string text) {
        Show();
        uiText.text = text;
    }

    public override bool CanMerge(RW_Draggable target) {

        var slotButton = target as RW_SlotButton;
        if (slotButton != null) {
            Debug.Log($"Merging with slot");
            return true;
        } else {
            Debug.Log($"That's not a Slot Buttton");
        }

        return base.CanMerge(target);

    }

    public override void Merge(RW_Draggable target) {
        base.Merge(target);

        
    }
}
