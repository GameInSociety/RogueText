using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Debug_WorldActions : MonoBehaviour
{
    public static Debug_WorldActions Instance;
    public int displayIndex = 0;

    public Transform parent;

    private void Awake() {
        Instance = this;
    }

    public List<DebugButton> buttons = new List<DebugButton>();
    public DebugButton prefab;
    int currentCount = 0;

    private void Update() {
        UpdateDisplay();

    }
    int safe = 0;
    public void UpdateDisplay() {
        displayIndex = 0;
        safe = 0;
        int seconds = 0;
        for (int i = 0; i < WorldAction.debug_list.Count; i++) {
            if ( safe == 500) {
                Debug.LogError($"safe out");
                break;
            }
            ++safe;

            var worldAction = WorldAction.debug_list[i];
            if (worldAction.skip) {
                ++seconds;
                if (WorldAction.debug_list[i - 1].skip) {
                    --displayIndex;
                }
                DisplayNewButton($"{seconds}", Color.magenta);

            } else {
                seconds = 0;
                DisplayWorldAction(worldAction);
            }
        }
    }
    int secondsGroupCount;
    private void DisplayWorldAction(WorldAction worldAction) {


        Color c = worldAction.lines.Find(x => x.failed) != null ? Color.red : worldAction.timeAction ? Color.cyan : worldAction.source == WorldAction.Source.PlayerAction ? Color.yellow: Color.white;
        var wa_button = DisplayNewButton(worldAction.Name, worldAction.skip ? Color.gray : c);

            worldAction.debug_selected = wa_button.selected;
        if (wa_button.selected) {
            if (! worldAction.debug_selected ) {  
                foreach (var item in worldAction.lines) {
                    item.debug_selected = false;
                }
            }
        }
        wa_button.UpdateUI(worldAction.debug_selected);


        if (worldAction.debug_selected) {
            foreach (var line in worldAction.lines) {

                var line_button = DisplayNewButton(line.content, Color.clear);
                if (line_button.isPressed) {
                    line.debug_selected = ! line.debug_selected;
                }
                line_button.UpdateUI(line.debug_selected);

                if ( line.debug_selected) {
                    foreach (var part in line.parts) {
                        string p = $"<color=cyan>{part.text}</color> [ {part.debug_feedback} ]";
                        DisplayNewButton(p, Color.clear);
                    }
                }
            }
        }

    }

    DebugButton DisplayNewButton(string text, Color c) {
        if (displayIndex >= buttons.Count)
            buttons.Add(Instantiate(prefab, parent));

        var button = buttons[displayIndex];
        button.gameObject.SetActive(true);
        button.Display(text, c);
        ++displayIndex;
        return button;
    }
}
