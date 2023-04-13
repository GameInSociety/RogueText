using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextUtils
{
    public static string FirstLetterCap(string str)
    {
        if ( str.Length < 2)
        {
            return str;
        }

        bool code = false;

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];

            if ( c == '<')
            {
                code = true;
                continue;
            }

            if (code)
            {
                if ( c == '>')
                {
                    code = false;
                }
                continue;
            }

            if (char.IsLetter(c))
            {
                str = str.Insert(i, str[i].ToString().ToUpper());
                str = str.Remove(i + 1, 1);
                return str;
            }
        }

        return str;
    }
}
