using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [Header("[TILE]")]
    [Space]
    public Tile tile;
    [Header("[VERB]")]
    public Verb verb;
    [Space]
    [Header("[INVENTORY]")]
    [Space]
    public Item inventory;

    public bool AI_Enabled = false;

    [Space]
    [Header("[FUNCTION]")]
    [Space]
    public Function currentFunction;
    public FunctionSequence currentFunctionList;

    [Header("[TEXT]")]
    public bool colorWords = true;

    [Header("[AVAILABLE ITEMS]")]
    public List<Item> availableItems = new List<Item>();

    [Header("[CURRENT ITEMS]")]
    public List<Item> currentItems;

    [Header("[ITEM EVENTS]")]
    public List<ItemEvent> propertyEvents = new List<ItemEvent>();


    private void Start()
    {
        currentFunction = Function.current;
        currentFunctionList = FunctionSequence.current;
        propertyEvents = ItemEvent.list;
        currentItems = CurrentItems.list;
        availableItems = AvailableItems.list;
        verb = Verb.GetCurrent;
    }

    private static DebugManager _instance;
    public static DebugManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<DebugManager>().GetComponent<DebugManager>();
            }

            return _instance;
        }
    }
}
