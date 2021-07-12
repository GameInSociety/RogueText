using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDescription : MonoBehaviour {
	
	public static DisplayDescription Instance;
    public RectTransform verticalLayoutGroup;

    string newText = "";

    string currentDescription;

    public ScrollRect scrollRect;

    public float test;

    public Text uiText;
    public Text uiText_Old;

	void Awake () {
		Instance = this;
	}

    void Start()
    {
        uiText_Old.text = "";
        uiText.text = "";

        PlayerActionManager.onPlayerAction += HandleOnAction;
    }

    private void HandleOnAction(PlayerAction action)
    {
        if (action.type == PlayerAction.Type.LookAround)
        {
            UpdateDescription();
        }
    }

    public void ClearDescription()
    {
        uiText.text = "";
    }

    public void UpdateDescription()
    {
        string str = "";

        // display current tile
        str += Tile.current.GetDescription();
        str += "\n";

        // display surrounding tiles
        str += TileGroupDescription.GetSurroundingTileDescription();
        str += "\n";

        // display tile items
        str += Tile.current.GetItemDescriptions();

        // pas sûr que les choses d'état de santé, de temps et autre trucs divers doivent être là, pense à changer
        str += StateManager.GetInstance().GetDescription();

        // l'indication de la lettre
        if ( !Story.Instance.GetParam("retrieved_letter"))
        {
            str += "\n\nJ'ai laissé la lettre quelque part dans cette maison, mais je ne sais plus où...";
        }

        DisplayDescription.Instance.AddToDescription(str);
    }

    public void AddToDescription(string str)
    {
        str = Phrase.ReplaceItemInString(str);

        // archive
        uiText_Old.text += uiText.text;

        // clear
        uiText.text = "";

        // add
        uiText.text += "\n";
        uiText.text += "----------------";
        uiText.text += "\n";
        uiText.text += str;

        AudioInteraction.Instance.StartSpeaking(str);

        Invoke("AddToDescriptionDelay", 0.001f);
    }

    void AddToDescriptionDelay()
    {
        scrollRect.verticalNormalizedPosition = 0f;

    }

}
