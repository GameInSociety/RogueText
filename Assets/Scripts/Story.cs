using System.Collections.Generic;
using UnityEngine;

public class Story : MonoBehaviour {
    public static Story Instance;

    [System.Serializable]
    public class Param {
        public string key = "";
        public bool value = false;
    }

    public List<Param> _params = new List<Param>();

    void Awake() {
        Instance = this;
    }

    public bool GetParam(string key) {
        var getParam = _params.Find(x => x.key == key);

        if (getParam == null) {
            Debug.LogError("couldn't find GetPart " + key);
            return false;
        }

        return getParam.value;
    }

    public void SetParam(string key, bool value) {
        var getParam = _params.Find(x => x.key == key);

        if (getParam == null) {
            Debug.LogError("couldn't find GetPart " + key);
            return;
        }

        getParam.value = value;
    }
}
