using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
 public class Group{
        public Group (int index, Verb verb){
            this.index = index;
            this.verb = verb;
            itemGroups = new List<ItemGroup>();
            input = "";
        }

        public string input;
        public int index;
        public Verb verb;

        /// <summary>
        ///  ITEMS
        /// </summary>
        public List<ItemGroup> itemGroups;
        public struct ItemGroup{
            public ItemGroup(int index, List<Item> itms){
                this.index = 0;
                this.itms = itms;
            }

            public int index;
            public List<Item> itms;
        }

        public void tryFetchItems (){

            if ( referencingPreviousItems() ) {
                // try fetch previous itms
                Debug.Log("trying to fetch previous parsed itms");
                return;
            }

            var itms = getItemsFromText(input);
            if (itms.Count == 0){
                var verbItem = Item.GetDataItem("no item");
                if (verb.HasCell(verbItem)) {
                    itms.Add(verbItem);
                    Debug.Log($"found general universal for verb {verb.getWord}");
                } else {
                    Debug.Log($"no items were found for {input}");
                }
                return;
            }

            // try to see if next 
        }

        public bool referencingPreviousItems(){
            return Regex.IsMatch(input, @$"\bthem\b") || Regex.IsMatch(input, @$"\bit\b");
        }

        List<Item> getItemsFromText (string text) {
            var itms = new List<Item>();
            itms.AddRange(AvailableItems.Get.list.FindAll(x => x.getIndexInText(text, Word.Number.Singular) >= 0));
            itms.AddRange(AvailableItems.Get.list.FindAll(x => x.getIndexInText(text, Word.Number.Plural) >= 0));
            return itms.OrderByDescending(x => x.indexInInput).ToList();
        }

        
    }


/*
void createGroups() {
        Debug.Log("input : " + mainInput);

        verbs = Verb.verbs.FindAll(x => x.getIndexInText(mainInput) >= 0);

        foreach(var verb in verbs){
            int[] indexes = verb.getIndexesInText(mainInput);
            foreach (var i in indexes){
                // if two words share the same index, it's a longer/shorter homonym
                // ( look at / look ) pick longest.
                // ça a pas l'air de servir à grand chose
                var group = groups.Find(x=> x.index == i);
                if ( group.verb != null){
                    if (verb.getWord.Length > group.verb.getWord.Length)
                        group.verb = verb;
                    continue;
                }

                groups.Add(new Group(i, verb));
            }
        }
        
        groups = groups.OrderBy(x => x.index).ToList();

        for(int i = 0; i < groups.Count; i++){
            Group group = groups[i];
            if (i == groups.Count - 1){
                group.input = mainInput.Remove(0, groups[i].index);
            } else {
                Group nverb = groups[i+1];
                if (i == 0)
                    group.input = mainInput.Remove(nverb.index);
                else
                    group.input = mainInput.Remove(nverb.index).Remove(0, groups[i].index);
            }
        }

        AvailableItems.update();

        for(int i = 0; i < groups.Count; i++) {

            Group group = groups[i];
            Debug.Log($"fetching itms in group {group.input}");
            group.tryFetchItems();
            
        }

    }
    */