using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class DebugDisplay : MonoBehaviour
{
    public static DebugDisplay Instance;
    public float decal = 30f;
    public int displayIndex = 0;

    public float wa_scale = 60f;
    public float line_scale = 40f;
    public float linePart_Scale = 25f;
    public float linePartContent_Scale = 15f;

    public Transform parent;

    public enum Category {
        None,
        WorldActions,
        AvailableItems,
        LineParts
    }
    public Category category;

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
            case 3:
                UpdateLineParts();
                break;
            case 4:
                UpdateItemDescriptions();
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

        if ( category == (Category)i) {
            category = Category.None;
            group.SetActive(false);
            return;
        }
        
        categoryButtons[i - 1].color = Color.gray;

        group.SetActive(true);

        category = (Category)i;
    }

    #region world actions
    public void UpdateWorldActions() {
        for (int i = 0; i < WorldAction.debug_list.Count; i++) {
            var parent_worldACtions = WorldAction.debug_list[i];
            DisplayWorldAction(parent_worldACtions);
            if (parent_worldACtions.origin) {
            }
        }

        
    }
    private void DisplayWorldAction(WorldAction wa) {

        float offset = 0f;
        var origin_button = DisplayWorldAction(wa, offset);
        if (origin_button.selected) {
            offset += decal;
            foreach (var child in wa.children) {
                var child_button = DisplayWorldAction(child, offset);
            }
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

        Color color = wa.source == WorldAction.Source.PlayerAction ? Color.yellow : wa.IsTimeAction ? Color.magenta : Color.cyan;
        string text = "";
        switch (wa.state) {
            case WorldAction.State.None:
            color = Color.Lerp(color, Color.clear, 0.5f);
                text = "None";
                break;
            case WorldAction.State.Done:
                text = "Done";
            color = Color.Lerp(color, Color.green, 0.5f);
                break;
            case WorldAction.State.Broken:
                text = $"Stopped : {wa.stop_feedback}";
            color = Color.Lerp(color, Color.gray, 0.5f);
                break;
            case WorldAction.State.Paused:
                text = "Paused";
            color = Color.Lerp(color, Color.blue, 0.5f);
                break;
            case WorldAction.State.Error:
                text = "Error";
            color = Color.Lerp(color, Color.red, 0.5f);
                break;
        }

        string label = wa.Name;
        string add_info = "";
        if (wa.IsTimeAction) {
            if (wa.state == WorldAction.State.Paused) {
                label = wa.Name;
                add_info = "Pause";
            } else {
                label = wa.debug_count.ToString();
                add_info = "Time";
            }
        }

        label += $"{label} {(wa.children.Count > 0 ? wa.children.Count.ToString() : "")}";
        var button = DisplayNewButton($"{label}||{text}", color, offset, wa_scale);

        if (button.selected) {
            offset += decal;
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
                var line_button = DisplayNewButton($"{line.content} || {lineText} ({line.parts.Count})", lineColor, offset, line_scale);
                if (line_button.selected) {
                    string p = "";
                    foreach (var part in line.parts) {
                        DisplayLinePart(part, offset + decal);
                    }
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
            var catButton = DisplayNewButton(cat.name, Color.magenta, 0f, wa_scale);
            if (!catButton.selected)
                continue;
            foreach (var item in cat.items.FindAll(x=>!x.HasParent())) {
                DisplayItem(item, offset + decal);
            }
        }
    }
    void DisplayItem(Item item, float offset) {
        var item_button = DisplayNewButton(item.DebugName, Color.yellow, offset, line_scale);
       if (item_button.selected) {
            foreach (var prop in item.props) {

                var prop_txt = $"<b>{prop.name}</b>";
                if (prop.HasPart("value"))
                    prop_txt += $"  (<color=red>{prop.GetPart("value").content}</color>)";

                var prop_button = DisplayNewButton(prop_txt, Color.cyan, offset + (decal*2), linePart_Scale);
                if ( prop_button.selected) {
                    foreach (var part in prop.parts) {
                        DisplayNewButton($"<i>{part.key}</i> <color=white>{part.content}</color>", Color.gray, offset+ (decal*3), linePartContent_Scale);
                    }
                }
            }

            if (!item.HasChildItems())
                return;
            foreach (var child in item.GetChildItems())
                DisplayItem(child, offset + decal);
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

            var logButton = DisplayNewButton("Search Log", Color.gray, offset + (decal * 2f), linePartContent_Scale);
            if (logButton.selected) {
                foreach (var log in linePart.linkLog) {
                    DisplayNewButton(log, Color.gray, offset + (decal * 2f), linePartContent_Scale);
                }
            }

            DisplayNewButton($"text:{linePart.output}", Color.white, offset + decal, linePartContent_Scale);
            if (linePart.item != null)
                DisplayNewButton($"Item : {linePart.item.DebugName}", Color.white, offset + decal, linePartContent_Scale);
            if (linePart.prop != null)
                DisplayNewButton($"Prop : {linePart.prop.name}", Color.white, offset+ decal, linePartContent_Scale);
            if (linePart.HasValue())
                DisplayNewButton($"Value : {linePart.value}", Color.white, offset + decal, linePartContent_Scale);
            foreach (var line in linePart.errors)
                DisplayNewButton(line, Color.red, offset + decal, linePartContent_Scale);

            if ( linePart.children.Count > 0) {
                foreach (var child in linePart.children) {
                    DisplayLinePart(child, offset + decal);
                }
            }

        }
    }
    #endregion

    #region item descriptions
    void UpdateItemDescriptions() {
        foreach (var dGroup in ItemDescription.archive) 
        {
            float offset = 0f;

            // SHOW GROUP
            var group_button = DisplayNewButton(dGroup.Name, Color.blue, offset, wa_scale);
            if (group_button.selected) {
                foreach (var id in dGroup.ids) {

                    // SHOW ITEM DESCRIPTION
                    if (id.name == "line break") {
                        DisplayNewButton(id.name, Color.gray, offset + (decal * 1), linePartContent_Scale);
                        continue;
                    }
                    var id_button = DisplayNewButton(id.name, Color.cyan, offset + (decal * 1), line_scale);
                    if (id_button.selected) {
                        foreach (var itGrp in id.groups) {
                            string s = $"[{itGrp.key}:{itGrp.itemSlots.Count}]";

                            // SHOW ITEM GROUP 
                            var itGrp_button = DisplayNewButton(s, Color.white, offset + (decal * 2), linePart_Scale);
                            if (itGrp_button.selected) {
                                foreach (var itemSlot in itGrp.itemSlots) {
                                    string it_s = $"[{itemSlot.key}] : {itemSlot.items.Count}";
                                    // ITEM SLOTS
                                    var it_button = DisplayNewButton(it_s, Color.yellow, offset + (decal * 3), linePartContent_Scale);
                                    string np_s = "";
                                    foreach (var prop in itemSlot.nestedProps)
                                        np_s += $"({prop.GetCurrentDescription()}) ";
                                    // NESTED PROPS
                                    var np_button = DisplayNewButton($"Nested : {np_s}", Color.magenta, offset + (decal * 3), linePartContent_Scale);
                                    string dp_s = "";
                                    foreach (var prop in itemSlot.describeProps)
                                        dp_s += $"({prop.GetCurrentDescription()}) ";
                                    // DESCRUBE PROPS
                                    var dp_button = DisplayNewButton($"Describe : {dp_s}", Color.magenta, offset + (decal * 3), linePartContent_Scale);

                                }
                            }
                        }
                    }
                }
            }

        }
    }
    #endregion
    DebugButton DisplayNewButton(string text, Color c, float offset, float height) {

        string sec = "";
        if (text.Contains("||")) {
            var split = text.Split("||");
            text = split[0];
            sec = split[1];
        }

        if (displayIndex >= buttons.Count)
            buttons.Add(Instantiate(prefab, parent));

        var button = buttons[displayIndex];
        button.gameObject.SetActive(true);
        button.Display(text, c, sec);
        button.image.rectTransform.offsetMin = new Vector2(offset, button.image.rectTransform.offsetMin.y);
        button.rectTransform.sizeDelta = new Vector2(button.rectTransform.sizeDelta.x, height);
        ++displayIndex;
        return button;
    }
}
