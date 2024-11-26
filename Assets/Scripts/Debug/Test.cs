using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Rendering;

public class Test : MonoBehaviour {

    public List<SlotText> slots = new List<SlotText>();

    public class SlotText
    {
        public string name;
        public string prop;
    }

}
