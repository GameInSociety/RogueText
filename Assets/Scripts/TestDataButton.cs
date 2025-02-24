using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TestDataButton : MonoBehaviour, IPointerClickHandler {
    public enum Type {
        ItemLoader,
        Map,
        GlobalProperties,
        AnyItem,
        NoItem,
    }

    public string text = "";
    public Type type;
    public KeyCode keyCode;
    CanvasGroup canvasGroup;

    private void Start() {
        canvasGroup = GetComponent<CanvasGroup>();
        var load = PlayerPrefs.GetString(type.ToString(), "");
        if (type != Type.ItemLoader && load == "") {
            canvasGroup.alpha = 0f;
        }
    }

    private void Update() {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(keyCode)) {
            PlayerPrefs.SetString(type.ToString(), "active");
            canvasGroup.alpha = 1f;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        StartCoroutine(LoadDatasInGameCoroutine());
    }

    IEnumerator LoadDatasInGameCoroutine() {
        TextManager.Write(text, Color.yellow);
        yield return new WaitForSeconds(2f);

        switch (type) {
            case Type.ItemLoader:
                yield return ItemLoader.Instance.StartCoroutine(ItemLoader.Instance.DownloadsCSV("Test Area"));
                break;
            case Type.Map:
                yield return MapLoader.Instance.StartCoroutine(MapLoader.Instance.DownloadsCSVs());
                break;
            case Type.GlobalProperties:
                yield return ContentLoader.Instance.StartCoroutine(ContentLoader.Instance.DownloadsCSVs());
                break;
            case Type.AnyItem:
                yield return ItemLoader.Instance.StartCoroutine(ItemLoader.Instance.DownloadsCSV("Any Item"));
                break;
            case Type.NoItem:
                yield return ItemLoader.Instance.StartCoroutine(ItemLoader.Instance.DownloadsCSV("No Item"));
                break;
            default:
                break;
        }
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene(0);
    }

}
