using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour {

    [SerializeField] private bool _description_DebugList;

    public List<ItemParser> parsers = new List<ItemParser>();

    public bool Description_DebugList()
    {
        return _description_DebugList;
    }

    private void Start() {
        parsers = ItemParser.debug_archive;
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
