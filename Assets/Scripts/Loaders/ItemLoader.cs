using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using UnityEditor.PackageManager;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Analytics;

public class ItemLoader : DataDownloader {

    // singleton
    public static ItemLoader Instance;

    int itemIndex = 0;

    public Item[] items_debug;

    public int propertyIndex = 0;
    public int actionIndex = 0;
    public int appearRateIndex = 0;


    private void Awake() {
        Instance = this;
    }

    public override void FinishLoading() {
        base.FinishLoading();
        // debug list to explore items
        items_debug = Item.dataItems.ToArray();
        foreach (var item in items_debug)
            item.debug_name = item.word.text;
        //
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);
        if (rowIndex == 0) {
            propertyIndex = cells.FindIndex(x => x == "PROPERTIES");
            actionIndex = cells.FindIndex(x => x == "ACTIONS");
            appearRateIndex = cells.FindIndex(x => x == "CONTAINED ITEMS");
            return;
        }

        // skip empty
        if (cells.Count > 0 && string.IsNullOrEmpty(cells[0]))
            return;

        // create new item
        var newItem = new Item();

        var nameCell = cells[0];
        var synonyms = nameCell.Split('\n');

        // has specific class name
        if (!string.IsNullOrEmpty(cells[1]))
            newItem.className = cells[1];

        // SYNONYMS HERE
        // new word
        for (var i = 0; i < synonyms.Length; i++) {
            // pour l'instant le mot peut être reconnu mais il va toujours chercher le premier
            // de la liste words
            var newWord = new Word();
            newWord.SetText(synonyms[i]);

            // word
            newItem.words.Add(newWord);
            // pour synonym autre ligne aussi dans les genre
            newWord.UpdateNumber(cells[2]);

            if (cells[3].Length > 1) {
                // ainsi que les locations peut être
                newItem.word.locationPrep = cells[3];
            }
        }

        //newItem.index = rowIndex-1;
        newItem.dataIndex = itemIndex;
        // add to item list
        Item.dataItems.Add(newItem);

        newItem.infos = cells[4].Split('\n').ToList();

        LoadProperties(cells);
        LoadActions(cells);
        LoadAppearRates(cells);
        ++itemIndex;
    }

    void LoadProperties(List<string> cells) {

        if (cells.Count <= propertyIndex)
            return;
        for (var i = propertyIndex; i < actionIndex; i++) {
            if (string.IsNullOrEmpty(cells[i]))
                break;
            InitProperty(Item.dataItems[itemIndex], cells[i]);
        }
    }

    void LoadActions(List<string> cells) {
        if (cells.Count <= actionIndex)
            return;
        for (var i = actionIndex; i < appearRateIndex; i++) {
            if (string.IsNullOrEmpty(cells[i]))
                break;
            initAction(cells[i]);
        }
    }

    void LoadAppearRates(List<string> cells) {
        var appearInfo = new AppearInfo();
        appearInfo.name = cells[0];
        Item.appearInfos.Add(appearInfo);

        if (cells.Count <= appearRateIndex)
            return;
        for (var i = appearRateIndex; i < cells.Count; i++) {
            if (string.IsNullOrEmpty(cells[i]))
                break;
            initAppearInfo(cells[i], Item.appearInfos[itemIndex]);
        }
    }

    public void initAppearInfo(string cell, AppearInfo appearInfo) {
        var itemInfo = new AppearInfo.ItemInfo();
        itemInfo.chance = 100;
        itemInfo.amount = 1;
        // chance
        if (cell.Contains('%')) {
            try {
                var s = cell.Split('%')[0];
                cell = cell.Split('%')[1];
                itemInfo.chance = int.Parse(s);
            } catch (Exception e) {
                Debug.LogError($"SPAWN CHANCE ERROR ({cell})({e})");
            }
        }
        if (cell.Contains('*')) {
            try {
                var s = cell.Split('*')[1];
                cell = cell.Split('*')[0];
                itemInfo.amount = int.Parse(s);
            } catch (Exception e) {
                Debug.LogError($"SPAWN AMOUNT ERROR ({cell})({e})");
            }
        }

        // name or type
        itemInfo.name = cell;
        appearInfo.itemInfos.Add(itemInfo);
    }

    void initAction(string cell) {
        var firstLine = cell.Split('\n')[0];
        firstLine = firstLine.TrimEnd(' ');
        cell = cell.Remove(0, cell.IndexOf('\n') + 1);

        var strs = firstLine.Split(" / ");
        foreach (var str in strs) {
            var verb = Verb.FindInData(str);
            verb.AddCell(itemIndex, cell);
        }
    }

    void InitProperty(Item item, string cell) {
        /// PROPERTIES
        var lines = cell.Split('\n');

        /// CREATE PROPERTY ///
        var newProperty = new Property();

        var parts = lines[0].Split(" / ").ToList();
        newProperty.type = parts[0];

        if (parts.Count < 2) {
            ThrowError($"property error : couldn't parse cell {cell}");
            return;
        }

        newProperty.name = parts[1];
        if (parts.Count > 2) {
            newProperty.SetValue(parts[2]);
        }

        item.properties.Add(newProperty);


        /// GET EVENTS AND FUNCTIONS
        var getFunctions = false;
        for (var i = 1; i < lines.Length; i++) {
            var line = lines[i];

            if (string.IsNullOrEmpty(line)) {
                // pour séparer les evenements
                if (getFunctions) {
                    var lastProp = item.properties[item.properties.Count - 1];
                    var lastEvent = lastProp.eventDatas[lastProp.eventDatas.Count - 1];
                    lastEvent.cellContent = lastEvent.cellContent.Trim(new char[1] { '\n' });
                    getFunctions = false;
                }

                continue;
            }

            // an event line, subscribes the property to an event
            if (line.StartsWith('%')) {
                var eventName = line;
                var propertyEvent = new Property.EventData();
                propertyEvent.eventName = eventName.Remove(0, 1);
                var lastProp = item.properties[item.properties.Count - 1];
                lastProp.AddEventData(propertyEvent);
                getFunctions = true;
                continue;
            }

            if (line.StartsWith('&')) {
                string description;

                var lastProp = item.properties[item.properties.Count - 1];

                if (line.StartsWith("&&")) {
                    // always describe
                    description = line.Remove(0, 2);
                    lastProp.alwaysDescribe = true;
                } else {
                    // describe when described
                    description = line.Remove(0, 1);
                }

                lastProp.descriptions = description.Split('/');
            }

            // getting the function of it
            if (getFunctions) {
                var lastProp = item.properties[item.properties.Count - 1];
                var lastEvent = lastProp.eventDatas[lastProp.eventDatas.Count - 1];
                lastEvent.cellContent += line + '\n';
                continue;
            }

            //newItem.CreateProperty(property_line);
        }
        //
    }

    void ThrowError(string message) {
        Debug.Log($"{message} at row {row} and sheet {sheet}");
    }
}