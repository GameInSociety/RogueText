using System;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ItemLoader : DataDownloader {

    // singleton
    public static ItemLoader Instance;
    int currIndex = 0;

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

        // range
        var rangeProp = new Property();
        rangeProp.name = "range";
        rangeProp.AddPart("value", !string.IsNullOrEmpty(cells[2]) ? cells[2] : "0");
        newItemData.properties.Add(rangeProp);

        // preposition
        newItemData.words[0].preposition = cells[3].Length > 1 ? cells[3] : "in";

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

        // add grammar
        if (newItemData.properties.Find(x=> x.name == "grammar") == null) {
            var gProp = new Property();
            gProp.name = "grammar";
            newItemData.properties.Add(gProp);
        }

        var nameProp = new Property();
        nameProp.name = "name";
        nameProp.AddPart("value", $"{newItemData.name}");
        newItemData.properties.Add(nameProp);

        ItemData.itemDatas.Add(newItemData);
        ++currIndex;
    }

    void LoadProperties(ItemData data, List<string> cells) {
        for (var i = 0; i < cells.Count; i++) {
            if (string.IsNullOrEmpty(cells[i]))
                continue;

            string content = cells[i];

            // load groups
            if (content.StartsWith('%')) {
                // loading all properties of a group
                // extracting the first line ( in the case there is replacements )
                var groupName = content.TrimStart('%').TrimStart(' ');
                int exit = groupName.IndexOf('\n');
                if (exit>=0)
                    groupName = groupName.Remove(exit);

                // get the group
                ContentLoader.Group group = ContentLoader.GetGroup(groupName);
                if (group == null) {
                    TextManager.Write($"[Prop Load Error ] : No property group named : {groupName}", Color.red);
                    Debug.LogError($"no group named {group}");
                    continue;
                }

                // loading the properties of the gorup
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
                    nestedProp.SetPart("value", split[1]);
                }
                continue;
            }

            try {
                // load sequences
                int sequenceDuration = 0;
                if (content.StartsWith('$') || content.StartsWith('E') || cells[i].StartsWith('!')) {
                    var cellParts = cells[i].Split(new char[1] { '\n' }, 2);
                    var firstLine = cellParts[0];


                    int parenthesesIndex = firstLine.IndexOf('(');
                    if (parenthesesIndex >= 0) {
                        string duration_txt = firstLine.Remove(0, parenthesesIndex+1);
                        duration_txt = duration_txt.Remove(duration_txt.IndexOf(')'));
                        Debug.Log($"Sequence Duration TXT : {duration_txt}");
                        int.TryParse(duration_txt, out sequenceDuration);
                        Debug.Log($"Sequence Duration : {sequenceDuration}");
                        firstLine = firstLine.Remove(parenthesesIndex);
                        Debug.Log($"First Line OUTPUT: {firstLine}");
                    }

                    var triggers = firstLine.Remove(0, 2).Split('/');
                    for (int t = 0; t < triggers.Length; t++)
                        triggers[t] = triggers[t].Trim(' ');

                    var seq = new Sequence(triggers, cellParts[1]);
                    seq.duration = sequenceDuration;
                    if (content.StartsWith('$')) 
                        data.verbSequences.Add(seq);
                    if (content.StartsWith('!')) {
                        data.acts.Add(seq);
                    }
                    continue;
                }
            } catch (Exception e) {
                Debug.LogError($"cell : {cells[i]} / sheet : {sheetName}");
                TextManager.Write($"error loading content : {content} on {data.name}", Color.red);
                Debug.LogException(e);
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
                try {
                    var prop = new Property();
                    prop.Parse(cellLines);
                    data.properties.Add(prop);
                } catch (Exception e) {
                    var str = "";
                    foreach (var line in cellLines) {
                        foreach (var item in cellLines)
                            str += $"\n{item}";
                    }
                    Debug.LogError($"<color=yellow>PROP INIT</color>:{str}\n");
                    Debug.LogException(e);
                }
                
            }


        }
    }

    void ThrowError(string message) {
        Debug.Log($"{message} at row {row} and sheet {sheetName}");
    }
}