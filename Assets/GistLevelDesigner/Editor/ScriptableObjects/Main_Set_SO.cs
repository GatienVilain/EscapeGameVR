using UnityEngine;
using System;

namespace GistLevelDesigner {
    public class Main_Set_SO : ScriptableObject {
        public float        wallLength = 1f;
        public GameObject   wall_I;
        public GameObject   wall_L;
        public GameObject   wall_T;
        public GameObject   wall_X;
        public GameObject[] wall_Caps;
        public FloorEntry[] floors;
        
        public void OnValidate() {
            if (floors != null) {
                for (int i = 0; i < floors.Length; i++) {
                    if (floors[i].size <= 0f) floors[i].size = 1f;
                }
            }
            LD_Window.ScriptableObjectUpdated(this);
        }
    }
    [Serializable]
    public struct FloorEntry {
        public GameObject floorPrefab;
        public float      size;
    }
}