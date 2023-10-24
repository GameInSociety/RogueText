using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DebugManager : MonoBehaviour {

    // TESTS //
    // 1) plates (10x, tester les piles)
    // 2) flashlight (flashlight + battery)
    // 3) gardening (graine)
    // 4) doors & keys
    // 5) boat (à voir)

    [Space]
    [Header("[TILE]")]
    public Tile TILE;

    [Header("[PARSER]")]
    public List<ItemParser> parsers = new List<ItemParser>();

    [Space]
    [Header("[FUNCTION]")]
    public FunctionSequence currentFunctionList;

    [Space]
    [Header("[TEXT]")]
    public bool colorWords = true;

    [Space]
    [Header("[AVAILABLE ITEMS]")]
    public AvailableItems AVAILABLE_ITEMS;

    [Space]
    [Header("[ITEM EVENTS]")]
    public List<ItemEvent> EVENTS = new List<ItemEvent>();

    [Space]
    [Header("[PLAYER]")]
    public Player PLAYER;

    private void Start() {
        currentFunctionList = FunctionSequence.current;
        EVENTS = ItemEvent.list;

        parsers = ItemParser.history;

        AVAILABLE_ITEMS = AvailableItems.Get;

        PLAYER = Player.Instance;
    }
    private static DebugManager _instance;
    public static DebugManager Instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<DebugManager>().GetComponent<DebugManager>();
            }

            return _instance;
        }
    }
}
