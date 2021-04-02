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
    public CanvasGroup fade_Top_CanvasGroup;
    public CanvasGroup fade_Bottom_CanvasGroup;
    public float fade_Buffer = 0.1f;
    public float fade_Speed = 1f;

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
    }

    private void Update()
    {
        UpdateScrollFade();
    }

    private void UpdateScrollFade()
    {
        /*if (scrollRect.verticalNormalizedPosition >= 1f - fade_Buffer)
        {
            fade_Top_CanvasGroup.alpha = Mathf.Lerp(fade_Top_CanvasGroup.alpha, 0f, fade_Speed * Time.deltaTime);
        }
        else
        {
            fade_Top_CanvasGroup.alpha = Mathf.Lerp(fade_Top_CanvasGroup.alpha, 1f, fade_Speed * Time.deltaTime);
        }

        if (scrollRect.verticalNormalizedPosition <= fade_Buffer)
        {
            fade_Bottom_CanvasGroup.alpha = Mathf.Lerp(fade_Bottom_CanvasGroup.alpha, 0f, fade_Speed * Time.deltaTime);
        }
        else
        {
            fade_Bottom_CanvasGroup.alpha = Mathf.Lerp(fade_Bottom_CanvasGroup.alpha, 1f, fade_Speed * Time.deltaTime);
        }

        test = scrollRect.verticalNormalizedPosition;*/
    }

    public void UpdateDescription()
    {
        StartCoroutine(UpdateDescriptionCoroutine());
    }

    IEnumerator UpdateDescriptionCoroutine()
    {
        DisplayInput.Instance.Hide();

        Transition.Instance.FadeIn();

        yield return new WaitForSeconds(Transition.Instance.duration);

        Transition.Instance.FadeOut();

        uiText.text = newText;

        DisplayInput.Instance.Show();

    }

    public void ClearDescription()
    {
        uiText.text = "";
    }

    public void AddToDescription(string str)
    {
        // archive
        uiText_Old.text += uiText.text;

        // clear
        uiText.text = "";

        // add
        uiText.text += "\n";
        uiText.text += "----------------";
        uiText.text += "\n";
        uiText.text += str;

        Invoke("AddToDescriptionDelay", 0.001f);
    }

    void AddToDescriptionDelay()
    {
        scrollRect.verticalNormalizedPosition = 0f;
    }

}
