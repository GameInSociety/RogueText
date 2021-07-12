using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class State
{
    public enum Progress
    {
        Normal,
        Concerning,
        Problematic,
        Critical
    }

    public string name;
    public int hour;
    public int rate;
    public Progress progress;
    public string[] phrases;
    public Color color = Color.white;

    public void Advance()
    {
        if (progress == Progress.Critical)
        {
            StateManager.GetInstance().GetState(StateManager.StateType.Health).Advance();
            return;
        }

        ++hour;
        if (hour == rate)
        {
            hour = 0;
            ++GetProgress;
        }
    }

    public void Change(int i)
    {
        GetProgress += i;
    }

    public void Remove(int i)
    {
        GetProgress -= i;
    }

    public string GetFeedbackText()
    {
        return phrases[(int)progress];
    }

    public string GetDebugText()
    {
        string str = name + " : " + progress.ToString() + " (" + hour + " / " + rate + ")";

        Color c = Color.Lerp(color, Color.white, 0.5f);

        string html = ColorUtility.ToHtmlStringRGB(c);
        str = "<color=#" + html + ">" + str + "</color>";

        return str;
    }

    public string GetDescription()
    {
        string str = "";

        if (progress != Progress.Normal)
        {
            string phrase = "\n" + phrases[(int)progress - 1];

            str += phrase;
        }

        return str;
    }

    public Progress GetProgress
    {
        get
        {
            return progress;
        }

        set
        {
            progress = value;

            if (progress < 0)
                progress = Progress.Normal;

            if (progress > Progress.Critical)
                progress = Progress.Critical;
        }
    }
}
