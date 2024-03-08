using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour {

    // TESTS //
    // 1) plates (10x, tester les piles)
    // 2) flashlight (flashlight + battery)
    // 3) gardening (graine)
    // 4) doors & keys
    // 5) boat (� voir)

    [Header("[ITEM LINK]")]
    public List<Item> itemsFound = new List<Item>();
    public List<Property> propsFound = new List<Property>();

    [Space]
    [Header("[TILE]")]
    public Tile TILE;
    public List<Item> adjacentTiles;


    [Space]
    [Header("[PARSER]")]
    public ItemParser previousParser;
    public ItemParser currentParser;

    [Space]
    [Header("[FUNCTION]")]
    public FunctionSequence currentFunctionList;

    [Space]
    [Header("[TEXT]")]
    public bool colorWords = true;

    [Space]
    [Header("[AVAILABLE ITEMS]")]
    public List<Item> availableItems;

    public List<Item> globalItems;

    [Space]
    [Header("[ITEM EVENTS]")]
    public List<WorldEvent> debug_worldEvents = new List<WorldEvent>();
    public List<PropertyDescription> debug_PropertyDescriptions;

    public bool displayItemID;

    [Space]
    [Header("[PLAYER]")]
    public Player PLAYER;

    private void Start() {
        currentParser = ItemParser.GetCurrent;
        previousParser = ItemParser.GetPrevious;

        availableItems = AvailableItems.currItems;

        globalItems = WorldData.globalItems;
        debug_worldEvents = WorldEvent.worldEvents;


        debug_PropertyDescriptions = PropertyDescription.propDescriptions;

        PLAYER = Player.Instance;
    }
    private static DebugManager _instance;
    public static DebugManager Instance {
        get {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<DebugManager>().GetComponent<DebugManager>();
            return _instance;
        }
    }

    
}
