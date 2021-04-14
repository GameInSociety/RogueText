#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Windows.Speech;

public class VoiceRecognition : MonoBehaviour
{
    public DictationRecognizer dictationRecognizer;
    //public KeywordRecognizer keywordRecognizer;
    public List<string> keywords = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        Invoke("Init", 0.0011f);
    }

    void Init()
    {
        for (int i = 0; i < Verb.GetVerbs.Count; i++)
        {
            Verb verb = Verb.GetVerbs[i];

            string keyword = verb.names[0];

            keywords.Add(keyword);
        }

        for (int i = 0; i < Item.items.Count; i++)
        {
            Item item = Item.items[i];

            string keyword = item.word.text;

            keywords.Add(keyword);
        }

        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += HandleOnDictationResult;
        dictationRecognizer.Start();

        /*keywordRecognizer = new KeywordRecognizer(keywords.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeed;
        keywordRecognizer.Start();*/
    }

    private void HandleOnDictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log(text);
        DisplayInput.Instance.ParsePhrase(text);
    }
}
#endif