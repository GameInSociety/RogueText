using UnityEngine;
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