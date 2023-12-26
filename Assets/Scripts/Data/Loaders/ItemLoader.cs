using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class ItemLoader : DataDownloader {

    // singleton
    public static ItemLoader Instance;
    int currIndex = 0;
    public List<DebugGroup> debugGroups = new List<DebugGroup>();

    [System.Serializable]
    public class DebugGroup {
        public string name;
        public List<ItemData> debug_items = new List<ItemData>();
    }

    private void Awake() {
        Instance = this;
    }

    public override void FinishLoading() {
        base.FinishLoading();
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        if (rowIndex == 0)
            return;
        // skip empty
        if (cells.Count <= 1) {
            return;
        }

        if (string.IsNullOrEmpty(cells[0])) {
            LoadProperties(ItemData.itemDatas[currIndex-1], cells);
            return;
        }

        // Create new item
        var newItemData = new ItemData();
        var nameCell = cells[0];
        var synonyms = nameCell.Split('\n');

        // HasPart numberSpecific class name
        if (!string.IsNullOrEmpty(cells[1]))
            newItemData.className = cells[1];

        newItemData.debugName = synonyms[0];

        // SYNONYMS HERE
        // new word
        for (var i = 0; i < synonyms.Length; i++) {
            var newWord = new Word();
            newWord.SetText(synonyms[i]);
            newItemData.words.Add(newWord);
            newWord.UpdateNumber(cells[2]);
        }

        if (cells[3].Length > 1)
            newItemData.words[0].preposition = cells[3];

        // TYPES
        if (!string.IsNullOrEmpty(cells[4])) {
            string[] types = cells[4].Split('\n');
            var prop = new Property();
            prop.name = "types";
            for (int i = 0; i < types.Length; i++) {
                if (string.IsNullOrEmpty(types[i])) { continue; }
                prop.AddPart(types[i], "");
            }
            newItemData.properties.Add(prop);
        }
        //

        //newItem.index = rowIndex-1;
        newItemData.index = currIndex;
        cells.RemoveRange(0, 5);
        if ( cells.Count > 0) {
            // add to item list
            LoadProperties(newItemData, cells);
        }

        var debugGroup = debugGroups.Find(x => x.name == sheetName);
        if (debugGroup == null) {
            debugGroup = new DebugGroup();
            debugGroup.name = sheetName;
            debugGroups.Add(debugGroup);
        }
        debugGroup.debug_items.Add(newItemData);
        ItemData.itemDatas.Add(newItemData);
        ++currIndex;
    }

    void LoadProperties(ItemData data, List<string> contents) {
        for (var i = 0; i < contents.Count; i++) {
            if (string.IsNullOrEmpty(contents[i]))
                continue;

            string content = contents[i];

            // load groups
            if (content.StartsWith('%')) {
                var groupName = content.Substring(2);
                int exit = groupName.IndexOf('\n');
                if (exit>=0)
                    groupName = groupName.Remove(exit);
                ContentLoader.Group group = ContentLoader.GetGroup(groupName);
                if (group == null) {
                    Debug.LogError($"no group named {content}");
                    continue;
                }

                LoadProperties(data, group.contents);

                // OVERRIDE the properties of the group
                var lines = content.Split('\n');
                for (int j = 1; j < lines.Length; j++) {
                    var split = lines[j].Split(':');
                    var nestedProp = data.properties.Find(x => x.name == split[0]);
                    if ( nestedProp == null) {
                        Debug.LogError($"GROUP PARSING : no property named {split[0]} in group {groupName}");
                        continue;
                    }
                    nestedProp.AddPart("value", split[1]);
                }
                continue;
            }

            // load sequences
            if (content.StartsWith('$') || content.StartsWith('!')) {
                var cellParts = contents[i].Split(new char[1] { '\n' }, 2);
                var triggers = cellParts[0].Remove(0, 2);
                var seq = new Sequence(triggers, cellParts[1]);
                if (content.StartsWith('$'))
                    data.verbSequences.Add(seq);
                if (content.StartsWith('!'))
                    data.events.Add(seq);
                continue;
            }

            var cellLines = content.Split('\n');
            // add prop
            if (cellLines[0].Contains(':')) {
                // one liners
                for (int a = 0; a < cellLines.Length; a++) {
                    var prop = new Property();
                    var split = cellLines[a].Split(':');
                    prop.name = split[0];
                    prop.AddPart("value", split[1]);
                    data.properties.Add(prop);
                }
            } else {
                var prop = new Property();
                prop.Parse(cellLines);
                data.properties.Add(prop);
            }


        }
    }

    void ThrowError(string message) {
        Debug.Log($"{message} at row {row} and sheet {sheetName}");
    }
}