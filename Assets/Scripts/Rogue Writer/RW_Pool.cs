using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;


[System.Serializable]
public class RW_Pool
{
    public string id;
    public int count;
    public Displayable prefab;
    public Transform parent;
    public List<Displayable> stack = new List<Displayable>();
    public List<Displayable> active = new List<Displayable>();
}
