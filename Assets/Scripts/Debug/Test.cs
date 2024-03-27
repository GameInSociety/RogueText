using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Rendering;

public class Test : MonoBehaviour {

    public string tile = "north";
    public Coords coords;

    public List<ItemGroup> groups = new List<ItemGroup>();
    public string feedback;
    public List<ItemGroup> samePropGroups = new List<ItemGroup>();

}
