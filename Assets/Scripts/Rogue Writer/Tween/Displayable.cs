using UnityEngine;

using DG.Tweening;

public class Displayable : MonoBehaviour
{
    public enum State {
        Visible,
        Hidden,
        None,
    }
    public State state;
    public float fade_duration = 0.3f; 
    private  CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Transform _transform;
    [SerializeField] private GameObject group;

    public virtual void Start() {
        switch (state) {
            case State.Visible:
                Show();
                break;
            case State.Hidden:
                Hide();
                break;
            case State.None:

                break;
            default:
                break;
        }
    }


    public CanvasGroup CanvasGroup{
        get{
            if (_canvasGroup == null){
                _canvasGroup = GetComponent<CanvasGroup>();
                if ( _canvasGroup == null )
                    _canvasGroup = gameObject.AddComponent(typeof(CanvasGroup)) as CanvasGroup;
            }

            return _canvasGroup;
        }
    }

    public virtual void Show(){
        Group.SetActive(true);
        CanvasGroup.alpha = 1f;
        state = State.Visible;
    }

    public virtual void Hide(){
        Group.SetActive(false);
        state = State.Hidden;
    }

    public void FadeOut()
    {
        CanvasGroup.DOFade(0f, fade_duration);

        CancelInvoke("Hide");
        Invoke("Hide", fade_duration);
    }

    public void FadeIn(bool delay = false) {
        CancelInvoke("Hide");
        CanvasGroup.DOKill();
        Show();
        CanvasGroup.alpha = 0f;

        if (delay) {
            CancelInvoke("FadeInDelay");
            Invoke("FadeInDelay", fade_duration);
        } else {
            FadeInDelay();
        }
    }

    void FadeInDelay() {
        CanvasGroup.DOFade(1f, fade_duration);
    }

    public GameObject Group {
        get {
            if (group == null) {
                group = gameObject;
            }

            return group;
        }
    }

    public Transform GetTransform {
        get {
            if (_transform==null)
                _transform = transform;
            return _transform;
        }
    }

    public RectTransform GetRectTransform {
        get {
            if (_rectTransform == null) {
                _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform;
        }
    }
}
