using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// IMPORTANT :
/// On dirait que CleanProperties d'un côté, et CleanUselessProps, SetPropertyLinks et GetSplitProperty sont redondantes et pourraient être optimiséeS. 


/// Ce quil reste à faire : 
/// 1 ) Supprimer les Propriétés si un group a différents objets.
/// 2 ) Si un groupe d'objet a les memes props, ou si le group ne contient qu'un objet, GARDER LES PROPS.
/// 3 ) Isoler les slots aux propertiés uniques.
/// 4 ) Tester intensivement.
/// 5 ) Commenter.

/// <summary>
/// L'idée pour l'instant:

/// 0 ) Séparer les slots par types d'objet
/// ==> Si les objets sont seuls, détailler la description et aller à étape 2

/// 1 ) Vérifier si toutes les slots ont tous le meme nombre de propriétés
/// ==> Permet d'isoler les slots aux propriétés uniques, éphemeres ou importantes ( Burning house, Dying bird )

/// 2 ) Vérifier les liens entre chacune des propriétés
/// ==> "Shared" : Si une propriété est partagée par toutes les slots, la laisser tranquille, elle apparaitra dans la description
/// ==> "Split" : Si la 1ERE PROP NON SHARED du slot n'est PAS partagée par les slots du group, séparer
/// ==> "No link" : Si les 

/// 3 ) Supprimer les propriétés en trop ( Celle qui n'ont pas été utilisées pour la distinction de la description ) 

/// </summary>

public class Test : MonoBehaviour {
    public TextMeshProUGUI uiText;

    // Tous les Slots, en vrac, tels qu'ils sont envoyés pendnat le jeu
    [SerializeField] private Description _input;

    // Les slots triés et séparées
    [SerializeField] private Description _output;

    /// <summary>
    /// La Description permet de décrire un objet, un groupe d'un meme objet, ou plusieurs objets.
    /// </summary>
    [System.Serializable]
    public class Description {
        public List<SlotGroup> groups = new List<SlotGroup>();
        public string GetText() {
            var txt = "";
            foreach (var item in groups) {
                txt += item.GetText();
            }
            return "";
        }
    }

    /// <summary>
    /// Contient un group de slots regroupé par leur type d'objets ou de propriétés.
    /// </summary>
    [System.Serializable]
    public class SlotGroup {
        /// <summary>
        /// L'id permet de formet les groupe. Il peut être le nom d'un objet ou un type de Property.
        /// </summary>
        public string _id;
        public List<Slot> _slots = new List<Slot>();
        public List<Slot> GetSlots() => _slots;
        public string GetID() => _id;

        public SlotGroup(string id) {
            _id = id;
        }

        public void AddSlot(Slot slot) {
            _slots.Add(slot);
        }
        
        public string GetText() {
            var crotte = $"{GetSlots().Count} {GetSlots().First().GetText()}";
            return $"<color=red>{GetID()}</color>\n{crotte}\n";
        }

        // Renvoie les slots, regroupées par la nature de l'Item.
        public List<SlotGroup> SplitByName() {

            var tmp_SlotGroups = new List<SlotGroup>();

            foreach (var slot in _slots) {
                // Search an existing slot group with matching NAME.
                var slotGroup = tmp_SlotGroups.Find(x => x.GetID() == slot.name);
                if (slotGroup == null) {
                    slotGroup = new SlotGroup(slot.name);
                    tmp_SlotGroups.Add(slotGroup);
                }
                slotGroup.AddSlot(slot);
            }

            return tmp_SlotGroups;
        }

        // Sets if each property's description is shared among other slots, or is unique 
        public void SetPropertyLinks() {
            foreach (var slot in GetSlots()) {
                foreach (var prop in slot.props) {
                    var sharedProps = GetSlots().FindAll(x => x.props.Find(p => p.content == prop.content) != null);
                    // If the shared properties are the same amount of the number of slots in the groups, Shared, otherwise, Unique.
                    prop.state = sharedProps.Count() == GetSlots().Count() ? Slot.Prop.State.Shared : Slot.Prop.State.Unique;
                }
            }
        }

        // Get the property type that will split the slots
        public string GetPropertyForSplit() {
            var splitProperty = GetSlots().First().props.Find(x => x.state == Slot.Prop.State.Unique);
            if (splitProperty == null) {
                Debug.LogError($"No split property");
                return "";
            }
            return splitProperty.type;
        }

        // Create groups for each different description of the target propety.
        public List<SlotGroup> SplitByProperty (string propertyType) {

            var tmp_SlotGroups = new List<SlotGroup>();

            // Check for shared property types in slots
            foreach (var slot in GetSlots()) {
                var refProp = slot.props.Find(x => x.type == propertyType);
                // Find matching SlotGroup for property
                var slotGroup = tmp_SlotGroups.Find(x => refProp.content == x.GetID());
                if (slotGroup == null) {
                    slotGroup = new SlotGroup(refProp.content);
                    tmp_SlotGroups.Add(slotGroup);
                }
                slotGroup.AddSlot(slot);
            }

            return tmp_SlotGroups;

        }

        // Remove all unique properties that are not used in the Property Split 
        public void CleanUselessProperties(string propertyType) {
            foreach (var slot in GetSlots()) {
                slot.props.RemoveAll(x=> x.type != propertyType);
            }
        }

        public void CleanProperties() {
            // No need to clean if only one slot
            if (GetSlots().Count == 1)
                return;
            foreach (var slot in GetSlots()) {
                foreach (var prop in slot.props) {
                    // Check if all the slots have the same 
                    var matchingProps = GetSlots().FindAll(x=>x.props.Find(x=>
                    // Ignore current prop
                    x!=prop &&
                    // Check if same prop content
                    x.content == prop.content &&
                    // Check if same prop type
                    x.type == prop.type
                    ) != null);
                    Debug.Log($"matching props for [{prop.type}.{prop.content}] : {matchingProps.Count}");
                    // If not all the slots (-1, minus current prop) have this prop with this content, remove them
                    if (matchingProps.Count != GetSlots().Count-1)
                        prop.state = Slot.Prop.State.Remove;
                }
                // Remove all marked props
                int count = slot.props.RemoveAll(x => x.state == Slot.Prop.State.Remove);
                Debug.Log($"removed {count} properties on slot {slot.name}");
            }
        }
    }


    [System.Serializable]
    public class Slot {
        public string name;
        public List<Prop> props= new List<Prop>();
        [System.Serializable]
        public class Prop {
            public string type;
            public string content;
            public State state;

            public enum State {
                Shared, // The property is shared with the rest of the group
                Unique, // The property is not present in all of the slots
                Remove, // The slots needs to be removed. ( redondant ? )
            }
        }

        public string GetText() {
            if (props.Count > 0) {
                var prs = "";
                foreach (var prop in props)
                    prs += $" ({prop.content}) ";
                return $"{name} {prs}";
            } else {
                return $"{name}";
            }
        }

    }

    private void Start() {

        //clear all 
        _output.groups.Clear();

        uiText.text = $"<color=green>Input:\n{_input.groups.First().GetText()}</color>\n";

        HandleProps();

        uiText.text += "<color=green>Output:<color=green>\n";
        foreach (var slotGroup in _output.groups) {
            uiText.text += $"{slotGroup.GetText()}\n";
        }
    }

    void HandleProps() {
        // Split the items by name/nature
        var newGroups = _input.groups.First().SplitByName();

        if (newGroups.Count > 1) {
            // Remove all properties that are not share with all the slots
            foreach (var group in newGroups)
                group.CleanProperties();

            _output.groups.AddRange(newGroups);
        } else {
            // First, single out slots with unique properties (w/additional or different types)

            // Then, try to split the group by properties.
            for (int i = 0; i < newGroups.Count; i++) {

                var group = newGroups[i];

                group.SetPropertyLinks();

                var splitProp = group.GetPropertyForSplit();

                if (!string.IsNullOrEmpty(splitProp)) {
                    // Delete all non splited props
                    group.CleanUselessProperties(splitProp);

                    Debug.Log($"Spliting : {splitProp}.");
                    // Split group with target property
                    var splitedGroups = group.SplitByProperty(splitProp);
                    _output.groups.AddRange(splitedGroups);
                } else {
                    Debug.Log($"No distinction needed, no split.");
                    _output.groups.Add(group);
                }
            }
        }

    }

}
