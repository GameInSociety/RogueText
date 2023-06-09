using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDescription : MonoBehaviour {
	
	public static DisplayDescription Instance;
    public RectTransform verticalLayoutGroup;

    public ScrollRect scrollRect;

    public bool enableAI = false;
    public bool useAIForNextText = false;

    public bool AI_Enabled()
    {
        return DebugManager.Instance.AI_Enabled;
    }

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

        if (AI_Enabled())
        {
            ChatGPT.Instance.onSendReply += HandleOnSendReply;
        }
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

    public void ClearDescription()
    {
        uiText.text = "";
        text_target = "";
        uiText_Old.text = "";
    }

    public void Reset()
    {
        typeIndex = 0;
        text_target = "";
        uiText.text = "";
    }

    public void Renew()
    {
        uiText_Old.text += uiText.text;

        Reset();


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
            str = "\n" + str;
        }

        text_target += str;

        //uiText.text += "\n____________________\n";
        scrollRect.verticalNormalizedPosition = 0f;

        CancelInvoke("Delay");
        Invoke("Delay", 0f);
    }

    public void UseAI()
    {
        if (AI_Enabled())
        {
            useAIForNextText = true;
        }
    }

    public void Delay()
    {
        if (useAIForNextText )
        {
            useAIForNextText = false;

            DallE.Instance.SendImageRequest(text_target);

            ChatGPT.Instance.SendReply(text_target);
        }
        else
        {
            UpdateDescription(text_target);
        }
    }

    void HandleOnSendReply(string text)
    {
        UpdateDescription(text);   
    }

    public void UpdateDescription(string text)
    {
        text_target = text;
        voice_Text = text;
        playVoice = true;
        voiceTimer = 0f;
    }



}
