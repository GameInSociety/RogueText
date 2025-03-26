 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineManager : MonoBehaviour
{
    public static EngineManager Instance;

    [System.Serializable]
    public class DisplayVisual {
        public string name;
        public Slot.Type type;
        public Color color;
        public Color color_Dark;
        public Color color_Light;
        public void Init() {
            color_Dark = Color.Lerp(Color.black, color, 0.5f);
            color_Light = Color.Lerp(Color.white, color, 0.5f);
        }

    }
    public DisplayVisual[] displayVisuals;

    public Canvas main_Canvas;

    // UI
    [SerializeField] private RW_DisplayCategory m_DisplayCategory;

    public RW_DisplayCategory DisplayCategory {
        get => m_DisplayCategory;
    }

    private void Awake() {
        Instance = this;
    }

    private void Start() {

        ContentLoader.Instance.Load();
        ItemLoader.Instance.Load();

        DisplayCategory.Display(ItemManager.Instance.InitCategory);

        InitVisuals();
    }

    void InitVisuals() {
        foreach (var item in displayVisuals) {
            item.Init();
        }
    }

    public DisplayVisual GetVisual(string name) {
        return System.Array.Find(displayVisuals, x=> x.name == name);
    }
    public DisplayVisual GetVisual(Slot.Type type) {
        var dv = System.Array.Find(displayVisuals, x => x.type == type);
        if (dv == null) {
            Debug.LogError($"no display visual type : {type}");
            return displayVisuals[0];
        }
        return dv;
    }
}
