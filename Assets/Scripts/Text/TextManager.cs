using UnityEngine;
public static class TextManager {

    #region write phrase
    public static void Write(string str, Color c) {
        DisplayDescription.Instance.AddToDescription($"\n{str}", c);
    }
    public static void Write(string str) {
        DisplayDescription.Instance.AddToDescription($"\n{str}");
    }
    #endregion
}