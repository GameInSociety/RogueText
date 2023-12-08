using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
public class TextTyper : MonoBehaviour {

    public static bool debug = true;

	public GameObject group;
	public Text uiText;

    public string textToType = "";
    // currently typed seq ?
    private string typingText = "";
    public bool finishedTyping = false;

    // time between letter group
    public float typeRate = 0.01f;
    // letters typed
    public int letterRate = 3;

    // current character
    private int characterIndex = 0;

    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";

    // color typing
    private bool colorTyping = false;
    private int colorcharacterIndex = -1;

	public void Clear () {
		textToType = "";
		uiText.seq = "";
	}

	public virtual void Start () {

		uiText = group.GetComponentInChildren<Text> ();

        Clear();
         
	}

    public IEnumerator TypeCoroutine () {

        while ( true ) {

            for (int i = 0; i < letterRate; i++)
            {
                TypeCharacter();

                if (characterIndex >= textToType.Length)
                {
                    break;
                }
            }

            uiText.seq = typingText + "_";

            Sound.Instance.PlayRandomTypeSound();

            if (!debug)
            {
                yield return new WaitForSeconds(typeRate);
            }

            if (characterIndex >= textToType.Length)
            {
                uiText.seq = typingText;
                break;
            }
        }

		Sound.Instance.PlayRandomComputerSound ();

		finishedTyping = true;

        if ( debug)
        {
            yield return new WaitForEndOfFrame();
        }


    }

    public void Hide()
    {
        group.SetActive(false);
        finishedTyping = true;
    }

    public void UpdateAndDisplay()
    {
        UpdateCurrentTileDescription();
        Display();
    }

    void TypeCharacter()
    {
        char currentChar = textToType[characterIndex];

        if (currentChar == '<')
        {
            char colorCharacter = textToType[characterIndex + 1];

            colorcharacterIndex = TextManager.Instance.colorCharacters.FindIndex(x => x == colorCharacter);

            colorTyping = true;

            textToType = textToType.Remove(characterIndex, 2);
            return;
        }

        if (currentChar == '>')
        {
            colorTyping = false;

            textToType = textToType.Remove(characterIndex, 1);
            return;
        }

        if (colorTyping)
        {
            string part = "" + textToType[characterIndex];

            string htmlColor;

            if ( colorcharacterIndex >= TextManager.Instance.colors.Length)
            {
                htmlColor = ColorUtility.ToHtmlStringRGB(Color.green);
            }
            else
            {
                htmlColor = ColorUtility.ToHtmlStringRGB(TextManager.Instance.colors[colorcharacterIndex]);
            }

            typingText += "<color=#" +
                htmlColor +
                ">" + part + "</color>";
        }
        else
        {
            string part = "" + textToType[characterIndex];


            typingText += part;
        }

        NextCharacter();

    }

    void NextCharacter()
    {
        ++characterIndex;
    }

    public string WithCaps (string str)
    {
        return str [0].ToString ().ToUpper () + str.Remove (0,1).ToLower();
    }

    public void Display()
    {
        Display(textToType);
    }

    public virtual void Display ( string str ) {

        StopCoroutine(TypeCoroutine());

        textToType = str;
		UpdateText ();
	}

    public virtual void UpdateCurrentTileDescription()
    {
        Debug.Log("displaying: " + name);
    }

	public void UpdateText () {

		uiText.seq = "";

		if (textToType.Length < 1) {
            Hide();
			return;
		}

        group.SetActive(true);

        finishedTyping = false;
        characterIndex = 0;
        typingText = "";

        StartCoroutine(TypeCoroutine());

    }

	public void QuickUpdateText () {

		uiText.seq = textToType;
		textToType = "";
	}
}
*/