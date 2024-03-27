using System;
using UnityEngine;

public class TextUtils {

    public static string WordWithNumber(string word, int number) {
        if (number <= 1)
            return word;
        return word + "s";

    }

    public static bool GetCondition(string text, int matchValue) {

        int value = int.Parse(text.Remove(0, 1));

        switch (text[0]) {
            case '=':
                return value == matchValue;
            case '>':
                return value > matchValue;
            case '<':
                return value < matchValue;
        }

        UnityEngine.Debug.LogError($"GetCondition: no case in text {text}");
        return false;
    }

    public static string Extract(char c,string inText, out string outText) {
        try {
            char endC = c;
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
            int startIndex = inText.IndexOf(c);
            var content = inText.Remove(0, startIndex + 1);
            content = content.Remove(content.IndexOf(endC));
            outText = inText.Remove(startIndex, content.Length + 2);
            return content;
        } catch (Exception e) {
            Debug.LogError($"extracting char {c} from text {inText}");
            Debug.LogException(e);
            outText = inText;
            return inText;
        }
    }
    public static string GetSpaces(int index, int lenght) {
        if (index < lenght - 1)
            return " ";
        return "";
    }
    public static string GetCommas(int index, int lenght, bool useAnd = true) {
        if (index == lenght - 1 || lenght == 1)
            return "";
        if ((lenght == 2 || index == lenght - 2))
            return useAnd ? " and " : ", ";
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
