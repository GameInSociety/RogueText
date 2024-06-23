using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour {

    public int value;
    public int max;
    public float lerp;
    public int index;
    public string result;
    public string descriptions;
    public bool strict = false;

    private void Update() {
        /*lerp = (float)value / max;
        var split = descriptions.Split('/');
        index = (int)(lerp * split.Length);
        result = split[index];*/

        if (strict) {
            lerp = (float)value / max;
            var split = descriptions.Split('/');
            index = (int)(lerp * split.Length);
            result = split[index];
        } else {
            lerp = (float)value / max;
            var split = descriptions.Split('/');
            index = (int)(lerp * (split.Length - 2));
            if (value == 0)
                result = split[0];
            else
                result = split[1 + index];
        }
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
