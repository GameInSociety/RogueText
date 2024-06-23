using System.Collections.Generic;
using UnityEngine;

public class ContentLoader : DataDownloader{
    public static ContentLoader Instance;

    public List<Property> properties_debug = new List<Property>();
    public List<Group> groups_debug = new List<Group>();
    public List<Sequence> sequences_debug = new List<Sequence>();

    public static List<Group> groups = new List<Group>();
    [System.Serializable]
    public class Group {
        public Group(string name) {
            this.name = name;
        }

        public string name;
        public List<string> contents = new List<string>();
    }

    public static Group GetGroup(string name) {
        return groups.Find(x => x.name == name);
    }

    private void Awake() {
        Instance = this;
    }

    public override void Load() {
        groups.Clear();
        Property.sharedProps.Clear();
        base.Load();
    }

    public override void FinishLoading() {
        base.FinishLoading();
        properties_debug = Property.sharedProps;
        groups_debug = groups;
        sequences_debug = Sequence.sequences;
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        if (rowIndex == 0)
            return;

        var group = (Group)null;
        if (!string.IsNullOrEmpty(cells[0])) {
            if (cells[0] == "x") {
                group = groups[groups.Count - 1];
            } else {
                group = new Group(cells[0]);
                groups.Add(group);
            }
            
        }

        for (int i = 1; i < cells.Count; i++) {
            if (string.IsNullOrEmpty(cells[i])) continue;

            if (cells[i].StartsWith('$') || cells[i].StartsWith('E') || cells[i].StartsWith('!')) {
                if (group != null) group.contents.Add(cells[i]);
            } else {
                var dataProp = new Property();
                dataProp.Parse(cells[i].Split('\n'));
                Property.AddPropertyData(dataProp);
                if (group != null) group.contents.Add(cells[i]);
            }
        }
    }
}
