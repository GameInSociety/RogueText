using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextManager : MonoBehaviour {

    public static TextManager Instance;

    public List<char> colorCharacters = new List<char>();
    public Color[] colors;

    private void Awake()
    {
        Instance = this;
    }

    public char GetColorChar (TextColor c)
    {
        return colorCharacters[(int)c];
    }

    public static string WithCaps ( string str)
    {
        return str[0].ToString().ToUpper() + str.Remove(0, 1).ToLower();
    }

}

public enum TextColor
{
    Red,
    Green,
    Blue,
    Pink,
    Yellow,

    None,
}
