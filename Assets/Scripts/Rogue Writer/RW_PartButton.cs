using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RW_PartButton : Displayable
{
    public TextMeshProUGUI ui_NameText;
    public TextMeshProUGUI ui_ValueText;
    public TextMeshProUGUI ui_DescriptionText;

    public void Display(Property.Part part) {
        ui_NameText.text = part.key;
        ui_ValueText.text = part.content;
    }

    private void OnDisable() {
        RW_PoolManager.Instance.Push("PartButton", this);
        Hide();
    }
}
