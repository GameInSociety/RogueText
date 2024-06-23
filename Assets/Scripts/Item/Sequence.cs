using System.Collections.Generic;

[System.Serializable]
public class Sequence {

    public static List<Sequence> sequences = new List<Sequence>();

    public Sequence(string[] triggers, string _seq) {
        this.triggers = triggers;
        content = _seq;
    }

    public string[] triggers;
    public string content;
}
