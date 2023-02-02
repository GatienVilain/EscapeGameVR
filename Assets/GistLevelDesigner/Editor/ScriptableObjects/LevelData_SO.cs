using System;
using UnityEngine;
using UnityEditor;

namespace GistLevelDesigner {
    public class LevelData_SO : ScriptableObject {
        [SerializeField]
        private LevelData levelData;

        public           LevelData_SO() {
            levelData = new LevelData();
        }
        public LevelData LevelData() => levelData;
        public LevelData LevelDataClone() => levelData.Clone();
        public void      CopySettingsFrom(LevelData_SO other) {
            levelData.mainSet = other.levelData.mainSet;
            levelData.objectsSet = other.levelData.objectsSet;
            levelData.optionsSettings = (OptionsSettings)other.levelData.optionsSettings.Clone();
            levelData.visibilitySettings = (VisibilitySettings)other.levelData.visibilitySettings.Clone();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        public void      UpdateData(LevelData levelData) {
            this.levelData = levelData.Clone();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            OnValidate();
        }
        public void      OnValidate() {
            LD_Window.ScriptableObjectUpdated(this);
        }
        public void      SetPrefabGUID(string prefabGUID) {
            levelData.prefabGUID = prefabGUID;
        }
        public string    GetRootPrefabGUID() {
            return levelData.prefabGUID;
        }
    }

    [CustomEditor(typeof(LevelData_SO))]
    public class LevelData_SO_Editor : Editor {
        SerializedProperty   levelData;
        SerializedProperty   mainSet;
        SerializedProperty   objectsSet;
        SerializedProperty   enemiesSet;
        SerializedProperty   optionsSettings;
        SerializedProperty   moveSnap;
        SerializedProperty   rotationSnap;
        SerializedProperty   wallsManipulatorsHeight;
        SerializedProperty   hoverResponsiveHandles;
        SerializedProperty   visibilitySettings;
        SerializedProperty   movement;
        SerializedProperty   rotation;
        SerializedProperty   walls;
        SerializedProperty   wallsMovement;
        SerializedProperty   wallsRotation;
        SerializedProperty   wings;
        SerializedProperty   wingsGrowth;
        SerializedProperty   wingsLines;
        SerializedProperty   objects;
        SerializedProperty   floors;
        SerializedProperty   hidden;

        private bool modified;
        private bool useFoldoutMarginFix = true;
        
        private static readonly GUIContent emptyContent = new GUIContent();
        private static float guiCalcHeight;
        private static GUIStyle foldoutStyle;
        
        void                 OnEnable() {
            levelData = serializedObject.FindProperty(nameof(levelData));
            
            mainSet            = levelData.FindPropertyRelative(nameof(mainSet));
            objectsSet         = levelData.FindPropertyRelative(nameof(objectsSet));
            enemiesSet         = levelData.FindPropertyRelative(nameof(enemiesSet));
            
            optionsSettings         = levelData.FindPropertyRelative(nameof(optionsSettings));
            moveSnap                = optionsSettings.FindPropertyRelative(nameof(moveSnap));
            rotationSnap            = optionsSettings.FindPropertyRelative(nameof(rotationSnap));
            wallsManipulatorsHeight = optionsSettings.FindPropertyRelative(nameof(wallsManipulatorsHeight));
            hoverResponsiveHandles  = optionsSettings.FindPropertyRelative(nameof(hoverResponsiveHandles));
            
            visibilitySettings = levelData.FindPropertyRelative(nameof(visibilitySettings));
            movement           = visibilitySettings.FindPropertyRelative(nameof(movement));
            rotation           = visibilitySettings.FindPropertyRelative(nameof(rotation));
            walls              = visibilitySettings.FindPropertyRelative(nameof(walls));
            wallsMovement      = visibilitySettings.FindPropertyRelative(nameof(wallsMovement));
            wallsRotation      = visibilitySettings.FindPropertyRelative(nameof(wallsRotation));
            wings              = visibilitySettings.FindPropertyRelative(nameof(wings));
            wingsGrowth        = visibilitySettings.FindPropertyRelative(nameof(wingsGrowth));
            wingsLines         = visibilitySettings.FindPropertyRelative(nameof(wingsLines));
            objects            = visibilitySettings.FindPropertyRelative(nameof(objects));
            floors             = visibilitySettings.FindPropertyRelative(nameof(floors));
            hidden             = visibilitySettings.FindPropertyRelative(nameof(hidden));
        }
        public bool          DataField() {
            OnInspectorGUI();
            return modified;
        }
        public float         GetCalculatedHeight() {
            return guiCalcHeight;
        }
        public override void OnInspectorGUI() {
            if (serializedObject == null || serializedObject.targetObject == null) return;
            
            if (useFoldoutMarginFix) foldoutStyle = Styles.foldStylem12marginFixed;
            else foldoutStyle = Styles.foldStylem12;
            
            serializedObject.Update();
            
            guiCalcHeight = 0;
            
            EditorGUILayout.BeginVertical();
            UnityEngine.Object newRef;
            
            // Sets
            if (Foldout(ref Prefs.Foldouts.sets)) {
                // Walls Set
                EditorGUILayout.BeginHorizontal();
                // prevRef = mainSet.objectReferenceValue;
                GUILayout.Space(28);
                EditorGUILayout.PropertyField(mainSet, emptyContent);
                newRef = mainSet.objectReferenceValue;
                if (newRef == null && GUILayout.Button("new", Styles.newSObutton)) {
                    string path = EditorUtility.SaveFilePanelInProject("New LevelDesign Main Set", "LD_Main_Set", "asset", "New Main Set asset file path");
                    if (!string.IsNullOrEmpty(path)){
                        var newMainSet = ScriptableObject.CreateInstance<Main_Set_SO>();
                        AssetDatabase.CreateAsset(newMainSet, path);
                        mainSet.objectReferenceValue = newMainSet;
                    }
                }
                EditorGUILayout.EndHorizontal();
                guiCalcHeight += Styles.newSObutton.fixedHeight + 2;
                
                // Objects Set
                EditorGUILayout.BeginHorizontal();
                // prevRef = objectsSet.objectReferenceValue;
                GUILayout.Space(28);
                EditorGUILayout.PropertyField(objectsSet, emptyContent);
                newRef = objectsSet.objectReferenceValue;
                if (newRef == null && GUILayout.Button("new", Styles.newSObutton)) {
                    string path = EditorUtility.SaveFilePanelInProject("New LevelDesign Objects Set", "LD_Objects_Set", "asset", "New Objects Set asset file path");
                    if (!string.IsNullOrEmpty(path)){
                        var newObjectsSet = ScriptableObject.CreateInstance<Objects_Set_SO>();
                        AssetDatabase.CreateAsset(newObjectsSet, path);
                        objectsSet.objectReferenceValue = newObjectsSet;
                    }
                }
                EditorGUILayout.EndHorizontal();
                guiCalcHeight += Styles.newSObutton.fixedHeight + 2;
                guiCalcHeight += 2;
            }
            
            // Options
            if (Foldout(ref Prefs.Foldouts.options)) {
                guiCalcHeight += 2;
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Move snap", Styles.settingsOptionsLabel);
                moveSnap.floatValue = EditorGUILayout.FloatField(moveSnap.floatValue, Styles.settingsOptionsValue);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                guiCalcHeight += 20;
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Rotation snap", Styles.settingsOptionsLabel);
                rotationSnap.floatValue = EditorGUILayout.FloatField(rotationSnap.floatValue, Styles.settingsOptionsValue);
                EditorGUILayout.EndHorizontal();
                guiCalcHeight += 20;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Walls manipulators height", Styles.settingsOptionsLabel);
                wallsManipulatorsHeight.floatValue = EditorGUILayout.FloatField(wallsManipulatorsHeight.floatValue, Styles.settingsOptionsValue);
                EditorGUILayout.EndHorizontal();
                guiCalcHeight += 20;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Hover responsive handles", Styles.settingsOptionsLabel);
                hoverResponsiveHandles.boolValue = EditorGUILayout.Toggle(hoverResponsiveHandles.boolValue);
                EditorGUILayout.EndHorizontal();
                guiCalcHeight += 20;
            }
            
            // Visibility
            if (Foldout(ref Prefs.Foldouts.visibility)) {
                movement.boolValue      = GUILayout.Toggle(movement.boolValue, new GUIContent("Movement"), Styles.toggleStylem22);
                rotation.boolValue      = GUILayout.Toggle(rotation.boolValue, new GUIContent("Rotation"), Styles.toggleStylem22);
                walls.boolValue         = GUILayout.Toggle(walls.boolValue, new GUIContent("Walls"), Styles.toggleStylem22);
                wallsMovement.boolValue = GUILayout.Toggle(wallsMovement.boolValue, new GUIContent("Movement"), Styles.toggleStylem39);
                wallsRotation.boolValue = GUILayout.Toggle(wallsRotation.boolValue, new GUIContent("Rotation"), Styles.toggleStylem39);
                wings.boolValue         = GUILayout.Toggle(wings.boolValue, new GUIContent("Wings"), Styles.toggleStylem39);
                wingsGrowth.boolValue   = GUILayout.Toggle(wingsGrowth.boolValue, new GUIContent("Growth"), Styles.toggleStylem56);
                wingsLines.boolValue    = GUILayout.Toggle(wingsLines.boolValue, new GUIContent("Lines"), Styles.toggleStylem56);
                objects.boolValue       = GUILayout.Toggle(objects.boolValue, new GUIContent("Objects"), Styles.toggleStylem22);
                floors.boolValue        = GUILayout.Toggle(floors.boolValue, new GUIContent("Floors"), Styles.toggleStylem22);
                // hidden.boolValue        = GUILayout.Toggle(hidden.boolValue, new GUIContent("Show hidden"), Styles.toggleStylem22);
                
                guiCalcHeight += 10 * (Styles.toggleStylem22.lineHeight + Styles.toggleStylem22.margin.top);
            }
            
            GUILayout.Space(10);
            guiCalcHeight += 10;
            
            EditorGUILayout.EndVertical();
            modified = serializedObject.hasModifiedProperties;
            serializedObject.ApplyModifiedProperties();
        }
        
        private bool Foldout(ref Prefs.Foldouts.Foldout foldout) {
            var foldValue = EditorGUILayout.BeginFoldoutHeaderGroup(foldout.value, foldout.name, foldoutStyle);
            if (foldValue != foldout.value) foldout.SetValue(foldValue);
            EditorGUILayout.EndFoldoutHeaderGroup();
            guiCalcHeight += Styles.foldHeight;
            return foldValue;
        }
        
        public void UseFoldoutMarginFix(bool use) {
            useFoldoutMarginFix = use;
        }
        public override bool UseDefaultMargins() {
            return false;
        }
    }


    [Serializable]
    public class LevelData : ManagedObjectData<LevelData> {
        public string             prefabGUID;
        public Main_Set_SO        mainSet;
        public Objects_Set_SO     objectsSet;
        public OptionsSettings    optionsSettings = new OptionsSettings{
            moveSnap = 1f,
            rotationSnap = 22.5f,
            wallsManipulatorsHeight = 1f,
            hoverResponsiveHandles = true
        };
        public VisibilitySettings visibilitySettings = new VisibilitySettings{
            movement = true,
            rotation = true,
            walls = true,
            wallsMovement = true,
            wallsRotation = true,
            wings = true,
            wingsGrowth = true,
            wingsLines = true,
            objects = true,
            floors = true,
            hidden = false
        };
        public WallData[]         wallsData;
        public SimpleObjectData[] simpleObjectsData;
        public FloorData[]        floorsData;
        
        public LevelData() {}
        public LevelData(LevelData other) {
            this.prefabGUID = other.prefabGUID;
            this.mainSet = other.mainSet;
            this.objectsSet = other.objectsSet;
            this.optionsSettings = (OptionsSettings)other.optionsSettings.Clone();
            this.visibilitySettings = (VisibilitySettings)other.visibilitySettings.Clone();
            if (other.wallsData == null) {
                this.wallsData = null;
            } else {
                this.wallsData = new WallData[other.wallsData.Length];
                for (int i = 0; i < other.wallsData.Length; i++) this.wallsData[i] = other.wallsData[i].Clone();
            }
            if (other.simpleObjectsData == null) {
                this.simpleObjectsData = null;
            } else {
                this.simpleObjectsData = new SimpleObjectData[other.simpleObjectsData.Length];
                for (int i = 0; i < other.simpleObjectsData.Length; i++) this.simpleObjectsData[i] = other.simpleObjectsData[i].Clone();
            }
            
            if (other.floorsData == null) {
                this.floorsData = null;
            } else {
                this.floorsData = new FloorData[other.floorsData.Length];
                for (int i = 0; i < other.floorsData.Length; i++) this.floorsData[i] = other.floorsData[i].Clone();
            }
        }
        public override LevelData Clone() {
            return new LevelData(this);
        }
        public override bool Equals(LevelData other) {
            if (!prefabGUID.Equals(other.prefabGUID)) return false;
            
            if (!ReferenceEquals(mainSet, other.mainSet)) return false;
            if (!ReferenceEquals(objectsSet, other.objectsSet)) return false;
            if (!optionsSettings.Equals(other.optionsSettings)) return false;
            if (!visibilitySettings.Equals(other.visibilitySettings)) return false;
            
            if (!Utility.ManagedObjectDataArrayEquals(wallsData, other.wallsData)) return false;
            if (!Utility.ManagedObjectDataArrayEquals(simpleObjectsData, other.simpleObjectsData)) return false;
            if (!Utility.ManagedObjectDataArrayEquals(floorsData, other.floorsData)) return false;
            
            return true;
        }
    }
}