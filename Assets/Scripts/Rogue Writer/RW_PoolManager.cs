using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class RW_PoolManager : MonoBehaviour {

    public static RW_PoolManager Instance;

    [SerializeField] private Transform m_parent;

    // Pools
    public List<RW_Pool> pools = new List<RW_Pool>();

    void Awake() {
        Instance = this;
    }

    private void Start() {
        foreach (var pool in pools) {
            if (pool.parent != null)
                continue;
            pool.parent = new GameObject().transform;
            pool.parent.name = $"[{pool.id}]";
            pool.parent.SetParent(m_parent);
        }   
    }

    public int GetCount(string id) {
        var pool = pools.Find(x => x.id == id);
        if (pool == null) {
            Debug.LogError($"no pool named ({id})");
            return -1;
        }
        return pool.count;
    }

    public Displayable Pull(string id, Transform parent = null) {
        var pool = pools.Find(x=>x.id == id);
        if (pool == null) {
            Debug.LogError($"no pool named ({id})");
            return null;
        }

        if (pool.stack.Count == 0) {
            ++pool.count;
            var newDisplayable = Instantiate(pool.prefab, parent == null ? pool.parent : parent);
            pool.active.Add(newDisplayable);
            return newDisplayable;
        }

        var displayable = pool.stack.First();
        pool.stack.RemoveAt(0);
        pool.active.Add(displayable);
        return displayable;
    }

    public void Push(string id, Displayable displayable) {
        var pool = pools.Find(x => x.id == id);
        if (pool == null) {
            Debug.LogError($"no pool named ({id})");
            return;
        }

        pool.active.Remove(displayable);
        pool.stack.Add(displayable);
    }
}