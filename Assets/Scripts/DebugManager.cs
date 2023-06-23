using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [Header("[TILE]")]
    [Space]
    public Tile tile;
    [Header("[VERB]")]
    public Verb verb;
    [Space]

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
    public List<Item> recentItems = new List<Item>();

    [Header("[CURRENT ITEMS]")]
    public List<ItemGroup> itemGroups;

    [Header("[ITEM EVENTS]")]
    public List<ItemEvent> propertyEvents = new List<ItemEvent>();

    [Header("[PLAYER]")]
    public Player player;

    private void Start()
    {
        currentFunction = Function.current;
        currentFunctionList = FunctionSequence.current;
        propertyEvents = ItemEvent.list;

        availableItems = AvailableItems.list;
        recentItems = AvailableItems.recentItems;

        itemGroups = ItemGroup.debug_groups;

        player = Player.Instance;

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
