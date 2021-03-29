using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjectiveLoader : MonoBehaviour {

    public string[] maleTerminaisons;
    public string[] femaleTerminaisons;

    public static AdjectiveLoader Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadAdjectives()
    {
		TextAsset textAsset = Resources.Load ("Adjectives") as TextAsset;

		for (int i = 0; i < (int)Adjective.Type.Any; i++) {

			Adjective.adjectives.Add (new List<Adjective> ());

		}
		string[] rows = textAsset.text.Split ('\n');

		for (int rowIndex = 2; rowIndex < rows.Length-1; rowIndex++) {

			string row = rows [rowIndex];
			row = row.TrimEnd ('\r', '\n');

			string[] cells = row.Split (';');

			Adjective.Type adjType = Adjective.Type.Rural;
			foreach (var cell in cells) {

				if (cell.Length > 1) {
				
					Adjective newAdjective = new Adjective ();

					newAdjective._text = cells [(int)adjType];
                    if (newAdjective._text.Contains("("))
                    {
                        newAdjective.beforeWord = true;
                        newAdjective._text = newAdjective._text.Replace("(", "");
                    }

					Adjective.adjectives [(int)adjType].Add (newAdjective);

//					Debug.LogError ("adding adjective : " + newAdjective.GetName(Word.Genre.Masculine, Word.Number.Singular) );


				} else {
//					Debug.LogError ("cell trop petite : " + cell);
				}

				++adjType;

			}


		}

	}
}
