using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEditor.UIElements;
using UnityEngine;

[System.Serializable]
public class Socket
{
    // LOCAL //
    // makes sens that the socket are unique ( no instances )
    // because they will not change, there will be no copy and nothing added to them

    // item that can appear in the socket
    public List<int> itemIndexes = new List<int>();

    // on the left and on the right
    // at the foot of a tree
    public List<string> _positions = new List<string>();

    // the road
    // 3 apples
    // a watering can
    public List<ItemGroup> itemGroups = new List<ItemGroup>();

    #region positions
    public void SetPosition(string str)
    {
        if (_positions.Count == 0)
        {
            _positions.Add(str);
        }
        else
        {
            _positions[0] = str;
        }
    }

    public void AddPosition(string str)
    {
        _positions.Add(str);
    }

    public string GetPosition()
    {
        return GetPosition(0);
    }

    public string GetPosition(int i)
    {
        if (i >= _positions.Count)
        {
            Debug.LogError("position out of range for socket");
            return "";
        }

        return _positions[i];
    }
    public string GetPositionText()
    {
        string str = "";

        for (int i = 0; i < _positions.Count; i++)
        {
            str += CheckKeywords(_positions[i]);

            str += TextUtils.GetLink(i, _positions.Count);
        }

        return str;
    }
    #endregion

    #region text

    private string CheckKeywords(string content)
    {
        switch (content)
        {
            case "front":
                return "in front on you";
            case "right":
                return "on your right";
            case "behind":
                return "behind you";
            case "left":
                return "on your left";
        }

        return content;
    }

    #endregion

    #region item groups
    public Item GetItem()
    {
        return GetItem(0);
    }

    public Item GetItem(int index)
    {
        return itemGroups[index].item;
    }

    public void AddItem(Item item)
    {
        AddItem(item, false);
    }

    public void AddItem(Item item, bool forceNew)
    {
        // see if the item is already in the socket
        Socket.ItemGroup itemGroup = itemGroups.Find(x => x.item.dataIndex == item.dataIndex);

        if (itemGroup == null || forceNew)
        {
            // it isn't, so add it
            itemGroup = new ItemGroup();

            itemGroup.item = item;

            itemGroup.count = 1;

            if (forceNew)
            {
                itemGroup.hidden = true;
            }

            itemGroups.Add(itemGroup);
        }
        else
        {

            // is already is, so add to the item group count
            itemGroup.count += 1;

        }
    }

    [System.Serializable]
    public class ItemGroup
    {
        public Item item;
        public int count;
        public bool hidden = false;

        public string GetWordGroup(string key)
        {
            if (count > 5)
            {
                return "a lot " + item.word.GetContent("of dogs");
            }
            else if (count > 3)
            {
                return "a few " + item.word.GetContent("dogs");
            }
            else if (count > 1)
            {
                return count + " " + item.word.GetContent("dogs");
            }
            else
            {
                return item.word.GetContent(key);
                /*if (!item.stackable)
                {
                    return item.word.GetContent("a good dog");
                }
                else
                {
                    return item.word.GetContent("a dog");
                }*/
            }
        }
    }

    public string GetItemsText(string wordInfo)
    {
        string text = "";

        List<ItemGroup> _groups = itemGroups.FindAll(x => !x.hidden);

        int l = _groups.Count;
        for (int i = 0; i < l; i++)
        {
            text += _groups[i].GetWordGroup(wordInfo);
            text += TextUtils.GetLink(i, l);
        }

        return text;
    }
    #endregion

}
