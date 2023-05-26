using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextUtils
{
    // a lot of apples, an apple, 4 apples etc...
    public static string GetItemCountText(Item item, int count, string key)
    {
        if (count > 5)
        {
            return "some";
        }
        else if (count > 3)
        {
            return "a few " + item.word.GetInfo("dogs");
        }
        else if (count > 1)
        {
            return count + " " + item.word.GetInfo("dogs");
        }
        else
        {
            return item.word.GetInfo(key);
        }
    }

    public static string GetLink(int index, int lenght)
    {
        if ( index == lenght - 1)
        {
            return "";
        }

        if (lenght == 1)
        {
            return "";
        }

        if (lenght == 2)
        {
            return " and ";
        }

        if (index == lenght - 2)
        {
            return " and ";
        }
        // dernier
        else if (index < lenght - 2)
        {
            return ", ";
        }

        return "";
    }

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

    public static string GetPropertyContainerText(Property property)
    {
        string[] phrases = new string[3]
        {
            "almost empty",
            Random.value < 0.5f ? "half full" : "half empty",
            "almost full",
        };

        int value = property.GetInt();

        if (value == 0)
        {
            return "empty of " + property.name;
        }

        if (value == property.value_max)
        {
            return "full of " + property.name;
        }

        float lerp = (float)value / property.value_max;

        int index = (int)(lerp * phrases.Length);
        index = Mathf.Clamp(index, 0, phrases.Length - 1);
        string text = phrases[index];
        return "" + text + " of " + property.name;
    }
}
