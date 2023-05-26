using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public Tile tile;
    public Item inventory;
    public InputInfo inputInfo;

    public bool colorWords = true;

    public List<Item> availableItems = new List<Item>();

    public List<string> itemsOnTile = new List<string>();

    private void Start()
    {
        availableItems = AvailableItems.List;

        foreach (var item in itemsOnTile)
        {
            
        }
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
