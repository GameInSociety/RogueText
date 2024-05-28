using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugDisplay : MonoBehaviour
{
    public static DebugDisplay Instance;
    public float decal = 30f;
    public int displayIndex = 0;

    public Transform parent;

    public enum CurrentCategory {
        WorldActions,
        AvailableItems,
    }
    public CurrentCategory category;

    public int previousSelected = -1;

    public GameObject group;
    public bool visible = false;


    public GameObject categoryParent;
    public Image[] categoryButtons;

    public List<DebugButton> buttons = new List<DebugButton>();
    public DebugButton prefab;
    int currentCount = 0;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        categoryButtons = categoryParent.GetComponentsInChildren<Image>();
        SetCategory(0);
    }

    private void Update() {
        displayIndex = 0;
        switch ((int)category) {
            case 0:
                return;
            case 1:
                UpdateWorldActions();
                break;
            case 2:
                UpdateAvailableItems();
                break;
                default : break;
        }

        for (int i = displayIndex; i < buttons.Count; i++) {
            buttons[i].gameObject.SetActive(false);
        }
    }

    public void SetCategory(int i) {

        foreach (var item in categoryButtons) {
            item.color = Color.white;
        }
        categoryButtons[i].color = Color.gray;

        if ( i == 0) {
            group.SetActive(false); 
            return;
        }

        group.SetActive(true);

        category = (CurrentCategory)i;
    }

    #region world actions
    public void UpdateWorldActions() {
        for (int i = 0; i < WorldAction.debug_list.Count; i++) {

            var parent_worldACtions = WorldAction.debug_list[i];
            if ( parent_worldACtions.origin) {
                DisplayWorldAction(parent_worldACtions);
            }
        }

        
    }
    private void DisplayWorldAction(WorldAction wa) {

        float offset = 0f;
        var origin_button = DisplayWorldAction(wa, offset);
        if (wa.debug_selected) {
            offset += decal;
            foreach (var child in wa.children) {
                var child_button = DisplayWorldAction(child, offset);
            }
        }
    }
    DebugButton DisplayWorldAction(WorldAction wa, float offset) {
        string text = wa.Name;
        Color c = Color.white;
        if (wa.IsTimeAction) {
            if (wa.interupted) {
                c = Color.red;
                text = wa.Name;
            } else {
                c = Color.magenta;
                text = wa.debug_count.ToString();
            }
        } else {
            if (wa.source == WorldAction.Source.Event)
                c = Color.cyan;
            else
                c = Color.yellow;
        }
        var button = DisplayNewButton(text, c, offset);
        button.SetWA(wa); ;

        if (wa.debug_selected) {
            offset += decal;
            foreach (var line in wa.lines) {

                // line btton
                var line_button = DisplayNewButton(line.content, Color.gray, offset);
                line_button.SetLine(line);

                if (line.debug_selected) {
                    Debug.Log($"line parts : {line.parts.Count}");
                    string p = "";
                    foreach (var part in line.parts) {
                        if (part.fail) {
                            p += $"<color=red> [ {part.text} ] ({part.debug_feedback}) </color>";
                        } else {
                            p += $"<color=green> [ {part.text} ] </color>";
                        }
                    }

                    // part button
                    var part_button = DisplayNewButton(p, Color.clear, offset + decal);
                    part_button.type = DebugButton.Type.Part;
                }
            }
        }

        return button;
    }
    #endregion

    #region available items
    void UpdateAvailableItems() {
        foreach (var cat in AvailableItems.categories) {
            float offset = 0f;
            DisplayNewButton(cat.name, Color.gray, 0f);
            foreach (var item in cat.items.FindAll(x=>!x.HasParent())) {
                DisplayItem(item, offset);
            }
        }
    }
    void DisplayItem(Item item, float offset) {
        DisplayNewButton(item.debug_name, Color.white, offset);

    }
    #endregion


    DebugButton DisplayNewButton(string text, Color c, float offset) {
        if (displayIndex >= buttons.Count)
            buttons.Add(Instantiate(prefab, parent));

        var button = buttons[displayIndex];
        button.gameObject.SetActive(true);
        button.Display(text, c);
        button.image.rectTransform.offsetMin = new Vector2(offset, 0f);
        ++displayIndex;
        return button;
    }
}
