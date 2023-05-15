using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInput : MonoBehaviour {

	public static DisplayInput Instance;

	public InputField inputField;

    void Awake () {
		Instance = this;
	}

	void Start () {
		//Hide ();
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

        InputInfo.Instance.ParseText(inputField.text);
        Clear();
    }

    public void EndInput()
    {
        inputField.interactable = false;
        inputField.enabled = false;
        inputField.text = "";
    }

    public void OnValueChanged () {
		Sound.Instance.PlayRandomTypeSound ();
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
