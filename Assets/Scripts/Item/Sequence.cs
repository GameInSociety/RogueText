using System.Collections.Generic;

[System.Serializable]
public class Sequence {

    public static List<Sequence> sequences = new List<Sequence>();

    public Sequence(string _verbs, string _seq) {
        triggers = _verbs.Split(" / ");
        seq = _seq;
    }

    public string seq;
    public string[] triggers;
}
