using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private void Awake()
    {
        Instance = this;
    }

    public override void FinishLoading()
    {
        base.FinishLoading();

        // debug list to explore items
        items_debug = Item.dataItems.ToArray();
        foreach (var item in items_debug)
        {
            item.debug_name = item.word.text;
        }
        //
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);


        // name row
        if (rowIndex < 1)
        {
            int col = 0;
            foreach (var cell in cells)
            {
                if ( cell == "PROPERTIES")
                {
                    propertyIndex = col; 
                }
                else if (cell == "ACTIONS")
                {
                    actionIndex = col;
                }
                else if (cell == "CONTAINED ITEMS")
                {
                    appearRateIndex = col;
                    break;
                }
                ++col;
            }

            return;
        }

        // skip empty
        if (cells.Count > 0 && string.IsNullOrEmpty(cells[0]))
        {
            Debug.Log("skipping");
            return;
        }

        // create new item
        Item newItem = new Item();

        string nameCell = cells[0];
        string[] synonyms = nameCell.Split('\n');

        if (!string.IsNullOrEmpty(cells[1]))
        {
            newItem.className = cells[1];
        }

        // SYNONYMS HERE
        // new word
        for (int i = 0; i < synonyms.Length; i++)
        {
            // pour l'instant le mot peut être reconnu mais il va toujours chercher le premier
            // de la liste words
            Word newWord = new Word();
            newWord.SetText(synonyms[i]);

            // word
            newItem.words.Add(newWord);
            // pour synonym autre ligne aussi dans les genre
            newWord.UpdateNumber(cells[2]);

            if (cells[3].Length > 1)
            {
                // ainsi que les locations peut être
                newItem.word.locationPrep = cells[3];
            }
        }

        //newItem.index = rowIndex-1;
        newItem.dataIndex = itemIndex;
        // add to item list
        Item.dataItems.Add(newItem);

        newItem.infos = cells[4].Split('\n').ToList();

        if ( newItem.debug_name == "chair")
        {
            Debug.Log("wtf chair");
        }

        LoadProperties(cells);
        LoadActions(cells);
        LoadAppearRates(cells);

        ++itemIndex;


    }

    void LoadProperties(List<string> cells)
    {

        if (cells.Count <= propertyIndex)
        {
            return;
        }

        for (int i = propertyIndex; i < actionIndex; i++)
        {
            if (string.IsNullOrEmpty(cells[i]))
            {
                // no properties in the item, continue
                break;
            }

            InitProperty(Item.dataItems[itemIndex], cells[i]);
        }
    }

    void LoadActions(List<string> cells)
    {
        if (cells.Count <= actionIndex)
        {
            return;
        }

        for (int i = actionIndex; i < appearRateIndex; i++)
        {
            if (string.IsNullOrEmpty(cells[i]))
            {
                // no properties in the item, continue
                break;
            }

            InitAction(cells[i]);
        }
    }

    void LoadAppearRates(List<string> cells)
    {
        AppearInfo appearInfo = new AppearInfo();
        appearInfo.name = cells[0];
        Item.appearInfos.Add(appearInfo);


        if (cells.Count <= appearRateIndex)
        {
            return;
        }

        for (int i = appearRateIndex; i < cells.Count; i++)
        {
            if (string.IsNullOrEmpty(cells[i]))
            {
                // no properties in the item, continue
                break;
            }

            
            InitAppearRate(cells[i]);
        }
    }

    void InitAppearRate(string cell)
    {
        AppearInfo.ItemInfo itemInfo = new AppearInfo.ItemInfo();

        // name or type
        string[] parts = cell.Split(", ");
        string itemName = parts[0];
        itemInfo.name = itemName;

        // params
        itemInfo.chanceAppear = 100;
        itemInfo.amount = 1;

        for (int j = 1; j < parts.Length; j++)
        {
            string part = parts[j];

            if (part.Contains('%'))
            {
                string chanceAppear_str = parts[1].Remove(part.Length - 1);
                if (!int.TryParse(chanceAppear_str, out itemInfo.chanceAppear))
                {
                    Debug.LogError("APPEAR INFO LOAD : couldn't parse chance appear : " + chanceAppear_str);
                }
            }
            else if (part.Contains('x'))
            {
                string amount_str;
                if (part.StartsWith('x'))
                {
                    amount_str = part.Remove(0, 1);
                }
                else
                {
                    amount_str = part.Remove(part.Length - 1);
                }

                // amount
                if (!int.TryParse(amount_str, out itemInfo.amount))
                {
                    Debug.LogError("APPEAR INFO LOAD : couldn't parse amount : " + amount_str);
                }
            }
        }

        Item.appearInfos[itemIndex].itemInfos.Add(itemInfo);
    }

    void InitAction(string cell)
    {
        string firstLine = cell.Split('\n')[0];
        firstLine = firstLine.TrimEnd(' ');
        cell = cell.Remove(0, cell.IndexOf('\n') +1);

        string[] strs = firstLine.Split(" / ");
        foreach (string str in strs)
        {
            Verb verb = Verb.FindInData(str);
            verb.AddCell(itemIndex, cell);
        }

    }

    void InitProperty(Item item, string cell)
    {
        /// PROPERTIES
        string[] lines = cell.Split('\n');

        /// CREATE PROPERTY ///
        Property newProperty = new Property();

        List<string> parts = lines[0].Split(" / ").ToList();
        newProperty.type = parts[0];

        if ( parts.Count < 2)
        {
            Debug.LogError("couldn't parse property : " + cell);
            return;
        }

        newProperty.name = parts[1];
        if (parts.Count > 2)
        {
            newProperty.SetValue(parts[2]);
        }

        item.properties.Add(newProperty);


        /// GET EVENTS AND FUNCTIONS
        bool getFunctions = false;
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrEmpty(line))
            {
                // pour séparer les evenements
                if(getFunctions)
                {
                    Property lastProp = item.properties[item.properties.Count - 1];
                    Property.EventData lastEvent = lastProp.eventDatas[lastProp.eventDatas.Count - 1];
                    lastEvent.cellContent = lastEvent.cellContent.Trim(new char[1] {'\n'});
                    
                    /*Debug.Log("<b>" + lastEvent.name + "</b>\n" +
                        lastEvent.content);*/

                    getFunctions = false;
                }

                continue;
            }

            // an event line, subscribes the property to an event
            if (line.StartsWith('%'))
            {
                string eventName = line;
                Property.EventData propertyEvent = new Property.EventData();
                propertyEvent.eventName = eventName.Remove(0, 1);
                Property lastProp = item.properties[item.properties.Count - 1];
                lastProp.AddEventData(propertyEvent);
                getFunctions = true;
                continue;
            }

            if (line.StartsWith('&'))
            {
                string description;

                Property lastProp = item.properties[item.properties.Count - 1];

                if (line.StartsWith("&&"))
                {
                    // always describe
                    description = line.Remove(0, 2);
                    lastProp.alwaysDescribe = true;
                }
                else
                {
                    // describe when described
                    description = line.Remove(0, 1);
                }

                lastProp.descriptions = description.Split('/');
            }

            // getting the function of it
            if (getFunctions)
            {
                Property lastProp = item.properties[item.properties.Count - 1];
                Property.EventData lastEvent = lastProp.eventDatas[lastProp.eventDatas.Count - 1];
                lastEvent.cellContent += line + '\n';
                continue;
            }

            //newItem.CreateProperty(property_line);
        }
        //
    }
}