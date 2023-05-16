using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDescription : MonoBehaviour {
	
	public static DisplayDescription Instance;
    public RectTransform verticalLayoutGroup;

    public ScrollRect scrollRect; 

    public Text uiText;
    public Text uiText_Old;

    public Cardinal debug_cardinal;

    /// <summary>
    /// TYPING EFFECTS
    /// OPTIONAL
    /// </summary>
    public int letterRate = 1;

    public string text_current;
    public string text_target;
    int typeIndex = 0;
    float timer = 0f;
    public float rate = 0.2f;

    bool playVoice = false;
    float voiceTimer;
    public float voiceDelay = 0.1f;
    string voice_Text = "";

	void Awake () {
		Instance = this;
	}

    void Start()
    {
        ClearDescription();

        PlayerActionManager.onPlayerAction += HandleOnAction;
    }

    bool taint = false;

    private void Update()
    {
        if ( playVoice )
        {
            voiceTimer += Time.deltaTime;
            if ( voiceTimer >= voiceDelay)
            {
                AudioInteraction.Instance.StartSpeaking(voice_Text);
                voice_Text = "";
                playVoice = false;
            }
        }

        if (timer >= rate)
        {
            timer = 0f;

            Type();
        }

        timer += Time.deltaTime;
    }

    void Type()
    {

        if (typeIndex >= text_target.Length)
        {
            return;
        }

        if (uiText.text.EndsWith("■"))
        {
            uiText.text = uiText.text.Remove(uiText.text.Length - 1);
        }

        Sound.Instance.PlayRandomTypeSound();

        if (taint)
        {
            if (text_target[typeIndex] == '<')
            {
                taint = false;
                typeIndex = uiText.text.Length;
                return;
            }

        }
        else
        {
            if (text_target[typeIndex] == '<')
            {
                taint = true;
                uiText.text += "<color=green>";
                typeIndex = uiText.text.Length;
                uiText.text += "</color>";
                return;
            }

        }

        uiText.text = uiText.text.Insert(typeIndex, text_target[typeIndex].ToString()) + "■";

        //

        ++typeIndex;
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
        text_target = "";
        uiText_Old.text = "";
    }

    public void UpdateDescription()
    {

        // je vide les sockets ici, mais à terme, il faut que les sockets restent dans la tile
        // à regarder "no item list" dans la feuille de route
        SocketManager.Instance.socketGroups.Clear();

        Tile.GetCurrent.Describe();

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
        typeIndex = 0;
        uiText_Old.text += uiText.text;
        text_target = "";

        uiText.text = "";


        /*uiText.text += "\n";
        uiText.text += "_____________________________";
        uiText.text += "\n";
        uiText.text += "\n";*/
    }

    public void AddToDescription(string str, bool doReturn)
    {
        // majuscule
        str = TextUtils.FirstLetterCap(str);

        // add
        if (doReturn)
        {
            text_target += "\n" + str;
        }
        else
        {
            text_target += str;
        }

        voice_Text += str;

        playVoice = true;
        voiceTimer = 0f;

        //uiText.text += "\n____________________\n";
        scrollRect.verticalNormalizedPosition = 0f;
    }

}
