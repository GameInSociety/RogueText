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
        ClearDescription();

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
        uiText_Old.text = "";
        uiText.text = "";
    }

    public void UpdateDescription()
    {
        PhraseKey.Renew();

        // CURRENT TILE
        if (Tile.GetPrevious != null && Tile.GetCurrent.type == Tile.GetPrevious.type)
        {

        }
        else
        {
            PhraseKey.Write("tile_current_description");
        }

        // SURROUNDING TILES
        SurroundingTileManager.WriteSurroundingTileDescription();

        // display tile items
        Tile.GetCurrent.WriteItemDescription();

        // pas sûr que les choses d'état de santé, de temps et autre trucs divers doivent être là, pense à changer
        ConditionManager.GetInstance().WriteDescription();

        // time of day
        TimeManager.GetInstance().WriteDescription();

        // weather
        TimeManager.GetInstance().WriteWeatherDescription();

        // l'indication de la lettre
        /*if ( !Story.Instance.GetParam("retrieved_letter"))
        {
            str += "\n\nJ'ai laissé la lettre quelque part dans cette maison, mais je ne sais plus où...";
        }*/
    }

    public void Renew()
    {
        uiText_Old.text += uiText.text;
        uiText.text = "";
        uiText.text += "\n";
        uiText.text += "_____________________________";
        uiText.text += "\n";
        uiText.text += "\n";
    }

    public void AddToDescription(string str)
    {
        // replace keywords
        str = PhraseKey.ExtractItemWords(str);

        // majuscule
        str = TextUtils.FirstLetterCap(str);

        // add
        uiText.text += "\n" + str;

        AudioInteraction.Instance.StartSpeaking(str);

        CancelInvoke("AddToDescriptionDelay");
        Invoke("AddToDescriptionDelay", 0.001f);
    }

    void AddToDescriptionDelay()
    {
        uiText.text += "\n____________________\n";
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void Return()
    {
        uiText.text += "\n";
    }

}
