using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class DebugDisplay : MonoBehaviour
{
    public enum Category {
        None,
        WorldActions,
        AvailableItems,
        LineParts
    }

    public static DebugDisplay Instance;

    public Transform parent;
    
    // params
    public float wa_scale = 60f;
    public float line_scale = 40f;
    public float linePart_Scale = 25f;
    public float linePartContent_Scale = 15f;

    // category
    public Category category;
    public GameObject categoryParent;
    public Image[] categoryButtons;
    // buttons
    public float buttons_Decal = 30f;
    public int buttons_DisplayIndex = 0;
    public List<DebugButton> buttons = new List<DebugButton>();
    public DebugButton debugButton_Prefab;

    public GameObject show_group;

    public int wa_fontSize = 13;
    public int line_fontSize = 10;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        categoryButtons = categoryParent.GetComponentsInChildren<Image>();
        SetCategory(0);
    }

    private void Update() {
        buttons_DisplayIndex = 0;
        switch ((int)category) {
            case 0:
                return;
            case 1:
                UpdateWorldActions();
                break;
            case 2:
                UpdateAvailableItems();
                break;
            case 3:
                UpdateLineParts();
                break;
            case 4:
                UpdateItemDescriptions();
                break;
            default : break;
        }

        for (int i = buttons_DisplayIndex; i < buttons.Count; i++) {
            buttons[i].gameObject.SetActive(false);
        }

    }

    public void SetCategory(int i) {

        foreach (var item in categoryButtons) {
            item.color = Color.white;
        }

        if ( category == (Category)i) {
            category = Category.None;
            show_group.SetActive(false);
            return;
        }
        
        categoryButtons[i - 1].color = Color.gray;

        show_group.SetActive(true);

        category = (Category)i;
    }

    #region world actions
    float wa_offset = 0f;
    public void UpdateWorldActions() {
        for (int i = 0; i < WorldAction.debug_list.Count; i++) {
            var wa = WorldAction.debug_list[i];
            wa_offset = 0f;
            var origin_button = DisplayWorldAction(wa, wa_offset);
           
        }

        
    }

    // time = magenta
    // event = cyan
    // player input = yellow
    // good : yellow / green
    // error : yellow / red
    // skipped : green / gray ?
    // paused : yellow / blue

    DebugButton DisplayWorldAction(WorldAction wa, float offset) {

        Color color = wa.source == WorldAction.Source.PlayerAction ? Color.yellow : wa.IsTimeAction ? Color.magenta : Color.white;
        string sideLabel = "";
        switch (wa.state) {
            case WorldAction.State.None:
            color = Color.Lerp(color, Color.clear, 0.5f);
                sideLabel = "None";
                break;
            case WorldAction.State.Done:
                sideLabel = "Done";
            color = Color.Lerp(color, Color.green, 0.5f);
                break;
            case WorldAction.State.Broken:
                sideLabel = $"Stopped : {wa.stop_feedback}";
            color = Color.Lerp(color, Color.gray, 0.5f);
                break;
            case WorldAction.State.Paused:
                sideLabel = "Paused";
            color = Color.Lerp(color, Color.blue, 0.5f);
                break;
            case WorldAction.State.Error:
                sideLabel = "Error";
            color = Color.Lerp(color, Color.red, 0.5f);
                break;
        }

        string mainLabel = wa.Name;
        if (wa.IsTimeAction) {
            if (wa.state == WorldAction.State.Paused) {
                mainLabel = wa.Name;
            } else {
                mainLabel = wa.debug_count.ToString();
            }
        }

        sideLabel += wa.children.Count > 0 ? $" (+{wa.children.Count})" : "";

        var wa_button = DisplayNewButton($"{mainLabel}||{sideLabel}", color, offset, wa_scale,wa_fontSize,  Color.yellow);

        if (wa_button.selected) {
            offset += buttons_Decal;
            foreach (var line in wa.lines) {
                Color lineColor = Color.white;
                string lineText = "";
                switch (line.state) {
                    case Line.State.None:
                        lineText = "None";
                        lineColor = Color.Lerp(lineColor, Color.clear, 0.5f);
                        break;
                    case Line.State.Skipped:
                        lineText = "Skipped";
                        lineColor = Color.Lerp(lineColor, Color.grey, 0.5f);
                        break;
                    case Line.State.Broken:
                        lineText = "Stopped";
                        lineColor = Color.Lerp(lineColor, Color.yellow, 0.5f);
                        break;
                    case Line.State.Error:
                        lineText = "Error";
                        lineColor = Color.Lerp(lineColor, Color.red, 0.5f);
                        break;
                    case Line.State.Done:
                        lineText = "Done";
                        lineColor = Color.Lerp(lineColor, Color.green, 0.5f);
                        break;
                    default:
                        break;
                }
                var line_button = DisplayNewButton($"{line.content}\n{line.debug_text} || {lineText}", lineColor, offset, line_scale, line_fontSize, Color.blue);
                if (line_button.selected) {
                    string p = "";
                    for (int i = 0; i < line.parts.Count; i++) {
                        DisplayLinePart(line.parts[i], offset + buttons_Decal);
                    }
                }
            }

            if ( wa.children.Count > 0 ) {
                var children_button = DisplayNewButton($"Show ${wa.children.Count} Children", Color.gray, offset, line_scale, line_fontSize);
                if (children_button.selected) {
                    foreach (var child in wa.children) {
                        var child_button = DisplayWorldAction(child, offset);
                    }
                }
            }
            
        }



        return wa_button;
    }
    #endregion

    #region available items
    void UpdateAvailableItems() {
        foreach (var cat in AvailableItems.categories) {
            float offset = 0f;
            var catButton = DisplayNewButton(cat.name, Color.magenta, 0f, wa_scale, wa_fontSize, Color.yellow);
            if (!catButton.selected)
                continue;
            foreach (var item in cat.items.FindAll(x=>!x.HasParent())) {
                DisplayItem(item, offset + buttons_Decal);
            }
        }
    }
    void DisplayItem(Item item, float offset) {
        var item_button = DisplayNewButton(item.DebugName, Color.yellow, offset, line_scale, line_fontSize, Color.blue);
       if (item_button.selected) {
            foreach (var prop in item.props) {

                var prop_txt = $"<b>{prop.name}</b>";
                if (prop.HasPart("value"))
                    prop_txt += $"  (<color=red>{prop.GetPart("value").content}</color>)";

                var prop_button = DisplayNewButton(prop_txt, Color.cyan, offset + (buttons_Decal*2), linePart_Scale, line_fontSize, Color.magenta);
                if ( prop_button.selected) {
                    foreach (var part in prop.parts) {
                        DisplayNewButton($"<i>{part.key}</i> <color=white>{part.content}</color>", Color.gray, offset+ (buttons_Decal*3), linePartContent_Scale);
                    }
                }
            }

            if (!item.HasChildItems())
                return;
            foreach (var child in item.GetChildItems())
                DisplayItem(child, offset + buttons_Decal);
        }
    }
    #endregion

    #region line parts
    void UpdateLineParts() {
        foreach (var linePart in LinePart.debug_lineParts) {
            float offset = 0f;
            DisplayLinePart(linePart, offset);
        }
    }

    void DisplayLinePart(LinePart linePart, float offset) {

        Color baseColor = Color.black;
        string feedback = "";
        switch (linePart.state) {
            case LinePart.State.None:
                baseColor = Color.Lerp(baseColor, Color.clear, 0.5f);
                feedback = "None";
                break;
            case LinePart.State.Done:
                baseColor = Color.Lerp(baseColor, Color.green, 0.5f);
                feedback = "Sucess";
                break;
            case LinePart.State.Failed:
                baseColor = Color.Lerp(baseColor, Color.yellow, 0.5f);
                feedback = "Failed";
                break;
            case LinePart.State.Error:
                feedback = "Error";
                baseColor = Color.Lerp(baseColor, Color.red, 0.5f);
                break;
            default:
                break;
        }

        string str = $"<i>{linePart.input}</i> => <b>{linePart.output}</b>";
        string sec = $"{ feedback +  (linePart.children.Count > 0 ? $"(+{linePart.children.Count})" : "")}{linePart.label}";

        var partButton = DisplayNewButton($"<color=white>{str}</color>||<color=white>{sec}</color>", baseColor, offset, linePart_Scale);

        if (partButton.selected) {
            var logButton = DisplayNewButton("Search Log", Color.gray, offset + (buttons_Decal * 2f), linePartContent_Scale);
            if (logButton.selected) {
                foreach (var log in linePart.linkLog) {
                    DisplayNewButton(log, Color.gray, offset + (buttons_Decal * 2f), linePartContent_Scale);
                }
            }

            /*DisplayNewButton($"text:{linePart.output}", Color.white, offset + decal, linePartContent_Scale);
            if (linePart.item != null)
                DisplayNewButton($"Item : {linePart.item.DebugName}", Color.white, offset + decal, linePartContent_Scale);
            if (linePart.prop != null)
                DisplayNewButton($"Prop : {linePart.prop.name}", Color.white, offset+ decal, linePartContent_Scale);
            if (linePart.HasValue())
                DisplayNewButton($"Value : {linePart.value}", Color.white, offset + decal, linePartContent_Scale);
            foreach (var line in linePart.errors)
                DisplayNewButton(line, Color.red, offset + decal, linePartContent_Scale);*/

            if ( linePart.children.Count > 0) {
                foreach (var child in linePart.children) {
                    DisplayLinePart(child, offset + buttons_Decal);
                }
            }

        }
    }
    #endregion

    #region item descriptions
    void UpdateItemDescriptions() {
        foreach (var dGroup in DescriptionManager.Instance.archive) 
        {
            float offset = 0f;

            // SHOW GROUP
            var dGroup_Button = DisplayNewButton(dGroup.id, Color.blue, offset, wa_scale);
            if (dGroup_Button.selected) {
                Debug.Log($"Count : {dGroup.slots.Count}");
                foreach (var slot in dGroup.slots) {
                    string it_s = $"[{slot.key}] : {slot.items.Count}";
                    // ITEM SLOTS
                    var it_button = DisplayNewButton(it_s, Color.yellow, offset + (buttons_Decal), line_scale);
                    if ( it_button.selected ) {
                        foreach (var prop in slot.props)
                            DisplayNewButton($"{prop.name} ({prop.GetCurrentDescription()})", Color.magenta, offset + (buttons_Decal * 2), linePartContent_Scale);
                    }
                }
            }

        }
    }
    #endregion
    DebugButton DisplayNewButton(string text, Color c, float offset, float height, int fontSize = 10, Color outlineColor = new Color()) {
        string sec = "";
        if (text.Contains("||")) {
            var split = text.Split("||");
            text = split[0];
            sec = split[1];
        }

        if (buttons_DisplayIndex >= buttons.Count)
            buttons.Add(Instantiate(debugButton_Prefab, parent));


        var button = buttons[buttons_DisplayIndex];
        button.transform.SetParent(parent);
        button.gameObject.SetActive(true);
        button.Display(text, c, sec, fontSize, outlineColor);
        button.image.rectTransform.offsetMin = new Vector2(offset, button.image.rectTransform.offsetMin.y);
        button.rectTransform.sizeDelta = new Vector2(button.rectTransform.sizeDelta.x, height);
        ++buttons_DisplayIndex;
        return button;
    }

}
