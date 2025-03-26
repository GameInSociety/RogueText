using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SocketPart : RW_Draggable
{
    public TextMeshProUGUI uiText;
    public string originalText;

    public override void Start() {
        base.Start();
        originalText = uiText.text;
    }

    
}
