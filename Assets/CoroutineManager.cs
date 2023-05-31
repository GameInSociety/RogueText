using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager _instance;
    public static CoroutineManager Instance
    {
        get { 

            if (_instance == null)
            {
                _instance = new CoroutineManager();
            }

            return _instance;
        }
    }

    public delegate void OnWait();
    public OnWait onWait;
    public void Wait(string func, float d)
    {
        CancelInvoke(func);
        Invoke(func, d);
    }
}
