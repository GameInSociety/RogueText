using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEditor.Progress;
public static class TextManager {

    #region write phrase
    public static void Return() {
        DisplayDescription.Instance.AddToDescription("\n");
    }
    public static void Write(string str, Color c) {
        DisplayDescription.Instance.AddToDescription($"\n{str}", c);
    }
    public static void Write(string str) {
        DisplayDescription.Instance.AddToDescription($"\n{str}");
    }
    #endregion
}