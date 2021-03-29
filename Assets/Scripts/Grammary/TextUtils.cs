using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextUtils
{
    public static string FirstLetterCap(string str)
    {
        return str[0].ToString().ToUpper() + str.Remove(0, 1).ToLower();
    }
}
