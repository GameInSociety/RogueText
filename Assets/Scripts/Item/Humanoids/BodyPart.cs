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

            for (int i = 0;i < GetChildItemsWithProp.Count;i++)
            {
                TextManager.Add("&a dog&", GetChildItemsWithProp[i]);
                TextManager.AddLink(i, GetChildItemsWithProp.Count);
            }

            TextManager.Add(" on my &dog&", this);
        }*/
    }
}
