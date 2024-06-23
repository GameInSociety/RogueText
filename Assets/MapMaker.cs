using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour
{
    public string phrase = "";
    public string result = "";
    public string feedback = "";

    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();

    public List<string> results = new List<string>();

    int outB = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update() {
        outB = 0;
        result = Replace(phrase);
        
    }

     string Replace(string input) {
        int startIndex = 0;
        while ((startIndex = input.IndexOf('[', startIndex)) != -1) {

            ++outB;
            if (outB >= 10) {
                Debug.LogError($"out of look");
                return "";
            }

            int endIndex = FindClosingParenthesis(input, startIndex);
            if (endIndex == -1)
                break;

            string extracted = input.Substring(startIndex + 1, endIndex - startIndex - 1);
            string replace = extracted;
            if (extracted.Contains('[')) {
                replace = Replace(extracted);
            }
            int valueIndex = keys.FindIndex(x => x == extracted);
            if (valueIndex >= 0) {
                input = input.Remove(startIndex, extracted.Length + 2);
                input = input.Insert(startIndex, values[valueIndex]);
            } else {
                input = input.Remove(startIndex, extracted.Length+2);
                input = input.Insert(startIndex, replace);
            }

            startIndex = FindClosingParenthesis(input, startIndex) + 1;
        }

        return input;
    }

    static int FindClosingParenthesis(string input, int openIndex) {
        int closeIndex = openIndex;
        int counter = 1;

        while (counter > 0 && ++closeIndex < input.Length) {
            if (input[closeIndex] == '[')
                counter++;
            else if (input[closeIndex] == ']')
                counter--;
        }

        return counter == 0 ? closeIndex : -1;
    }

}
