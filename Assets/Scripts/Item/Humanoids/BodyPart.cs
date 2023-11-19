using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BodyPart : Item {

    public override void WriteDescription() {
        base.WriteDescription();

        /*if (!ContainsItems())
        {
            TextManager.Write("I'm not wearing anything on my &dog&", this);
        }
        else
        {
            TextManager.Write("I'm wearing ");

            for (int i = 0;i < GetChildItems.Count;i++)
            {
                TextManager.Add("&a dog&", GetChildItems[i]);
                TextManager.AddLink(i, GetChildItems.Count);
            }

            TextManager.Add(" on my &dog&", this);
        }*/
    }
}
