using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextUtils {
    public static string GetLink(int index, int lenght, bool useAnd = true) {
        if (index == lenght - 1) {
            return "";
        }

        if (lenght == 1) {
            return "";
        }

        if (lenght == 2 && useAnd) {
            return " and ";
        }

        if (index == lenght - 2 && useAnd) {
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
}
