using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using UnityEditor;
using UnityEditor.Search;

public class RW_PropertyButton : RW_Draggable {
    public TextMeshProUGUI ui_NameText;
    private Property m_Property;

    public void Display(Property property) {
        Show();
        m_Property = property;
        ui_NameText.text = m_Property.name;
    }

    public override void Copy() {
        base.Copy();
        DraggableManager.Instance.Copy(this, m_Property.name);
    }


    private void OnDisable() {
        RW_PoolManager.Instance.Push("PropertyButton", this);
        Hide();
    }

    public override void Over_Start() {
        base.Over_Start();
    }

    public override void Over_Exit() {
        base.Over_Exit();
        // reset description
    }

    public override void Select() {
        base.Select();
        var displayProperty = RW_PoolManager.Instance.Pull("DisplayProperty") as RW_DisplayProperty;
        displayProperty.Display(m_Property);
    }

    public Property GetProperty() {
        return m_Property;
    }
}
