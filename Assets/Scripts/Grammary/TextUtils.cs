using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextUtils {
    public static string GetLink(int index, int lenght) {
        if (index == lenght - 1) {
            return "";
        }

        if (lenght == 1) {
            return "";
        }

        if (lenght == 2) {
            return " and ";
        }

        if (index == lenght - 2) {
            return " and ";
        }
        // dernier
        else if (index < lenght - 2) {
            return ", ";
        }

        return "";
    }

    public static string FirstLetterCap(string str) {
        if (str.Length < 2) {
            return str;
        }

        var code = false;

        for (var i = 0; i < str.Length; i++) {
            var c = str[i];

            if (c == '<') {
                code = true;
                continue;
            }

            if (code) {
                if (c == '>') {
                    code = false;
                }
                continue;
            }

            if (char.IsLetter(c)) {
                str = str.Insert(i, str[i].ToString().ToUpper());
                str = str.Remove(i + 1, 1);
                return str;
            }
        }

        return str;
    }

    public static string GetPropertyContainerText(Property property) {
        var phrases = new string[3]
        {
            "almost empty",
            Random.value < 0.5f ? "half full" : "half empty",
            "almost full",
        };

        var value = property.GetInt();

        if (value == 0) {
            return "empty of " + property.name;
        }

        if (value == property.value_max) {
            return "full of " + property.name;
        }

        var lerp = (float)value / property.value_max;

        var index = (int)(lerp * phrases.Length);
        index = Mathf.Clamp(index, 0, phrases.Length - 1);
        var text = phrases[index];
        return "" + text + " of " + property.name;
    }
}
