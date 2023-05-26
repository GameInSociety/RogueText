using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Story : MonoBehaviour
{
    public static Story Instance;

    [System.Serializable]
    public class Param
    {
        public string key = "";
        public bool value = false;
    }

    public List<Param> _params = new List<Param>();

    void Awake()
    {
        Instance = this;
    }

    void HandleSetParam()
    {
        bool b = CellEvent.GetContent(0) == "true";
        SetParam(CellEvent.GetContent(0), b);
    }

    public bool GetParam(string key)
    {
        Param param = _params.Find(x => x.key == key);

        if (param == null)
        {
            Debug.LogError("couldn't find param " + key);
            return false;
        }

        return param.value;
    }

    public void SetParam(string key, bool value)
    {
        Param param = _params.Find(x => x.key == key);

        if ( param == null )
        {
            Debug.LogError("couldn't find param " + key);
            return;
        }

        param.value = value;
    }
}
