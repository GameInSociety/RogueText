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

    public bool AI_Enabled = false;

    [Space]
    [Header("[FUNCTION]")]
    [Space]
    public Function currentFunction;
    public WorldEvent currentFunctionList;

    [Header("[TEXT]")]
    public bool colorWords = true;

    [Header("[AVAILABLE ITEMS]")]
    public List<Item> availableItems = new List<Item>();


    private void Start()
    {
        currentFunction = Function.current;
        currentFunctionList = WorldEvent.current;
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
