using System.Collections.Generic;

[System.Serializable]
public class Sequence {

    public static List<Sequence> sequences = new List<Sequence>();

    public Sequence(string _verbs, string _seq) {
        triggers = _verbs.Split(" / ");
        _name = triggers[0];
        seq = _seq;
    }

    public string _name = "";
    public string[] triggers;
    public string seq;
}
