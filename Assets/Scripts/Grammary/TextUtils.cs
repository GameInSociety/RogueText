using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextUtils {

    public static string WordWithNumber(string word, int number) {
        if (number <= 1)
            return word;
        return word + "s";

    }

    public static string Extract(char c, string text) {
        char endC = ')';
        switch (c) {
            case '(':
                endC = ')';
                break;
            case '{':
                endC = '}';
                break;
            case '[':
                endC = ']';
                break;
        }
        int startIndex = text.IndexOf(c) + 1;
        text = text.Remove(0, startIndex);
        int endIndex = text.IndexOf(endC);
        text = text.Remove(endIndex);
        return text;
    }
    public static string GetSpaces(int index, int lenght) {
        if (index == lenght - 1 || lenght == 1)
            return "";
        return "";
    }
    public static string GetCommas(int index, int lenght, bool useAnd = true) {
        if (index == lenght - 1 || lenght == 1)
            return "";
        if (useAnd && (lenght == 2 || index == lenght - 2))
            return " and ";
        else if (index < lenght - 2)
            return ", ";
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
