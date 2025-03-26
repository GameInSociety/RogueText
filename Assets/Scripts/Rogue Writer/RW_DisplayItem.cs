using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RW_DisplayItem : RW_Menu {

    public TextMeshProUGUI uiText_Name;
    public ItemData _itemData;
    public RectTransform[] groups;

    public void Display(ItemData itemData) {

        _itemData = itemData;

        // display name
        uiText_Name.text = _itemData.name;

        // groups 
        groups[0].gameObject.SetActive(_itemData.roots.Count > 0);
        groups[1].gameObject.SetActive(_itemData.properties.Count > 0);
        groups[2].gameObject.SetActive(_itemData.sequences.Count > 0);
        groups[3].gameObject.SetActive(_itemData.verbSequences.Count > 0);

        // display Properties
        foreach (var rootItemData in _itemData.roots) {
            var newButton = RW_PoolManager.Instance.Pull("RootItemDataButton", groups[0]) as RW_ItemDataButton;
            newButton.Display(rootItemData);
        }

        // display Properties
        foreach (var prop in _itemData.properties) {
            var propButton = RW_PoolManager.Instance.Pull("PropertyButton", groups[1]) as RW_PropertyButton;
            propButton.Display(prop);
        }

        // display Sequences
        foreach (var seq in _itemData.sequences) {
            var sequenceButton = RW_PoolManager.Instance.Pull("SequenceButton", groups[2]) as RW_SequenceButton;
            sequenceButton.Display(seq);
        }

        // display Sequences
        foreach (var seq in _itemData.verbSequences) {
            var sequenceButton = RW_PoolManager.Instance.Pull("SequenceButton", groups[3]) as RW_SequenceButton;
            sequenceButton.Display(seq);
        }

        UpdateDisplay();
    }

    public void Rebuild() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetRectTransform);
        foreach (var group in groups)
            LayoutRebuilder.ForceRebuildLayoutImmediate(group);
    }
    public void Open_Grammar() {

    }

    public override void Hide() {
        base.Hide();
        RW_PoolManager.Instance.Push("DisplayItem", this);
    }
}
