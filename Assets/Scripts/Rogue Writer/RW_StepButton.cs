using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StepButton : Displayable, IPointerClickHandler {

    public TextMeshProUGUI ui_Name;

    public Transform _parent;
    private int _profileIndex;

    private Step _step;

    public void Display(Step step) {
        _step = step;

        if ( _step._data == null)
        {
            Debug.LogError($"No step data.");   
            ui_Name.text = "?";
            return;
        }

        ui_Name.text = _step._stepType;

        _profileIndex = 0;
        DisplayProfile(_profileIndex);

        Show();

        /*foreach (var slot in step._slots) {
            var newSlotButton = RW_PoolManager.Instance.Pull("SlotButton", _parent);
            newSlotButton.Display(slot);
        }*/
    }

    void DisplayProfile(int profileIndex) {
        var prof = _step._data._profiles[0];

        // display slots from excel (?)
        for (int i = 0; i < _step._slots.Count; i++) {
            var slot = _step._slots[i];
            var newSlotButton = RW_PoolManager.Instance.Pull("SlotButton", _parent) as RW_SlotButton;
            newSlotButton.Display(slot, prof._slotDatas[i]._type);
        }

        // display slots datas ( types etc.. )
        /*foreach (var slotData in prof._slotDatas) {
            var newSlotButton = RW_PoolManager.Instance.Pull("SlotButton", _parent);
            newSlotButton.Display(slotData);
        }*/
    }

    public void ChangeProfile() {
        Hide();
        ++_profileIndex;
        _profileIndex = _profileIndex % _step._data._profiles.Count;
        Debug.Log($"Profil : {_profileIndex}/{_step._data._profiles.Count}");
        DisplayProfile(_profileIndex);
        Show();
    }

    public void OnPointerClick(PointerEventData eventData) {
        
    }

    private void OnDisable() {
        RW_PoolManager.Instance.Push("StepButton", this);
        Hide();
    }
}
