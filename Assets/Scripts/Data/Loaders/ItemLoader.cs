using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

public class ItemLoader : DataDownloader {

    // singleton
    public static ItemLoader Instance;
    int currIndex = 0;
    public ItemData[] items_debug;


    private void Awake() {
        Instance = this;
    }

    public override void FinishLoading() {
        base.FinishLoading();
        // debug list to explore items
        items_debug = ItemData.itemDatas.ToArray();
        //
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        if (rowIndex == 0)
            return;
        // skip empty
        if (cells.Count > 0 && string.IsNullOrEmpty(cells[0])) {
            return;
        }

        if (string.IsNullOrEmpty(cells[0])) {
            LoadProperties(ItemData.itemDatas[currIndex], cells);
            return;
        }

        // Create new item
        var newItemData = new ItemData();
        var nameCell = cells[0];
        newItemData.debugName = nameCell;
        var synonyms = nameCell.Split('\n');

        // HasPart numberSpecific class name
        if (!string.IsNullOrEmpty(cells[1]))
            newItemData.className = cells[1];

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


        //newItem.index = rowIndex-1;
        newItemData.index = currIndex;
        // add to item list
        LoadProperties(newItemData, cells);

        ItemData.itemDatas.Add(newItemData);
        ++currIndex;
    }

    void LoadProperties(ItemData data, List<string> cells) {

        if (cells.Count <= 5)
            return;

        for (var i = 5; i < cells.Count; i++) {
            if (string.IsNullOrEmpty(cells[i]))
                continue;

            // $ = verb action // ! = item action ( ex : undead attacks )
            if (cells[i].StartsWith('$') || cells[i].StartsWith('!')) {
                var cellParts = cells[i].Split(new char[1] { '\n' }, 2);
                try {
                    var verbs = cellParts[0].Remove(0, 2);
                    var seq = new ItemData.Sequence(verbs, cellParts[1]);
                    data.sequences.Add(seq);
                } catch (Exception e) {
                    Debug.Log($"error on loading sequence (item:{data.debugName}) (cell:{cells[i]})");
                    Debug.LogError(e.Message);
                }
                continue;
            }

            data.properties.Add(new Property(cells[i]));
        }
    }

    void ThrowError(string message) {
        Debug.Log($"{message} at row {row} and sheet {sheet}");
    }
}