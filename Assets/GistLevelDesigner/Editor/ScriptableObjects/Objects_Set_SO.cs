using UnityEngine;
using System;

namespace GistLevelDesigner {
    public class Objects_Set_SO : ScriptableObject {
        public ObjectsSetEntry[] entries;
        
        public void OnValidate() {
            LD_Window.ScriptableObjectUpdated(this);
        }
    }
    
    [Serializable]
    public struct ObjectsSetEntry {
        public string name;
        public GameObject prefab;
    }
}