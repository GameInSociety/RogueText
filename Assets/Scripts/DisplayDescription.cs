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

    public Cardinal debug_cardinal;

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

        // je vide les sockets ici, mais à terme, il faut que les sockets restent dans la tile
        // à regarder "no item list" dans la feuille de route
        SocketManager.Instance.socketGroups.Clear();

        Tile.GetCurrent.Describe();

        // SURROUNDING TILES
        

        // display tile items
        
        // pas sûr que les choses d'état de santé, de temps et autre trucs divers doivent être là, pense à changer
        ConditionManager.GetInstance().WriteDescription();

        // time of day
        TimeManager.GetInstance().WriteDescription();

        // weather
        TimeManager.GetInstance().WriteWeatherDescription();
    }

    public void Renew()
    {
        uiText_Old.text += uiText.text;
        uiText.text = "";
        uiText.text += "\n";
        /*uiText.text += "_____________________________";
        uiText.text += "\n";
        uiText.text += "\n";*/
    }

    public void AddToDescription(string str)
    {

        // majuscule
        str = TextUtils.FirstLetterCap(str);

        // add
        uiText.text += "\n" + str;

        AudioInteraction.Instance.StartSpeaking(str);

        //uiText.text += "\n____________________\n";
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void Return()
    {
        uiText.text += "\n";
    }

}
