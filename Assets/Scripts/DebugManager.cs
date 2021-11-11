using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public Tile tile;

    public bool colorWords = true;

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
