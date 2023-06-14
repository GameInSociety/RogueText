using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Analytics;

public class ItemLoader : TextParser {

    // singleton
    public static ItemLoader Instance;

    int itemIndex = 0;

    public Item[] items_debug;

    private void Awake()
    {
        Instance = this;
    }

    public override void FinishLoading()
    {
        base.FinishLoading();

        // debug list to explore items
        items_debug = ItemManager.Instance.dataItems.ToArray();
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
            return;
        }

        // skip empty
        if (cells.Count > 0 && string.IsNullOrEmpty(cells[0]))
        {
            return;
        }

        // * est pour passer les lignes vides,
        // car on ne peut pas supprimer dans lignes ( elle ont des liens dans les autres DATAs )
        // quand on rajouter un nouveau mot, il faut utiliser les * d'abord
        if ( cells[0][0] == '*')
        {
            // can't continue, parce que sinon ça décale tout donc juste on fait rien
        }

        // create new item
        Item newItem = new Item();

        string nameCell = cells[0];
        string[] synonyms = nameCell.Split('\n');

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
            newWord.UpdateNumber(cells[1]);

            if (cells[2].Length > 1)
            {
                // ainsi que les locations peut être
                newItem.word.SetLocationPrep(cells[2]);
            }
        }

        //newItem.index = rowIndex-1;
        newItem.dataIndex = itemIndex;
        // add to item list
        ItemManager.Instance.dataItems.Add(newItem);

        newItem.info.invisible = cells[3].Contains("invisible");

        if ( cells.Count > 4)
        {
            for (int i = 4; i < cells.Count; i++)
            {
                if (string.IsNullOrEmpty(cells[i]))
                {
                    // no properties in the item, continue
                    break;
                }

                InitProperty(newItem, cells[i]);
            }
        }

        ++itemIndex;


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
        for (int i = 0; i < lines.Length; i++)
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