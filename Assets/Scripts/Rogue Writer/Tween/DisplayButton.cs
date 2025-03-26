using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayButton : Displayable, IPointerClickHandler
{   
    public Displayable showTarget;
    public Displayable hideTarget;

    public bool locked = false;

    public bool lockOnEnable = false;

    void OnEnable()
    {
        if (lockOnEnable)
        {
            Lock();
        }
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (locked)
        {
            return;
        }

        Trigger();   
    }
    
    public void Trigger()
    {
        Tween.Bounce(GetTransform);

        showTarget?.FadeIn();
        hideTarget?.FadeOut();
    }

    public void Lock()
    {
        CanvasGroup.alpha = 0.5f;
        locked = true;
    }

    public void Unlock()
    {
        CanvasGroup.alpha = 1f;

        locked = false;
    }
}
