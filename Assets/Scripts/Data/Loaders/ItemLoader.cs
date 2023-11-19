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
        // create new item
        var newItemData = new ItemData();
        var nameCell = cells[0];
        newItemData.debugName = nameCell;
        var synonyms = nameCell.Split('\n');

        // hasPart specific class name
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
            for (int i = 0; i < types.Length; i++) {
                if (string.IsNullOrEmpty(types[i]) ) {  continue; }
                var type_prop = new Property();
                type_prop.name = types[i] + " (type)";
                newItemData.properties.Add(type_prop);
            }
        }
        

        //newItem.index = rowIndex-1;
        newItemData.index = currIndex;
        // add to item list
        ItemData.itemDatas.Add(newItemData);
        LoadProperties(cells);
        ++currIndex;
    }

    void LoadProperties(List<string> cells) {

        if (cells.Count <= 5)
            return;

        for (var i = 5; i < cells.Count; i++) {
            if (string.IsNullOrEmpty(cells[i]))
                break;

            // $ = verb action // ! = item action ( ex : undead attacks )
            if (cells[i].StartsWith('$') || cells[i].StartsWith('!')) {
                var cellParts = cells[i].Split(new char[1] { '\n' }, 2);
                try {
                    var verbs = cellParts[0].Remove(0, 2);
                    var seq = new ItemData.Sequence(verbs, cellParts[1]);
                    ItemData.itemDatas[currIndex].sequences.Add(seq);
                } catch (Exception e) {
                    Debug.Log($"error on loading sequence (item:{ItemData.itemDatas[currIndex].debugName}) (cell:{cells[i]})");
                    Debug.LogError(e.Message);
                }
                
                continue;
            }

            ItemData.itemDatas[currIndex].properties.Add(new Property(cells[i]));
        }
    }

    void ThrowError(string message) {
        Debug.Log($"{message} at row {row} and sheet {sheet}");
    }
}