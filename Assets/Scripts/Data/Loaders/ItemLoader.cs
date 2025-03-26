using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Security.Cryptography;
using TMPro.EditorUtilities;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ItemLoader : DataDownloader {

    // singleton
    public static ItemLoader Instance;
    int currIndex = 0;
    private string currSheetName = "";

    private void Awake() {
        Instance = this;
    }

    public override void Load() {
        ItemData.itemDatas.Clear();
        base.Load();
    }

    public override void FinishLoading() {
        base.FinishLoading();
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        if (rowIndex == 0)
            return;
        // skip empty
        if (cells.Count <= 1)
            return;

        if (string.IsNullOrEmpty(cells[0])) {
            LoadProperties(ItemData.itemDatas[currIndex-1], cells);
            return;
        }

        // Create new item
        var newItemData = new ItemData();
        var synonyms = cells[0].Split('\n');
        newItemData.debugName = synonyms[0];

        for (var i = 0; i < synonyms.Length; i++) {
            var newWord = new Word();

            if (synonyms[i].StartsWith('[')) {
                var condition = TextUtils.Extract('[', synonyms[i], out synonyms[i]);
                var prop = new Property();
                prop.name = condition;
                prop.AddPart("description", $"{synonyms[i]}");
                prop.AddPart("condition", $"{condition}");
                newItemData.properties.Add(prop);
            }

            var word_txt = synonyms[i];
            if (word_txt.EndsWith('s'))
                newWord.defaultNumber = Word.Number.Plural;
            if (word_txt.EndsWith("(s)")) {
                newWord.defaultNumber = Word.Number.Plural;
                word_txt = word_txt.Remove(word_txt.IndexOf('('));
            }

            newWord.SetText(word_txt);
            newItemData.words.Add(newWord);
        }

        // ClassName
        if (!string.IsNullOrEmpty(cells[1]))
            newItemData.className = cells[1];


        // preposition
        newItemData.words[0].preposition = cells[3].Length > 1 ? cells[3] : "in";

        if (newItemData.name == "common") {
        } else {
            newItemData.roots.Add(ItemData.GetItemData("common"));
        }

        //newItem.index = rowIndex-1;
        newItemData.id = currIndex;
        cells.RemoveRange(0, 5);
        if ( cells.Count > 0) {
            // add to item list
            LoadProperties(newItemData, cells);
        }

        ItemData.itemDatas.Add(newItemData);

        // RW //
        if (sheetName != currSheetName) {
            currSheetName = sheetName;
            ItemManager.Instance.AddCategory(sheetName);
        }
        ItemManager.Instance.InitCategory.ItemCategories.Last().AddItemData(newItemData);

        ++currIndex;
    }

    void LoadProperties(ItemData data, List<string> cells) {
        for (var i = 0; i < cells.Count; i++) {
            if (string.IsNullOrEmpty(cells[i]))
                continue;

            string content = cells[i];

            // load groups (parnets )
            if (content.StartsWith('%')) {
                // loading all properties of a group
                // extracting the first line ( in the case there is replacements )
                var groupName = content.TrimStart('%').TrimStart(' ');
                int exit = groupName.IndexOf('\n');
                if (exit>=0)
                    groupName = groupName.Remove(exit);

                // Get the root item, copy it and add it to the child
                var rootItemData = ItemData.itemDatas.Find(x => x.name == groupName);
                if (rootItemData != null)
                    data.roots.Add(ItemData.Copy(rootItemData));

                // OVERRIDE the properties of the group
                /*var lines = content.Split('\n');
                for (int j = 1; j < lines.Length; j++) {
                    var split = lines[j].Split(':');
                    var nestedProp = data.properties.Find(x => x.name == split[0]);
                    if ( nestedProp == null) {
                        Debug.LogError($"GROUP PARSING : no property named {split[0]} in group {groupName}");
                        continue;
                    }
                    nestedProp.SetPart("value", split[1]);
                }*/
                continue;
            }

            // load sequences
            int sequenceDuration = 0;
            // Not props but events verb & sequences
            if (content.StartsWith('$') || content.StartsWith('E') || cells[i].StartsWith('!')) {
                var cellParts = cells[i].Split(new char[1] { '\n' }, 2);
                var firstLine = cellParts[0];

                int parenthesesIndex = firstLine.IndexOf('(');
                if (parenthesesIndex >= 0) {
                    string duration_txt = firstLine.Remove(0, parenthesesIndex+1);
                    duration_txt = duration_txt.Remove(duration_txt.IndexOf(')'));
                    int.TryParse(duration_txt, out sequenceDuration);
                    firstLine = firstLine.Remove(parenthesesIndex);
                }

                var triggers = firstLine.Remove(0, 2).Split('/');
                for (int t = 0; t < triggers.Length; t++)
                    triggers[t] = triggers[t].Trim(' ');

                var seq = new Sequence(null, cellParts[1]);
                seq.triggers = triggers;
                seq.duration = sequenceDuration;
                if (content.StartsWith('$'))
                    data.verbSequences.Add(seq);
                if (content.StartsWith('!'))
                    data.sequences.Add(seq);
                continue;
            }

            var cellLines = content.Split('\n');
            // add prop
            if (cellLines[0].Contains(':')) {
                // one liners
                for (int a = 0; a < cellLines.Length; a++) {
                    var split = cellLines[a].Split(':');
                    var prop = data.properties.Find(x => x.name == split[0]);
                    if ( prop == null) {
                        prop = new Property();
                        prop.name = split[0];
                        data.properties.Add(prop);
                    }
                    prop.SetPart("value", split[1]);
                }
            } else {
                var prop = new Property();
                prop.Parse(cellLines);
                data.properties.Add(prop);
                var onvalueparts = prop.parts.FindAll(x => x.key.StartsWith("OnValue"));
                foreach (var ev in onvalueparts) {
                    var seq = new Sequence(ev.content);
                    seq.triggers = new string[1] { ev.key };
                    prop.sequences.Add(seq);
                }

                var eventParts = prop.parts.FindAll(x => x.key.StartsWith('E'));
                foreach (var ev in eventParts) {
                    var seq = new Sequence(ev.content);
                    seq.triggers = new string[1] { ev.key };
                    prop.sequences.Add(seq);
                }
            }

        }
    }

    void ThrowError(string message) {
        Debug.Log($"{message} at row {row} and sheet {sheetName}");
    }
}