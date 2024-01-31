using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    public string inText = "this is the [value] of the {item} with the ?properties?";
    public string outText = "";
    public string content;
    public char sC;
    public char eC;

    private void OnDrawGizmos() {
        int startIndex = inText.IndexOf(sC);
        content = inText.Remove(0, startIndex + 1);
        content = content.Remove(content.IndexOf(eC));
        outText = inText.Remove(startIndex, content.Length+(startIndex+content.Length+3>inText.Length?2:3));
    }
}
