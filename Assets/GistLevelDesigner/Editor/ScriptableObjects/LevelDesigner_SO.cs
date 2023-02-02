using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace GistLevelDesigner {
    public class LevelDesigner_SO : ScriptableObject {
        [SerializeField]
        private GameObjectIdentity[] managed;
        [NonSerialized]
        private static bool oneTimeCleaning;
        
        public LevelDesigner_SO() {
            managed = new GameObjectIdentity[0];
        }
        public void OneTimeCleaning() {
            if (!oneTimeCleaning) {
                List<GameObjectIdentity> cleaned = new List<GameObjectIdentity>(managed.Length);
                for (int i = 0; i < managed.Length; i++) {
                    GameObjectIdentity gameObjectIdentity = managed[i];
                    if (gameObjectIdentity.identity != Identity.UNDEFINED && gameObjectIdentity.identity != Identity.INSTANCE) {
                        if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GUIDToAssetPath(gameObjectIdentity.rootGUID)) != null) {
                            cleaned.Add(gameObjectIdentity);
                        }
                    }
                }
                if (cleaned.Count != managed.Length) {
                    managed = cleaned.ToArray();
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }
                oneTimeCleaning = true;
            }
        }
        public void Set(GameObjectIdentity gameObjectIdentity) {
            if (string.IsNullOrEmpty(gameObjectIdentity.rootGUID)) return;
            
            if (gameObjectIdentity.identity == Identity.INSTANCE) {
                for (int i = 0; i < managed.Length; i++) {
                    if (managed[i].instanceID == gameObjectIdentity.instanceID) {
                        managed[i] = gameObjectIdentity;
                        return;
                    }
                }
                ArrayUtility.Add<GameObjectIdentity>(ref managed, gameObjectIdentity);
                return;
            }
            if (gameObjectIdentity.identity == Identity.ASSET) {
                string GUID = gameObjectIdentity.GUID;
                long fileID = gameObjectIdentity.fileID;
                for (int i = 0; i < managed.Length; i++) {
                    if (managed[i].fileID == fileID && managed[i].GUID == GUID) {
                        managed[i] = gameObjectIdentity;
                        EditorUtility.SetDirty(this);
                        AssetDatabase.SaveAssets();
                        return;
                    }
                }
                ArrayUtility.Add<GameObjectIdentity>(ref managed, gameObjectIdentity);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                return;
            }
        }
        public bool TryGetRootGUID(GameObjectIdentity gameObjectIdentity, out string rootGUID) {
            rootGUID = null;
            if (gameObjectIdentity.identity == Identity.INSTANCE) {
                int instanceID = gameObjectIdentity.instanceID;
                for (int i = 0; i < managed.Length; i++) {
                    if (managed[i].instanceID == instanceID) {
                        rootGUID = managed[i].rootGUID;
                        return true;
                    }
                }
                return false;
            }
            if (gameObjectIdentity.identity == Identity.ASSET) {
                string GUID = gameObjectIdentity.GUID;
                long fileID = gameObjectIdentity.fileID;
                for (int i = 0; i < managed.Length; i++) {
                    if (managed[i].fileID == fileID && managed[i].GUID == GUID) {
                        rootGUID = managed[i].rootGUID;
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        public bool TryConnectAny(GameObjectIdentity gameObjectIdentity, List<string> possibleRootGUIDS, string[] existingRootGUIDS) {
            for (int i = 0; i < possibleRootGUIDS.Count; i++) {
                string possibleRootGUID = possibleRootGUIDS[i];
                for (int j = 0; j < existingRootGUIDS.Length; j++) {
                    if (existingRootGUIDS[j] == possibleRootGUID) {
                        gameObjectIdentity.rootGUID = possibleRootGUID;
                        Set(gameObjectIdentity);
                        return true;
                    }
                }
            }
            return false;
        }
   }
   
   [CustomEditor(typeof(LevelDesigner_SO))]
   public class LevelDesigner_SO_Editor : Editor {
       public override void OnInspectorGUI() {
           EditorGUILayout.LabelField(Prefs.levelDesignerPackageName + " " + Prefs.version);
           base.OnInspectorGUI();
       }
   }
}
