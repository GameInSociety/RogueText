using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [Header("[TILE]")]
    [Space]
    public Tile tile;
    [Space]
    [Header("[INVENTORY]")]
    [Space]
    public Item inventory;

    [Space]
    [Header("[FUNCTION]")]
    [Space]
    public List<Item> function_Items;
    public List<string> function_Params;
    public List<Property> function_Properties;

    [Header("[TEXT]")]
    public bool colorWords = true;

    [Header("[AVAILABLE ITEMS]")]
    public List<Item> availableItems = new List<Item>();

    [Header("[WORLD EVENTS]")]
    public List<WorldEvent> worldEvents = new List<WorldEvent>();

    private void Start()
    {
        worldEvents = WorldEvent.list;
        availableItems = AvailableItems.GetItems;
        function_Params = FunctionManager.GetParams();
        function_Properties = FunctionManager.pendingProps;
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
