using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[System.Serializable]
public class Condition {
    public enum Progress {
        Normal,
        Concerning,
        Problematic,
        Critical
    }
    public enum Type {
        Health,
        Thirst,
        Hunger,
        Sleep,
    }

    public Type type;

    public int hour;
    public int rate;
    public Progress progress;
    public Color color = Color.white;

    public void Advance() {
        if (progress == Progress.Critical) {
            ConditionManager.GetInstance().GetCondition(Type.Health).Advance();
            return;
        }

        ++hour;
        if (hour == rate) {
            hour = 0;
            ++GetProgress;
        }
    }

    public void Change(int i) {
        GetProgress += i;
    }

    public void Remove(int i) {
        GetProgress -= i;
    }
    public string GetDebugText() {
        var str = type.ToString() + " : " + progress.ToString() + " (" + hour + " / " + rate + ")";

        var c = Color.Lerp(color, Color.white, 0.5f);

        var html = ColorUtility.ToHtmlStringRGB(c);
        str = "<color=#" + html + ">" + str + "</color>";

        return str;
    }

    public string GetDescription() {
        var keyWord = type.ToString().ToLower() + "_" + progress.ToString().ToLower();

        return TextManager.getItemWord(keyWord);
    }

    public Progress GetProgress {
        get => progress;

        set {
            progress = value;

            if (progress < 0)
                progress = Progress.Normal;

            if (progress > Progress.Critical)
                progress = Progress.Critical;
        }
    }
}
