using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInput : MonoBehaviour {

	public static DisplayInput Instance;

	public InputField inputField;

    public string text;
    public List<string> inputParts;

    void Awake () {
		Instance = this;
	}

	void Start () {
		Hide ();
	}

	public void Show (){
		gameObject.SetActive (true);

    }
	public void Hide (){
		gameObject.SetActive (false);
    }

    void HandleOnStopTyping ()
	{
		Show ();
		Focus ();
	}

	void HandleOnStartTyping ()
	{
		Hide ();
	}

    public void OnEndEdit () {

            // get text from  input field
		text = inputField.text;

		if (text.Length == 0)
        {
            return;
        }

        // separate text
        inputParts = text.Split(new char[2] { ' ', '\'' }).ToList<string>();

        // create action
        InputInfo newInputInfo = new InputInfo();

        InputInfo.SetCurrent(newInputInfo);

        newInputInfo.FindVerb();

        newInputInfo.FindOrientation();

        newInputInfo.FindItems();

        newInputInfo.FindCombination();

        ActionManager.Instance.DisplayInputFeedback();

		Clear ();

    }

    public string[] SplitInWordGroups(string[] args)
    {
        string[] phraseParts = new string[args.Length];
        for (int a = 0; a < args.Length; a++)
        {
            string s = "";

            for (int i = a; i < args.Length; i++)
            {
                s += args[i];

                if (i < args.Length - 1)
                {
                    s += " ";
                }
            }

            phraseParts[a] = s;
        }

        return phraseParts;
    }

    public void EndInput()
    {
        inputField.interactable = false;
        inputField.enabled = false;
        inputField.text = "";
    }

    public void OnValueChanged () {
		//Sound.Instance.PlayRandomTypeSound ();
	}

	void Clear ()
	{
		inputField.text = "";
		Focus ();
	}
	void Focus () {

		inputField.Select ();
        inputField.ActivateInputField();

	}
}
