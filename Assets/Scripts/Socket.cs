using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Socket
{
    public class Part {
        public string Content;
        public Property Property;
    }

    public Slot.Type Type;
    public Item Item;
    public Property Property;
    public string Content;

    public List<Socket.Part> Parts = new List<Part>();

    public Socket(Slot.Type type, string name) {
        Type = type;
        Content = name;
    }
}

