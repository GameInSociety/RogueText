using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayText : MonoBehaviour
{
    public GameObject group;
    public Text uiText;

    public virtual void Start()
    {

    }

    public virtual void Display(string str)
    {
        Show();
        uiText.text = str;
    }

    public void Show()
    {
        group.SetActive(true);
    }

    public void Hide()
    {
        group.SetActive(false);
    }
}
