using System.Collections.Generic;

[System.Serializable]
public class ItemGroup {
    public ItemGroup(string key, int dataIndex) {
        this.key = key;
        this.dataIndex = dataIndex;
    }

    public string key;
    public int dataIndex;


}