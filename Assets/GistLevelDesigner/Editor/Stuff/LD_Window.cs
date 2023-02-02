#region USING 
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
#if !UNITY_2021_2_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System;
#endregion
namespace GistLevelDesigner {
    public class LD_Window : EditorWindow {
    #region SETTINGS 
        static bool undoCreatedObjectsOn = false;
        const  int  buttonsSizeMax       = 100;
        const  int  buttonsPerRowMin     = 4;
    #endregion
    #region FIELDS 
        private static LD_Window window;
        private enum   SelectionStatus {NONE, SUITABLE, MANAGED}
        private bool                 locked;
        private float                guiCalcHeight;
        private bool                 verticalScrollOn;
        private Vector2              scrollPos;
        private Vector3              prevCameraPosition;
        private UnityEngine.Object[] loadingAssetsPreviews;
        private bool                 nullPreviewsRepaint;
        private int                  nullPreviewsAttempts;
        private DateTime             nullPreviewsLastAttemptTime;
        private DateTime             nullIconsPreviewsLastAttemptTime;
        private GUIContent[]         wallsGUIcontents;
        private GUIContent[]         objectsGUIcontents;
        private ObjectsSetEntry[]    actualObjectsEntries;
        private GUIContent[]         floorsGUIcontents;
        private FloorEntry[]         actualFloorsEntries;
        
        private LevelDesigner_SO    SO;
        private GameObject          selection;
        private bool                selectionActive;
        private SelectionStatus     selectionStatus;
        private bool                deferredSave;
        private string              deferredSaveRootPrefabPath;
        private GameObject          root;
        private string              rootPrefab_GUID;
        private LevelData_SO        levelData_SO;
        private LevelData_SO_Editor levelData_SO_Editor;
        private LevelData           levelData;
        private bool                levelDataNotActual;
        private bool                lastHasFocus;
        private bool                undoRedoPerformedSubscribed;
        
        private List<SceneObject>  objects;
        private SceneObject[]      sortedObjects;
        private List<Wall>         walls;
        private List<SimpleObject> simpleObjects;
        private List<Floor>        floors;
    #endregion
    #region WINDOW 
        [MenuItem(Prefs.levelDesignerMenuPath)]
        public  static void OpenWindow() {
            window = (LD_Window)EditorWindow.GetWindow(typeof(LD_Window));
            window.titleContent = new GUIContent(Prefs.levelDesignerWindowTitle);
            window.minSize = new Vector2(10, 1);
            window.Show();
            window.Focus();
        }
        public  static void OpenWindowOnce() {
            if (window == null) OpenWindow();
        }
        private void        OnEnable() {
            window = this;
            Flush();
            CheckSO();
            OnSelectionChange();
            SceneView.duringSceneGui += OnSceneGUI;
            EditorSceneManager.sceneClosing += OnBeforeSceneClose;
            EditorSceneManager.sceneSaving += OnBeforeSceneSave;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.wantsToQuit += OnEditorWantsToQuit;
            EditorApplication.update += EditorUpdate;
            EditorApplication.update += IconsLoader;
        }
        private void        OnDisable() {
            EditorApplication.update -= IconsLoader;
            EditorApplication.update -= EditorUpdate;
            EditorApplication.wantsToQuit -= OnEditorWantsToQuit;
            EditorSceneManager.sceneSaving -= OnBeforeSceneSave;
            EditorSceneManager.sceneClosing -= OnBeforeSceneClose;
            SceneView.duringSceneGui -= OnSceneGUI;
            Save();
            Flush();
        }
        private void        OnInspectorUpdate() {
            if (selection != null) {
                if (selectionActive) {
                    if (!selection.activeInHierarchy) {
                        HierarchyGameObjectDeselected();
                        Repaint();
                    }
                } else {
                    if (selection.activeInHierarchy) {
                        HierarchyActiveGameObjectSelected();
                        Repaint();
                    }
                }
            }
        }
        private void        CheckForSelectionChange() {
            if (selection != GetSelectionGameObject()) OnSelectionChange();
        }
        private void        OnSelectionChange() {
            if (locked) return;
            
            selection = GetSelectionGameObject();
            if (selection != null) {
                if (selectionActive) HierarchyGameObjectDeselected();
                if (selection.activeInHierarchy) HierarchyActiveGameObjectSelected();
            } else {
                if (selectionActive) HierarchyGameObjectDeselected();
            }
            Repaint();
        }
        private void        Update() {
            if (nullPreviewsRepaint) NullPreviewRepaint();
            if (deferredSave) Save();
            if (loadingAssetsPreviews.Length > 0) CheckLoadingPreviews();
        }
        private void        EditorUpdate() {
            if (this.hasFocus != lastHasFocus) {
                lastHasFocus = this.hasFocus;
                SceneView.RepaintAll();
            }
        }
        private void        IconsLoader() {
            var currentTime = DateTime.UtcNow;
            var nullIconsPreviewRepaintPause = currentTime - nullIconsPreviewsLastAttemptTime;
            if (nullIconsPreviewRepaintPause.TotalSeconds > 1) {
                if (LoadIcons()) {
                    EditorApplication.update -= IconsLoader;
                    Repaint();
                }
                nullIconsPreviewsLastAttemptTime = currentTime;
            }
        }
        private void OnFocus() {
            CheckForSelectionChange();
        }
        public  static void ScriptableObjectUpdated(ScriptableObject updated_SO) {
            if (window != null && window.selectionStatus == SelectionStatus.MANAGED) {
                if (updated_SO == window.levelData.mainSet) {
                    window.floorsGUIcontents = null;
                    window.Repaint();
                } else if (updated_SO == window.levelData.objectsSet) {
                    window.objectsGUIcontents = null;
                    window.Repaint();
                } else if (updated_SO == window.levelData_SO) {
                    window.OnLevelData_SO_Update();
                }
            }
        }
    #endregion
    #region GUI 
        private void OnGUI() {
            if (Application.isPlaying) return;
            if (selectionStatus == SelectionStatus.SUITABLE) {CreatePickupGUI(); return;}
            if (selectionStatus != SelectionStatus.MANAGED) return;
            
            const int heightScrollFix = 21;
            int screenHeight = (int)(Screen.height / EditorGUIUtility.pixelsPerPoint);
            if (Event.current.type == EventType.Layout) verticalScrollOn = guiCalcHeight > screenHeight - heightScrollFix;
            if (Event.current.type == EventType.Repaint) {
                if (verticalScrollOn != guiCalcHeight > screenHeight - heightScrollFix) {
                    Repaint();
                    return;
                }
            }
            guiCalcHeight = 0;
            
            if (verticalScrollOn) {
                int screenWidth = (int)(Screen.width / EditorGUIUtility.pixelsPerPoint);
                scrollPos = GUILayout.BeginScrollView(
                    scrollPos, false, true, GUILayout.Width(screenWidth), GUILayout.Height(screenHeight - heightScrollFix)
                );
            } else if (scrollPos != Vector2.zero) scrollPos = Vector2.zero;

            // WALLS
            if (levelData.mainSet != null && (levelData.mainSet.wall_I != null || levelData.mainSet.wall_L != null || levelData.mainSet.wall_T != null || levelData.mainSet.wall_X != null)) {
                if (Foldout(ref Prefs.Foldouts.walls)) {
                    if (GUIpreviewButtonsGrid(wallsGUIcontents, out int clicked)) AddWall((WallType)clicked);
                }
            }
            
            // OBJECTS
            if (levelData.objectsSet != null && (levelData.objectsSet.entries != null && levelData.objectsSet.entries.Length > 0)) {
                if (Foldout(ref Prefs.Foldouts.objects)) {
                    FillGUIcontents(ref objectsGUIcontents, levelData.objectsSet.entries, ref actualObjectsEntries,
                        (ref ObjectsSetEntry entry, out UnityEngine.Object prefab, out string name) => {
                            prefab = entry.prefab;
                            var prefabName = entry.prefab == null ? null : prefab.name;
                            name = string.IsNullOrEmpty(entry.name) ? prefabName : entry.name;
                        }
                    );
                    if (GUIpreviewButtonsGrid(objectsGUIcontents, out int clicked)) AddSimpleObject(actualObjectsEntries[clicked]);
                }
            }
            
            // FLOORS
            if (levelData.mainSet != null && levelData.mainSet.floors != null && levelData.mainSet.floors.Length > 0) {
                if (Foldout(ref Prefs.Foldouts.floors)) {
                    FillGUIcontents(ref floorsGUIcontents, levelData.mainSet.floors, ref actualFloorsEntries,
                        (ref FloorEntry entry, out UnityEngine.Object prefab, out string name) => {
                            prefab = entry.floorPrefab;
                            name = prefab == null ? null : prefab.name + " (" + entry.size + "x" + entry.size + ")";
                        }
                    );
                    if (GUIpreviewButtonsGrid(floorsGUIcontents, out int clicked)) AddFloor(actualFloorsEntries[clicked]);
                }
            }
            
            
            // SETTINGS
            if (Foldout(ref Prefs.Foldouts.settings)) {
                if (levelData_SO_Editor.DataField()) OnLevelData_SO_Update();
                guiCalcHeight += levelData_SO_Editor.GetCalculatedHeight();
            }
            
            if (verticalScrollOn) GUILayout.EndScrollView();
        }
        private bool GUIpreviewButtonsGrid(GUIContent[] GUIContents, out int clicked) {
            clicked = -1;
            if (GUIContents == null || GUIContents.Length == 0) return false;
            int buttonsNum = GUIContents.Length;
            var buttonStyle = Styles.objectPreviewButton;
            int screenWidth = (int)(Screen.width / EditorGUIUtility.pixelsPerPoint);
            int areaWidth = verticalScrollOn ? screenWidth - (int)GUI.skin.verticalScrollbar.fixedWidth : screenWidth;
            int buttonMargin = buttonStyle.margin.right;
            int buttonsPerRow = (areaWidth - 4 + buttonMargin - 1) / (buttonsSizeMax + buttonMargin);
            if (buttonsPerRow < buttonsPerRowMin) buttonsPerRow = buttonsPerRowMin;
            const int shrinkFix = 1;
            int shrinkWidth = buttonsPerRow * buttonsSizeMax + (buttonMargin * (buttonsPerRow + 1)) + shrinkFix;
            int rows = 1 + (GUIContents.Length - 1) / buttonsPerRow;
            int buttonSizeWithoutScroll = ButtonSize(screenWidth, shrinkWidth, buttonMargin, buttonsPerRow);
            int buttonSize = verticalScrollOn ? ButtonSize(areaWidth, shrinkWidth, buttonMargin, buttonsPerRow) : buttonSizeWithoutScroll;
            int height = buttonSizeWithoutScroll * rows + (rows - 1) * buttonMargin;
            
            buttonStyle.fixedWidth = buttonSize;
            buttonStyle.fixedHeight = buttonSize;
            
            GUILayout.BeginVertical();
            for (int i = 0, j = 1; i < buttonsNum; i++, j++) {
                if (j == 1) {
                    GUILayout.BeginHorizontal();
                }
                if (GUILayout.Button(GUIContents[i], buttonStyle)) clicked = i;
                if (j == buttonsPerRow || i == buttonsNum - 1) {
                    GUILayout.EndHorizontal();
                    if (i < buttonsNum - 1) GUILayout.Space(buttonMargin);
                    j = 0;
                }
            }
            GUILayout.EndVertical();
            if (verticalScrollOn) GUILayout.Space(height - (rows * (buttonSize + buttonMargin) - buttonMargin));
            
            guiCalcHeight += height;
            
            if (clicked >= 0 && Event.current.button == 0) return true;
            return false;
        }
        private int  ButtonSize(int availableWidth, int shrinkWidth, int buttonMargin, int buttonsNum) {
            if (availableWidth < shrinkWidth) {
                const int shrinkFix = 1;
                return (availableWidth - (buttonMargin * (buttonsNum + 1)) - shrinkFix) / buttonsNum;
            } else return buttonsSizeMax;
        }
        private delegate void PickAsset<T>(ref T pickFrom, out UnityEngine.Object prefab, out string name);
        private void FillGUIcontents<T>(ref GUIContent[] GUIcontents, T[] assetPickArray, ref T[] actualAssetArray, PickAsset<T> pickCallback) {
            if (GUIcontents == null) {
                var GUIcontentsList = new List<GUIContent>();
                var actualAssetList = new List<T>();
                var loading = new List<UnityEngine.Object>();
                for (int i = 0; i < assetPickArray.Length; i++) {
                    pickCallback(ref assetPickArray[i], out var prefab, out var name);
                    if (prefab != null) {
                        var preview = AssetPreview.GetAssetPreview(prefab);
                        if (preview == null) {
                            nullPreviewsRepaint = true;
                            break;
                        }
                        GUIcontentsList.Add(new GUIContent(preview, name));
                        actualAssetList.Add(assetPickArray[i]);
                        if (AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID())) loading.Add(prefab);
                    }
                }
                if (!nullPreviewsRepaint) {
                    GUIcontents = GUIcontentsList.ToArray();
                    actualAssetArray = actualAssetList.ToArray();
                    ArrayUtility.AddRange(ref loadingAssetsPreviews, loading.ToArray());
                }
            }
        }
        private void NullPreviewRepaint() {
            if (nullPreviewsAttempts > 100) {
                nullPreviewsRepaint = false;
            } else {
                var currentTime = DateTime.UtcNow;
                var nullPreviewRepaintPause = currentTime - nullPreviewsLastAttemptTime;
                if (nullPreviewRepaintPause.TotalMilliseconds > 200) {
                    nullPreviewsRepaint = false;
                    nullPreviewsAttempts++;
                    nullPreviewsLastAttemptTime = currentTime;
                    Repaint();
                }
            }
        }
        private bool Foldout(ref Prefs.Foldouts.Foldout foldout) {
            var foldValue = EditorGUILayout.BeginFoldoutHeaderGroup(foldout.value, foldout.name, Styles.foldStyle);
            if (foldValue != foldout.value) foldout.SetValue(foldValue);
            EditorGUILayout.EndFoldoutHeaderGroup();
            guiCalcHeight += Styles.foldHeight;
            return foldValue;
        }
        private void CreatePickupGUI() {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create / Pickup", Styles.createButton)) CreateOrPickup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }
        private void OnSceneGUI(SceneView sceneView) {
            if (!this.hasFocus || root == null || Application.isPlaying) return;
            SceneObject.SampleCameraPosition();
            SceneObject.SetRootTransform(root.transform);
            
            if (selectionStatus != SelectionStatus.MANAGED) return;
            
            SortObjects();
            var objectsLength = sortedObjects.Length;
            for (int i = 0; i < objectsLength; i++) sortedObjects[i].SceneGUI();
            
            if (levelData.optionsSettings.hoverResponsiveHandles && objectsLength > 0 && Event.current.type == EventType.MouseMove) {
                HandleUtility.Repaint();
            }
        }
    #endregion
    #region OBJECTS ADD/REMOVE 
        private        void AddWall(WallType wallType) {
            if (!Operating() || Event.current.button != 0) return;
            GameObject wallPrefab = null;
            switch (wallType) {
                case WallType.WALL_I: wallPrefab = levelData.mainSet.wall_I; break;
                case WallType.WALL_L: wallPrefab = levelData.mainSet.wall_L; break;
                case WallType.WALL_T: wallPrefab = levelData.mainSet.wall_T; break;
                case WallType.WALL_X: wallPrefab = levelData.mainSet.wall_X; break;
            }
            if (wallPrefab == null) return;
            if (NewObjectCeiledPosition(Wall.GetMoveSnap(), out Vector3 ceiledPosition)) {
                GameObject wallGO = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab, root.transform);
                wallGO.name = wallPrefab.name;
                wallGO.transform.localPosition = ceiledPosition;
                GameObject cap = levelData.mainSet.wall_Caps == null || levelData.mainSet.wall_Caps.Length == 0 ? null : levelData.mainSet.wall_Caps[0];
                Wall newWall = new Wall(wallGO, wallType, cap);
                objects.Add(newWall);
                walls.Add(newWall);
                levelDataNotActual = true;
                if (undoCreatedObjectsOn) Undo.RegisterCreatedObjectUndo(wallGO, "Add " + wallGO.name);
            }
        }
        private        void AddSimpleObject(ObjectsSetEntry objectsSetEntry) {
            if (!Operating() || Event.current.button != 0 || objectsSetEntry.prefab == null) return;
            if (NewObjectCeiledPosition(SimpleObject.GetMoveSnap(), out Vector3 ceiledPosition)) {
                var objectPrefab = objectsSetEntry.prefab;
                if (objectPrefab == null) return;
                GameObject objectGO = (GameObject)PrefabUtility.InstantiatePrefab(objectPrefab, root.transform);
                objectGO.name = string.IsNullOrEmpty(objectsSetEntry.name) ? objectsSetEntry.prefab.name : objectsSetEntry.name;
                objectGO.transform.localPosition = ceiledPosition;
                SimpleObject newSimpleObject = new SimpleObject(objectGO);
                simpleObjects.Add(newSimpleObject);
                objects.Add(newSimpleObject);
                levelDataNotActual = true;
                if (undoCreatedObjectsOn) Undo.RegisterCreatedObjectUndo(objectGO, "Add " + objectGO.name);
            }
        }
        private        void AddFloor(FloorEntry floorEntry) {
            if (!Operating() || Event.current.button != 0 || floorEntry.floorPrefab == null) return;
            
            if (NewObjectCeiledPosition(Floor.GetMoveSnap(), out Vector3 ceiledPosition)) {
                var floorGO = new GameObject(floorEntry.floorPrefab.name);
                floorGO.transform.parent = root.transform;
                floorGO.transform.localPosition = ceiledPosition;
                floorGO.transform.localRotation = Quaternion.identity;
                floorGO.isStatic = true;
                Floor newFloor = new Floor(floorGO, floorEntry);
                floors.Add(newFloor);
                objects.Add(newFloor);
                levelDataNotActual = true;
                if (undoCreatedObjectsOn) Undo.RegisterCreatedObjectUndo(floorGO, "Add " + floorGO.name);
            }
        }
        private        bool RemoveDisappearedObject(SceneObject disappearedObject) {
            RemoveLevelObjectFromList<Wall>(walls, disappearedObject);
            RemoveLevelObjectFromList<SimpleObject>(simpleObjects, disappearedObject);
            RemoveLevelObjectFromList<Floor>(floors, disappearedObject);
            RemoveLevelObjectFromList<SceneObject>(objects, disappearedObject);
            for (int i = 0; i < sortedObjects.Length; i++) {
                if (ReferenceEquals(sortedObjects[i], disappearedObject)) {
                    ArrayUtility.RemoveAt(ref sortedObjects, i);
                    return true;
                }
            }
            return false;
        }
        public  static void Remove(SceneObject levelObjectToRemove) {
            window.RemoveLevelObject(levelObjectToRemove);
        }
        private        void RemoveLevelObject(SceneObject levelObjectToRemove) {
            RemoveLevelObjectFromList(walls, levelObjectToRemove);
            RemoveLevelObjectFromList(simpleObjects, levelObjectToRemove);
            RemoveLevelObjectFromList(floors, levelObjectToRemove);
            RemoveLevelObjectFromList(objects, levelObjectToRemove);
            RemoveGameObjectFromRoot(levelObjectToRemove.gameObject);
            levelDataNotActual = true;
        }
        private static void RemoveLevelObjectFromList<T>(List<T> list, SceneObject levelObject) where T : SceneObject {
            for (int i = 0; i < list.Count; i++) {
                if (ReferenceEquals(list[i], levelObject)) {
                    list.RemoveAt(i);
                    break;
                }
            }
        }
        private static void RemoveGameObjectFromRoot(GameObject rootChildGameObject) {
            if (PrefabUtility.IsAddedGameObjectOverride(rootChildGameObject)) {
                GameObject.DestroyImmediate(rootChildGameObject, false);
            } else {
                Utility.DestroyAddedOverridesInside(rootChildGameObject);
                string rootPrefabPath = GetActiveRootPrefabPath();
                var rootGO = PrefabUtility.LoadPrefabContents(rootPrefabPath);
                GameObject deletingGO = rootGO.transform.GetChild(rootChildGameObject.transform.GetSiblingIndex()).gameObject;
                GameObject.DestroyImmediate(deletingGO, false);
                PrefabUtility.SaveAsPrefabAsset(rootGO, rootPrefabPath);
                PrefabUtility.UnloadPrefabContents(rootGO);
            }
        }
    #endregion
    #region WORK    INIT/SAVE 
        private        void   InitializeManaged() {
            // SceneObject.SampleCameraPosition();
            objects = new List<SceneObject>(root.transform.childCount);
            walls = new List<Wall>(levelData.wallsData == null ? 0 : levelData.wallsData.Length);
            simpleObjects = new List<SimpleObject>(levelData.simpleObjectsData == null ? 0 : levelData.simpleObjectsData.Length);
            floors = new List<Floor>(levelData.floorsData == null ? 0 : levelData.floorsData.Length);
            sortedObjects = null;
            
            var rootPrefabPath = AssetDatabase.GUIDToAssetPath(rootPrefab_GUID);
            var wallsData = levelData.wallsData;
            var simpleObjectsData = levelData.simpleObjectsData;
            var floorsData = levelData.floorsData;
            
            var childList = Utility.GetChildrenListWithFileIDs(root, rootPrefabPath);
            
            if (wallsData != null) {
                for (int i = 0; i < wallsData.Length; i++) {
                    var wall = Wall.TryPickUp(wallsData[i], childList, rootPrefabPath);
                    if (wall != null) walls.Add(wall);
                }
            }
            if (simpleObjectsData != null) {
                for (int i = 0; i < simpleObjectsData.Length; i++) {
                    var simpleObject = SimpleObject.TryPickUp(simpleObjectsData[i], childList);
                    if (simpleObject != null) simpleObjects.Add(simpleObject);
                }
            }
            if (floorsData != null) {
                for (int i = 0; i < floorsData.Length; i++) {
                    var floor = Floor.TryPickUp(floorsData[i], childList, rootPrefabPath);
                    if (floor != null) floors.Add(floor);
                }
            }
            
            objects.AddRange(walls);
            objects.AddRange(simpleObjects);
            objects.AddRange(floors);
            
            CustomHandles.SetMaxSize(1f, HandlesStyles.handleMaxSizeBasis);
            WallWing.SetManipulatorsHeight(levelData != null ? levelData.optionsSettings.wallsManipulatorsHeight : 0);
            UpdateWallLinePositions();
            
            objectsGUIcontents = null;
            floorsGUIcontents = null;
        }
        private        void   DeferredSave() {
            if (selectionStatus != SelectionStatus.MANAGED) return;
            
            deferredSaveRootPrefabPath = AssetDatabase.GUIDToAssetPath(rootPrefab_GUID);
            deferredSave = true;
        }
        private        string SavingRootPrefabPath(bool deferred) {
            return deferred ? deferredSaveRootPrefabPath : AssetDatabase.GUIDToAssetPath(rootPrefab_GUID);
        }
        private        void   Save() {
            if (!Operating()) return;
            
            if (selectionStatus == SelectionStatus.MANAGED || deferredSave || levelDataNotActual) {
                if (EditorApplication.isUpdating) {
                    DeferredSave();
                    return;
                }
                
                bool hasPrefabOverrides = PrefabUtility.HasPrefabInstanceAnyOverrides(root, false);
                int overridesCount = 0;
                if (hasPrefabOverrides) {
                    var rootTransform = root.transform;
                    
                    var list1 = PrefabUtility.GetAddedGameObjects(root);
                    var list2 = PrefabUtility.GetAddedComponents(root);
                    var list3 = PrefabUtility.GetRemovedComponents(root);
                    var list4 = PrefabUtility.GetObjectOverrides(root, false);
                    
                    List<PrefabOverride> addedGameObjectsOverrides = new List<PrefabOverride>();
                    List<PrefabOverride> addedComponentsOverrides = new List<PrefabOverride>();
                    List<PrefabOverride> removedComponentsOverrides = new List<PrefabOverride>();
                    List<PrefabOverride> objectOverrides = new List<PrefabOverride>();
                    
                    list1.ForEach(item => {if (IsOneOfManagedGameObjects(item.instanceGameObject)) addedGameObjectsOverrides.Add(item);});
                    list2.ForEach(item => {if (IsOneOfManagedGameObjects(item.instanceComponent.gameObject)) addedComponentsOverrides.Add(item);});
                    list3.ForEach(item => {if (IsOneOfManagedGameObjects(item.containingInstanceGameObject)) removedComponentsOverrides.Add(item);});
                    list4.ForEach(item => {
                        System.Type type = item.instanceObject.GetType();
                        GameObject overridenObject = null;
                        
                        if (type == typeof(Transform)) {
                            overridenObject = ((Transform)item.instanceObject).gameObject;
                        } else if (type == typeof(GameObject)) {
                            overridenObject = (GameObject)item.instanceObject;
                        } else if (type == typeof(MeshRenderer)) {
                            overridenObject = ((MeshRenderer)item.instanceObject).gameObject;
                        } else if (type == typeof(MeshFilter)) {
                            overridenObject = ((MeshFilter)item.instanceObject).gameObject;
                        } else if (type.IsSubclassOf(typeof(Behaviour)) || type == typeof(Behaviour)) {
                            overridenObject = ((Behaviour)item.instanceObject).gameObject;
                        }
                        
                        if (overridenObject != null) {
                            if (IsOneOfManagedGameObjects(overridenObject)) objectOverrides.Add(item);
                        } else {
                            Debug.LogWarning("LevelDesigner: override of type " + type.ToString() + " was not handled");
                        }
                    });
                    
                    string rootPrefabPath = SavingRootPrefabPath(deferredSave);
                    #if !UNITY_2020_2
                    List<PrefabOverride>[] overrides = new List<PrefabOverride>[]{
                        addedGameObjectsOverrides,
                        addedComponentsOverrides,
                        removedComponentsOverrides,
                        objectOverrides
                    };
                    #else
                    List<PrefabOverride>[] overrides = new List<PrefabOverride>[]{
                        addedComponentsOverrides,
                        removedComponentsOverrides,
                        objectOverrides
                    };
                    for (int i = 0; i < addedGameObjectsOverrides.Count; i++) addedGameObjectsOverrides[i].Apply(rootPrefabPath);
                    overridesCount += addedGameObjectsOverrides.Count;
                    #endif
                    
                    for (int i = 0; i < overrides.Length; i++) {
                        if (overrides[i].Count > 0) {
                            overridesCount += overrides[i].Count;
                            AssetDatabase.StartAssetEditing();
                            for (int j = 0; j < overrides[i].Count; j++) overrides[i][j].Apply(rootPrefabPath);
                            AssetDatabase.StopAssetEditing();
                        }
                    }
                }
                
                if (overridesCount > 0 || levelDataNotActual || levelData != levelData_SO.LevelData()) {
                    string rootPrefabPath = SavingRootPrefabPath(deferredSave);
                    
                    var wallsData = new WallData[walls.Count];
                    for (int i = 0; i < wallsData.Length; i++) wallsData[i] = walls[i].GatherData(rootPrefabPath);
                    levelData.wallsData = wallsData;
                    
                    var simpleObjectsData = new SimpleObjectData[simpleObjects.Count];
                    for (int i = 0; i < simpleObjectsData.Length; i++) simpleObjectsData[i] = simpleObjects[i].GatherData(rootPrefabPath);
                    levelData.simpleObjectsData = simpleObjectsData;

                    var floorsData = new FloorData[floors.Count];
                    for (int i = 0; i < floorsData.Length; i++) floorsData[i] = floors[i].GatherData(rootPrefabPath);
                    levelData.floorsData = floorsData;

                    if (levelData != levelData_SO.LevelData()) levelData_SO.UpdateData(levelData);
                }
                
                if (deferredSave) {
                    deferredSaveRootPrefabPath = null;
                    deferredSave = false;
                }
            }
        }
        private        void   Flush() {
            locked = false;
            loadingAssetsPreviews = new UnityEngine.Object[0];
            nullPreviewsRepaint = false;
            nullPreviewsAttempts = 0;
            nullPreviewsLastAttemptTime = DateTime.UtcNow;
            nullIconsPreviewsLastAttemptTime = DateTime.UtcNow;
            objectsGUIcontents = null;
            floorsGUIcontents = null;
            
            SO = null;
            selection = null;
            selectionActive = false;
            selectionStatus = SelectionStatus.NONE;
            deferredSave = false;
            root = null;
            rootPrefab_GUID = null;
            levelData_SO = null;
            levelData_SO_Editor = null;
            levelData  = null;
            levelDataNotActual = false;
            
            objects = null;
            sortedObjects = null;
            walls = null;
            simpleObjects = null;
            floors = null;
        }
        public  static void   SaveCall() {
            if (window != null && window.selectionStatus == SelectionStatus.MANAGED) window.Save();
        }
        public  static void   SetLevelDataNotActual() {
            if (window != null && window.selectionStatus == SelectionStatus.MANAGED) window.levelDataNotActual = true;
        }
        private        void   CheckSO() {
            if (SO != null) return;
            var rootDir = RootDir();
            if (string.IsNullOrEmpty(rootDir)) return;
            var SO_path = rootDir + Prefs.levelDesignerMainSOassetName;
            SO = AssetDatabase.LoadAssetAtPath<LevelDesigner_SO>(SO_path);
            if (SO == null) {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(typeof(LevelDesigner_SO)), SO_path);
                SO = AssetDatabase.LoadAssetAtPath<LevelDesigner_SO>(SO_path);
            } else {
                SO.OneTimeCleaning();
            }
        }
        private        bool   Operating() {
            return root != null;
        }
        private static string RootDir() {
            if (window == null) return null;
            var dir = new DirectoryInfo(System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(window))));
            if (dir.Name == "Stuff") dir = dir.Parent;
            var rootDir = new System.Text.StringBuilder();
            var done = false;
            while (!done) {
                rootDir.Insert(0, System.IO.Path.DirectorySeparatorChar);
                rootDir.Insert(0, dir.Name);
                if (dir.Parent == null || dir.Name == "Assets") done = true;
                else dir = dir.Parent;
            }
            return rootDir.ToString();
        }
    #endregion
    #region OTHER 
        public  static Main_Set_SO        GetActiveMainSet() {
            return window?.levelData?.mainSet;
        }
        public  static string             GetActiveRootPrefabPath() {
            return AssetDatabase.GUIDToAssetPath(window.rootPrefab_GUID);
        }
        public  static VisibilitySettings VisibilitySettings() {
            return window.levelData.visibilitySettings;
        }
        public  static OptionsSettings    OptionsSettings() {
            return window.levelData.optionsSettings;
        }
        private static GameObject         GetSelectionGameObject() {
            return Selection.gameObjects.Length == 1 ? Selection.gameObjects[0] : null;
        }
        
        private void CheckLoadingPreviews() {
            bool repaint = false;
            for (int i = 0; i < loadingAssetsPreviews.Length; i++) {
                AssetPreview.GetAssetPreview(loadingAssetsPreviews[i]);
                if (!AssetPreview.IsLoadingAssetPreview(loadingAssetsPreviews[i].GetInstanceID())) {
                    ArrayUtility.RemoveAt(ref loadingAssetsPreviews, i);
                    i--;
                    repaint = true;
                }
            }
            if (repaint) {
                objectsGUIcontents = null;
                floorsGUIcontents = null;
                Repaint();
            }
        }
        private bool LoadIcons() {
            if (wallsGUIcontents != null) return true;
            
            string iconsDir = RootDir() + "Icons" + System.IO.Path.DirectorySeparatorChar;
            var loadingIcons = new List<UnityEngine.Object>();
            
            Texture2D icon_Wall_I = AssetDatabase.LoadAssetAtPath<Texture2D>(iconsDir + "wall_I.png");
            var preview_wall_I = AssetPreview.GetAssetPreview(icon_Wall_I);
            if (preview_wall_I == null || AssetPreview.IsLoadingAssetPreview(icon_Wall_I.GetInstanceID())) return false;
            
            Texture2D icon_Wall_L = AssetDatabase.LoadAssetAtPath<Texture2D>(iconsDir + "wall_L.png");
            var preview_wall_L = AssetPreview.GetAssetPreview(icon_Wall_L);
            if (preview_wall_L == null || AssetPreview.IsLoadingAssetPreview(icon_Wall_L.GetInstanceID())) return false;
            
            Texture2D icon_Wall_T = AssetDatabase.LoadAssetAtPath<Texture2D>(iconsDir + "wall_T.png");
            var preview_wall_T = AssetPreview.GetAssetPreview(icon_Wall_T);
            if (preview_wall_T == null || AssetPreview.IsLoadingAssetPreview(icon_Wall_T.GetInstanceID())) return false;
            
            Texture2D icon_Wall_X = AssetDatabase.LoadAssetAtPath<Texture2D>(iconsDir + "wall_X.png");
            var preview_wall_X = AssetPreview.GetAssetPreview(icon_Wall_X);
            if (preview_wall_X == null || AssetPreview.IsLoadingAssetPreview(icon_Wall_X.GetInstanceID())) return false;
            
            Prefs.Icons.wall_I = preview_wall_I;
            Prefs.Icons.wall_L = preview_wall_L;
            Prefs.Icons.wall_T = preview_wall_T;
            Prefs.Icons.wall_X = preview_wall_X;
            
            wallsGUIcontents = new GUIContent[]{
                new GUIContent(Prefs.Icons.wall_I),
                new GUIContent(Prefs.Icons.wall_L),
                new GUIContent(Prefs.Icons.wall_T),
                new GUIContent(Prefs.Icons.wall_X)
            };
            
            return true;
        }
        private void OnBeforeSceneSave(Scene scene, string scenePath) {
            Save();
        }
        private void OnBeforeSceneClose(Scene scene, bool removingScene) {
            Save();
            locked = false;
            OnSelectionChange();
        }
        private void OnPlayModeStateChanged(PlayModeStateChange stateChange) {
            if (stateChange == PlayModeStateChange.EnteredEditMode) OnSelectionChange();
            if (stateChange == PlayModeStateChange.ExitingEditMode) Save();
        }
        private void UpdateSceneGUIdrawingData() {
            if (sortedObjects != null) {
                for (int i = 0; i < sortedObjects.Length; i++) {
                    sortedObjects[i].UpdateSceneGUIdrawingData();
                }
            }
        }
        private bool CheckAndFixUnexpectedChangesAfterUndo() {
            var unexpectedChanges = false;
            if (sortedObjects != null) {
                for (int i = 0; i < sortedObjects.Length; i++) {
                    if (sortedObjects[i].CheckAndFixUnexpectedChangesAfterUndo()) unexpectedChanges = true;
                }
            }
            
            return unexpectedChanges;
        }
        private void UpdateWallLinePositions() {
            if (walls == null) return;
            for (int i = 0; i < walls.Count; i++) walls[i].UpdateWallLinePositions();
        }
        private bool OnEditorWantsToQuit() {
            Save();
            return true;
        }
        private bool IsOneOfManagedGameObjects(GameObject gameObject) {
            for (int i = 0; i < objects.Count; i++) if (objects[i].IsGameObjectManaged(gameObject)) return true;
            return false;
        }
        private void HierarchyActiveGameObjectSelected() {
            selectionActive = true;
            if (SelectionIsSuitable()) {
                CheckSO();
                selectionStatus = SelectionStatus.SUITABLE;
                if (SelectionIsManaged()) {
                    nullPreviewsLastAttemptTime = DateTime.UtcNow;
                    nullPreviewsAttempts = 0;
                    selectionStatus = SelectionStatus.MANAGED;
                    UndoRedoPerformedSubscribe();
                }
            }
        }
        private void HierarchyGameObjectDeselected() {
            // Save();
            DeferredSave();
            selectionActive = false;
            selectionStatus = SelectionStatus.NONE;
            UndoRedoPerformedUnsubscribe();
        }
        private void UndoRedoPerformedSubscribe() {
            if (undoRedoPerformedSubscribed) return;
            
            Undo.undoRedoPerformed += UndoRedoPerformed;
            undoRedoPerformedSubscribed = true;
        }
        private void UndoRedoPerformedUnsubscribe() {
            if (!undoRedoPerformedSubscribed) return;
            
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            undoRedoPerformedSubscribed = false;
        }
        private void UndoRedoPerformed() {
            if (undoCreatedObjectsOn) {
                if (sortedObjects == null) return;
                for (int i = 0; i < sortedObjects.Length; i++) {
                    if (sortedObjects[i].gameObject == null) {
                        if (RemoveDisappearedObject(sortedObjects[i])) i--;
                    }
                }
            }
            
            UpdateSceneGUIdrawingData();
            if (CheckAndFixUnexpectedChangesAfterUndo()) DeferredSave();
        }
        private void ShowButton(Rect position) {
            bool lockedNew = GUI.Toggle(position, locked, GUIContent.none, Styles.lockButton);
            if (lockedNew != locked) {
                locked = lockedNew;
                if (!locked) {
                    var currentSelection = GetSelectionGameObject();
                    if (currentSelection != selection || currentSelection == null) OnSelectionChange();
                }
            }
        }
        private void SortObjects() {
            bool forceSorting = false;
            
            if (sortedObjects == null || sortedObjects.Length != objects.Count) {
                sortedObjects = new SceneObject[objects.Count];
                for (int i = 0; i < sortedObjects.Length; i++) sortedObjects[i] = objects[i];
                forceSorting = true;
            }
            
            Vector3 cameraPosition = SceneObject.GetSampledCameraPosition();
            if ((GUIUtility.hotControl != 0 || cameraPosition == prevCameraPosition) && !forceSorting) return;
            prevCameraPosition = cameraPosition;
            
            Vector3 farthest = Vector3.positiveInfinity;
            int farthestI;
            for (int i = 0; i < sortedObjects.Length; i++) {
                farthestI = i;
                for (int j = i; j < sortedObjects.Length; j++) {
                    Vector3 camVector = cameraPosition - sortedObjects[j].gameObject.transform.position;
                    if (j == i || camVector.sqrMagnitude > farthest.sqrMagnitude) {
                        farthest = camVector;
                        farthestI = j;
                    }
                }
                if (farthestI > i) {
                    SceneObject tempLevelObject = sortedObjects[i];
                    sortedObjects[i] = sortedObjects[farthestI];
                    sortedObjects[farthestI] = tempLevelObject;
                }
                sortedObjects[i].SortObjects();
            }
        }
        private bool NewObjectCeiledPosition(float snap, out Vector3 position) {
            Camera sceneCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
            Ray centerRay = sceneCamera.ScreenPointToRay(new Vector3(sceneCamera.pixelWidth * 0.5f, sceneCamera.pixelHeight * 0.25f, 0));
            Plane floorPlane = SceneObject.GetFloorPlane();
            if (floorPlane.Raycast(centerRay, out float distance)) {
                Vector3 targetingPosition = centerRay.GetPoint(distance);
                targetingPosition = root.transform.worldToLocalMatrix.MultiplyPoint3x4(targetingPosition);
                position = new Vector3(Mathf.Round(targetingPosition.x / snap) * snap, 0, Mathf.Round(targetingPosition.z / snap) * snap);
                return true;
            }
            position = Vector3.zero;
            return false;
        }
        private void OnLevelData_SO_Update() {
            levelData = levelData_SO.LevelDataClone();
            if (levelData.optionsSettings.wallsManipulatorsHeight != WallWing.GetManipulatorsHeight()) {
                WallWing.SetManipulatorsHeight(levelData.optionsSettings.wallsManipulatorsHeight);
                UpdateWallLinePositions();
            }
            levelDataNotActual = false;
            objectsGUIcontents = null;
            floorsGUIcontents = null;
            Repaint();
        }
        private bool SelectionIsSuitable() {
            if (selection == null || !selectionActive || !selection.activeInHierarchy || PrefabUtility.IsPartOfImmutablePrefab(selection)) return false;
            return true;
        }
        private bool SelectionIsManaged() {
            if (SO == null) return false;
            GameObjectIdentity selectionIdentity = Utility.GetGameObjectIdentity(selection, out bool _, out bool _);
            if (selectionIdentity.identity == Identity.UNDEFINED) return false;

            if (SO.TryGetRootGUID(selectionIdentity, out rootPrefab_GUID)) {
                for (int i = 0; i < selection.transform.childCount; i++) {
                    Transform child = selection.transform.GetChild(i);
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject)) {
                        if (AssetDatabase.AssetPathToGUID(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child.gameObject)) == rootPrefab_GUID) {
                            GetExistingLevelData_Pair_GUIDS(out string[] levelDataSO_GUIDS, out string[] levelDataPrefab_GUIDS);
                            for (int j = 0; j < levelDataPrefab_GUIDS.Length; j++) {
                                if (levelDataPrefab_GUIDS[j] == rootPrefab_GUID) {
                                    levelData_SO = AssetDatabase.LoadAssetAtPath<LevelData_SO>(AssetDatabase.GUIDToAssetPath(levelDataSO_GUIDS[j]));
                                    if (levelData_SO is null) continue;
                                    Editor cachedEditor = null;
                                    Editor.CreateCachedEditor(levelData_SO, typeof(LevelData_SO_Editor), ref cachedEditor);
                                    levelData_SO_Editor = (LevelData_SO_Editor)cachedEditor;
                                    levelData_SO_Editor.UseFoldoutMarginFix(false);
                                    OnLevelData_SO_Update();
                                    root = child.gameObject;
                                    InitializeManaged();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        private bool TryPickupManaged() {
            GameObjectIdentity selectionIdentity = Utility.GetGameObjectIdentity(selection, out bool _, out bool _);
            if (selectionIdentity.identity == Identity.UNDEFINED) return false;
            
            List<string> childrenGUIDS = new List<string>();
            
            for (int i = 0; i < selection.transform.childCount; i++) {
                GameObject child = selection.transform.GetChild(i).gameObject;
                if (PrefabUtility.IsAnyPrefabInstanceRoot(child)) {
                    childrenGUIDS.Add(AssetDatabase.AssetPathToGUID(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child)));
                }
            }
            
            GetExistingRootPrefabs_GUIDS(out string[] existingRootPrefabs_GUIDS);
            if (SO.TryConnectAny(selectionIdentity, childrenGUIDS, existingRootPrefabs_GUIDS)) {
                OnSelectionChange();
                return true;
            }
            
            return false;
        }
        private void GetExistingLevelData_Pair_GUIDS(out string[] levelDataSO_GUIDS, out string[] levelDataPrefab_GUIDS) {
            GetExistingLevelData_SO_GUIDS(out levelDataSO_GUIDS);
            var levelDataSO_GUIDSList = new List<string>();
            var levelDataPrefab_GUIDSList = new List<string>();
            for (int i = 0; i < levelDataSO_GUIDS.Length; i++) {
                LevelData_SO ldSO = AssetDatabase.LoadAssetAtPath<LevelData_SO>(AssetDatabase.GUIDToAssetPath(levelDataSO_GUIDS[i]));
                if (ldSO != null) {
                    levelDataSO_GUIDSList.Add(levelDataSO_GUIDS[i]);
                    levelDataPrefab_GUIDSList.Add(ldSO.GetRootPrefabGUID());
                }
            }
            levelDataSO_GUIDS = levelDataSO_GUIDSList.ToArray();
            levelDataPrefab_GUIDS = levelDataPrefab_GUIDSList.ToArray();
        }
        private void GetExistingLevelData_SO_GUIDS(out string[] levelDataSO_GUIDS) {
            levelDataSO_GUIDS = AssetDatabase.FindAssets("t:" + nameof(LevelData_SO));
        }
        private void GetExistingRootPrefabs_GUIDS(out string[] levelDataPrefab_GUIDS) {
            GetExistingLevelData_Pair_GUIDS(out string[] _, out levelDataPrefab_GUIDS);
        }
        private void CreateOrPickup() {
            if (!SelectionIsSuitable()) return;
            if (!TryPickupManaged()) CreateManaged();
        }
        private void CreateManaged() {
            GameObjectIdentity selectionIdentity = Utility.GetGameObjectIdentity(selection, out bool selectionPrefabIdentity, out bool _);
            if (selectionIdentity.identity == Identity.UNDEFINED) return;
            
            string rootPrefabDir;
            string selectionIdentityGUIDassetPath = null;
            if (selectionPrefabIdentity) {
                selectionIdentityGUIDassetPath = AssetDatabase.GUIDToAssetPath(selectionIdentity.GUID);
                rootPrefabDir = System.IO.Path.GetDirectoryName(selectionIdentityGUIDassetPath);
            } else {
                rootPrefabDir = "Assets";
            }
            string rootPrefabName = selection.name + "." + Prefs.levelDesignObjectName;
            rootPrefabDir += System.IO.Path.DirectorySeparatorChar;

            const string prefabExt = ".prefab";
            int i = 1;
            string checkingPrefabName = rootPrefabName;
            var rootPrefabPath = rootPrefabDir + checkingPrefabName + prefabExt;
            while (AssetDatabase.GetMainAssetTypeAtPath(rootPrefabPath) != null) {
                checkingPrefabName = rootPrefabName + (++i).ToString();
                rootPrefabPath = rootPrefabDir + checkingPrefabName + prefabExt;
            }
            rootPrefabName = checkingPrefabName;
            root = new GameObject {
                name = Prefs.levelDesignObjectName
            };
            root.transform.SetParent(selection.transform, false);
            root.gameObject.isStatic = true;
            var rootPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(root.gameObject, rootPrefabPath, InteractionMode.AutomatedAction);
            
            if (selectionPrefabIdentity) {
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage == null || PrefabUtility.IsPartOfAnyPrefab(selection)) {
                    List<AddedGameObject> addedGO = PrefabUtility.GetAddedGameObjects(selection);
                    for (i = 0; i < addedGO.Count; i++) {
                        if (addedGO[i].instanceGameObject == root.gameObject) {
                            addedGO[i].Apply(selectionIdentityGUIDassetPath);
                            break;
                        }
                    }
                } else {
                    EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }
            } else {
                EditorSceneManager.MarkSceneDirty(root.scene);
            }
            
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(rootPrefab, out rootPrefab_GUID, out long _)) {
                selectionIdentity.rootGUID = rootPrefab_GUID;
                SO.Set(selectionIdentity);
                
                // Find latest created levelData SO
                GetExistingLevelData_SO_GUIDS(out string[] levelDatas);
                long latestDataCreationTime = 0;
                string latestDataPath = "";
                for (int j = 0; j < levelDatas.Length; j++) {
                    string ldPath = AssetDatabase.GUIDToAssetPath(levelDatas[j]);
                    long createdTime = File.GetCreationTime(ldPath).ToFileTimeUtc();
                    if (createdTime > latestDataCreationTime) {
                        latestDataCreationTime = createdTime;
                        latestDataPath = ldPath;
                    }
                }
                
                levelData_SO = (LevelData_SO)ScriptableObject.CreateInstance(typeof(LevelData_SO));
                levelData_SO.SetPrefabGUID(rootPrefab_GUID);
                
                // copy previous level data from latest
                if (latestDataCreationTime > 0) {
                    LevelData_SO latest_SO = AssetDatabase.LoadAssetAtPath<LevelData_SO>(latestDataPath);
                    if (latest_SO != null) levelData_SO.CopySettingsFrom(latest_SO);
                }
                
                AssetDatabase.CreateAsset(levelData_SO, rootPrefabDir + rootPrefabName + ".LevelData.asset");
                
                var lockStatus = locked;
                locked = false;
                Selection.activeGameObject = selection;
                OnSelectionChange();
                locked = lockStatus;
            }
        }
    #endregion
    }
}