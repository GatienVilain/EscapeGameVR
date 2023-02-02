using UnityEngine;
using UnityEditor;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
using System.Reflection;
using System.Collections.Generic;

namespace GistLevelDesigner {
    public static class Utility {
        public static GameObjectIdentity GetGameObjectIdentity(GameObject gameObject, out bool prefabIdentity, out bool sceneIdentity) {
            prefabIdentity = false;
            sceneIdentity = false;
            
            GameObjectIdentity gameObjectIdentity;
            gameObjectIdentity.identity = Identity.UNDEFINED;
            gameObjectIdentity.instanceID = 0;
            gameObjectIdentity.GUID = null;
            gameObjectIdentity.fileID = 0;
            gameObjectIdentity.rootGUID = null;
            
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null || PrefabUtility.IsPartOfAnyPrefab(gameObject)) {
                prefabIdentity = true;
                gameObjectIdentity.identity = Identity.ASSET;
                var gameObjectOriginal = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
                if (gameObjectOriginal == null && prefabStage != null) {
                    gameObjectIdentity.GUID = AssetDatabase.AssetPathToGUID(prefabStage.assetPath);
                    gameObjectIdentity.fileID = Utility.LocalFileIDfromReflection(gameObject);
                } else {
                    if (gameObjectOriginal != null) {
                        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(gameObjectOriginal, out string selectionOriginalGUID, out long selectionOriginalFileID)) {
                            gameObjectIdentity.GUID = selectionOriginalGUID;
                            gameObjectIdentity.fileID = selectionOriginalFileID;
                        }
                    } else {
                        gameObjectIdentity.identity = Identity.UNDEFINED;
                    }
                }
            } else {
                sceneIdentity = true;
                long fileID = Utility.LocalFileIDfromReflection(gameObject);
                if (fileID == 0) {
                    gameObjectIdentity.identity = Identity.INSTANCE;
                    gameObjectIdentity.instanceID = gameObject.GetInstanceID();
                } else {
                    gameObjectIdentity.identity = Identity.ASSET;
                    gameObjectIdentity.fileID = fileID;
                }
                gameObjectIdentity.GUID = AssetDatabase.AssetPathToGUID(gameObject.scene.path);
            }
            
            return gameObjectIdentity;
        }
        public static long LocalFileIDfromReflection(GameObject gameObject) {
            SerializedObject selectionSerialized = new SerializedObject(gameObject);
            PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            if (inspectorModeInfo != null) {
                inspectorModeInfo.SetValue(selectionSerialized, InspectorMode.Debug, null);
                SerializedProperty fileIDproperty = selectionSerialized.FindProperty("m_LocalIdentfierInFile");
                return fileIDproperty.longValue;
            }
            return 0;
        }
        public static void DestroyAddedOverridesInside(GameObject gameObject) {
            for (int i = 0; i < gameObject.transform.childCount; i++) {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                if (PrefabUtility.IsAddedGameObjectOverride(child)) {
                    GameObject.DestroyImmediate(child, false);
                    i--;
                } else {
                    DestroyAddedOverridesInside(child);
                }
            }
        }
        public static void RemoveChildChildGameObjectsFromPrefab(GameObject childGameObject, List<GameObject> deletingChildChildGameObjects, string prefabPath) {
            if (deletingChildChildGameObjects == null || deletingChildChildGameObjects.Count == 0) return;
            
            bool childIsOverride = PrefabUtility.IsAddedGameObjectOverride(childGameObject);
            if (childIsOverride) {
                for (int i = 0; i < deletingChildChildGameObjects.Count; i++) {
                    GameObject.DestroyImmediate(deletingChildChildGameObjects[i], false);
                }
                return;
            }
            
            var prefabSavedObjects = new List<GameObject>();
            for (int i = 0; i < deletingChildChildGameObjects.Count; i++) {
                var deletingChild = deletingChildChildGameObjects[i];
                if (PrefabUtility.IsAddedGameObjectOverride(deletingChild)) {
                    GameObject.DestroyImmediate(deletingChild, false);
                } else {
                    Utility.DestroyAddedOverridesInside(deletingChild);
                    prefabSavedObjects.Add(deletingChild);
                }
            }
            if (prefabSavedObjects.Count > 0) {
                prefabSavedObjects.Sort((go1,go2) => go1.transform.GetSiblingIndex().CompareTo(go2.transform.GetSiblingIndex()));
                LD_Window.SaveCall();
                var rootGO = PrefabUtility.LoadPrefabContents(prefabPath);
                var floorGO = rootGO.transform.GetChild(childGameObject.transform.GetSiblingIndex());
                for (int i = prefabSavedObjects.Count - 1; i >= 0 ; i--) {
                    GameObject.DestroyImmediate(floorGO.GetChild(prefabSavedObjects[i].transform.GetSiblingIndex()).gameObject, false);
                }
                PrefabUtility.SaveAsPrefabAsset(rootGO, prefabPath);
                PrefabUtility.UnloadPrefabContents(rootGO);
                LD_Window.SetLevelDataNotActual();
            }
        }
        public static Dictionary<long,GameObject> GetChildrenListWithFileIDs(GameObject gameObject, string rootPrefabPath) {
            Dictionary<long,GameObject> childrenList = new Dictionary<long,GameObject>(gameObject.transform.childCount);
            for (int i = 0; i < gameObject.transform.childCount; i++) {
                var child = gameObject.transform.GetChild(i).gameObject;
                var childOriginal = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(child, rootPrefabPath);
                if (childOriginal != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(childOriginal, out string _, out long fileID)) {
                    childrenList.Add(fileID, child);
                }
            }
            return childrenList;
        }
        public static bool TwoDArraysSequenceEqual2<T>(T[,] arr1, T[,] arr2) {
            if (arr1 == arr2) return true;
            if (arr1 == null || arr2 == null) return false;
            int arr1rows = arr1.GetLength(0);
            int arr1cols = arr1.GetLength(1);
            if (arr1rows != arr2.GetLength(0) || arr1cols != arr2.GetLength(1)) return false;
            for (int i = 0; i < arr1rows; i++) {
                for (int j = 0; j < arr1cols; j++) if (!arr1[i,j].Equals(arr2[i,j])) return false;
            }
            return false;
        }
        public static Vector3 NearestPointOnLine(Vector3 origin, Vector3 direction, Vector3 point) {
            direction.Normalize();
            Vector3 lhs = point - origin;

            float dotP = Vector3.Dot(lhs, direction);
            return origin + direction * dotP;
        }    
        public static bool ValueTypeSequenceEqual<T>(T[] a, T[] b) where T : struct {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (!a[i].Equals(b[i])) return false;
            return true;
        }
        public static bool ManagedObjectDataArrayEquals<T>(T[] a, T[] b) where T : ManagedObjectData<T> {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }
    }
}