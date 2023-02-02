using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GistLevelDesigner {
    public class WallWing : SceneObject, IManagedObject<WingData> {
        private static Vector3 manipulatorPositionShift = Vector3.zero;
        private VerticesMeshHandleDesc growHandleDesc = HandlesStyles.wallGrowHandleDesc;
        
        private readonly Wall wall;
        private Vector3       growDirection;
        private GameObject[]  chain;
        private GameObject    cap;
        private Quaternion    growRotation;
        private Vector3       wallLineStart;
        private Vector3       wallLineEnd;
        private int           lastCapCycled;
        private bool          drawLineFirst;
        
        public  WallWing(Wall wall, Vector3 growDirection, GameObject capPrefab) : base(null) {
            this.wall = wall;
            this.growDirection = growDirection;
            chain = new GameObject[0];
            Init();
            InstantiateCap(capPrefab);
        }
        private WallWing(Wall wall, GameObject[] chain, Vector3 growDirection, GameObject cap) : base(null) {
            this.wall = wall;
            this.chain = chain;
            this.cap = cap;
            this.growDirection = growDirection;
            Init();
        }
        private void Init() {
            lastCapCycled = -2;
            growRotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.back, growDirection, Vector3.up), Vector3.up);
            UpdateManipulators();
        }
        private void InstantiateCap(GameObject capPrefab) {
            if (capPrefab != null) {
                cap = (GameObject)PrefabUtility.InstantiatePrefab(capPrefab, wall.gameObject.transform);
                cap.name = capPrefab.name;
                cap.transform.localRotation = growRotation;
                UpdateCapPosition();
            } else {
                cap = null;
            }
        }
        
        public  static WallWing TryPickUp(Wall wall, Dictionary<long,GameObject> wallObjects, WingData wingData) {
            List<GameObject> pickedChain = new List<GameObject>(wingData.chainFileIDs.Length);
            for (int i = 0; i < wingData.chainFileIDs.Length; i++) {
                if (wallObjects.TryGetValue(wingData.chainFileIDs[i], out GameObject chainEntry)) {
                    pickedChain.Add(chainEntry);
                }
            }
            GameObject capGO;
            if (wingData.capFileID != -1) {
                wallObjects.TryGetValue(wingData.capFileID, out capGO);
            } else {
                capGO = null;
            }
            
            return new WallWing(wall, pickedChain.ToArray(), wingData.growDirection, capGO);
        }
        public  static float GetManipulatorsHeight() {
            return manipulatorPositionShift.y;
        }
        public  static void SetManipulatorsHeight(float height) {
            manipulatorPositionShift.y = height;
        }
        
        private void     UpdateCapPosition() {
            if (cap != null) {
                cap.transform.localPosition = DesiredCapPosition();
            }
        }
        private Vector3  DesiredCapPosition() {
            return growDirection * Wall.GetWallLength() * chain.Length;
        }
        public  WingData GatherData(string rootPrefabPath) {
            long[] chainFileIDs = new long[chain.Length];
            for (int i = 0; i < chain.Length; i++) {
                var chainOriginal = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(chain[i], rootPrefabPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(chainOriginal, out string _, out long chainLocalFileID);
                chainFileIDs[i] = chainLocalFileID;
            }
            long capLocalFileID;
            if (cap != null) {
                var capOriginal = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(cap, rootPrefabPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(capOriginal, out string _, out capLocalFileID);
            } else {
                capLocalFileID = -1;
            }
            return new WingData(growDirection, chainFileIDs, capLocalFileID);
        }
        
        public  void     UpdateManipulators() {
            UpdateWallLinePositions();
            Vector3 cameraPosition = SceneObject.GetSampledCameraPosition();
            if ((cameraPosition - wallLineEnd).sqrMagnitude > (cameraPosition - wallLineStart).sqrMagnitude) {
                drawLineFirst = false;
            } else {
                drawLineFirst = true;
            }
        }
        public  void     UpdateWallLinePositions() {
            if (wall == null || wall.gameObject == null) return;
            
            Vector3 wallPosition = wall.gameObject.transform.position;
            Matrix4x4 wallLocalToWorld = wall.gameObject.transform.localToWorldMatrix;
            wallLineStart = wallPosition + wallLocalToWorld.MultiplyVector(manipulatorPositionShift);
            wallLineEnd = wallLineStart + wallLocalToWorld.MultiplyVector(GetWallLineEndLocalPosition());
        }
        public  Vector3  GetWallLineEndPosition() {
            return wallLineEnd;
        }
        private Vector3  GetWallLineEndLocalPosition() {
            return growDirection * Wall.GetWallLength() * (chain.Length + 0.5f);
        }
        
        public  override void SceneGUI() {
            if (drawLineFirst) {
                DrawWallLine();
                DrawGrowHandle();
            } else {
                DrawGrowHandle();
                DrawWallLine();
            }
        }
        private          void DrawWallLine() {
            if (!LD_Window.VisibilitySettings().wingsLines) return;
            Handles.color = HandlesStyles.wallLineColor;
            Handles.DrawLine(wallLineStart, wallLineEnd);
        }
        private          void DrawGrowHandle() {
            if (!LD_Window.VisibilitySettings().wingsGrowth) return;
            growHandleDesc.general.position = GetWallLineEndLocalPosition() + manipulatorPositionShift;
            growHandleDesc.vertsLocalToWorld = wall.gameObject.transform.localToWorldMatrix;
            growHandleDesc.verticesRotation = growRotation;
            if (CustomHandles.DragHandle(ref growHandleDesc, ref eventInfo)) CheckWallChange();
            if (eventInfo.affected) {
                if (eventInfo.eventType == HandleEventInfo.Type.CLICK && eventInfo.eventButton == 0) CapCycle();
                if (eventInfo.eventType == HandleEventInfo.Type.PRESS && eventInfo.eventButton == 1) {
                    GenericMenu menu = new GenericMenu();
                    GameObject[] wall_Caps = LD_Window.GetActiveMainSet().wall_Caps;
                    if (wall_Caps != null && wall_Caps.Length > 0) {
                        for (int i = 0; i < wall_Caps.Length; i++) {
                            if (wall_Caps[i] == null) continue;
                            GameObject wallCap = wall_Caps[i];
                            bool current = cap != null && cap.name == wallCap.name;
                            menu.AddItem(new GUIContent(wallCap.name), current, () => {RemoveWingGameObject(wall.gameObject, cap); InstantiateCap(wallCap); lastCapCycled = -2;});
                        }
                    }
                    menu.AddItem(new GUIContent("<none>"), cap == null, () => {RemoveWingGameObject(wall.gameObject, cap); cap = null; lastCapCycled = -2;});
                    menu.AddSeparator(null);
                    menu.AddItem(new GUIContent("Remove Wall"), false, () => {LD_Window.Remove(wall);});
                    menu.ShowAsContext();
                }
            }
        }
        
        private          void CapCycle() {
            GameObject[] wall_Caps = LD_Window.GetActiveMainSet().wall_Caps;
            if (wall_Caps == null || wall_Caps.Length == 0) return;
            lastCapCycled++;
            if (cap == null) {
                if (lastCapCycled == -1) lastCapCycled++;
                if (lastCapCycled < wall_Caps.Length && wall_Caps[lastCapCycled] == null) lastCapCycled++;
            }
            if (lastCapCycled >= wall_Caps.Length) lastCapCycled = -1;
            RemoveWingGameObject(wall.gameObject, cap);
            GameObject newCap = lastCapCycled >= 0 ? wall_Caps[lastCapCycled] : null;
            InstantiateCap(newCap);
        }
        private          void CheckWallChange() {
            Ray growHandleRay = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(growHandleDesc.general.position));
            Plane wallLinesPlane = new Plane(wall.gameObject.transform.localToWorldMatrix.MultiplyVector(Vector3.up), wallLineStart);
            if (wallLinesPlane.Raycast(growHandleRay, out float distance)) {
                Vector3 pointing = growHandleRay.GetPoint(distance);
                
                Vector3 growDirectionWorld = wallLineEnd - wallLineStart;
                Vector3 nearestPointOnWallLine = Utility.NearestPointOnLine(wallLineStart, growDirectionWorld, pointing);
                
                CustomHandles.SetHotPosition(nearestPointOnWallLine);
                
                Vector3 change = nearestPointOnWallLine - wallLineEnd;
                if (change.magnitude < 0.7f * Wall.GetWallLength()) return;
                
                bool grow = Vector3.Dot(change, growDirectionWorld) >= 0;
                if (grow) {
                    var wallsSet = LD_Window.GetActiveMainSet();
                    if (wallsSet == null) return;
                    GameObject wallPrefab = wallsSet.wall_I;
                    if (wallPrefab == null) return;
                    
                    GameObject addedWall = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab, wall.gameObject.transform);
                    Vector3 addPosition = growDirection * (chain.Length + 1) * Wall.GetWallLength();
                    addedWall.transform.localRotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, growDirection, Vector3.up), Vector3.up);
                    addedWall.transform.localPosition = addPosition;
                    addedWall.name = wallPrefab.name;
                    
                    ArrayUtility.Add(ref chain, addedWall);
                    UpdateManipulators();
                    UpdateCapPosition();
                    CheckWallChange();
                } else {
                    if (chain.Length > 0) {
                        float changeMagnitude = change.magnitude;
                        int wantsRemoveCount = (int)(changeMagnitude / Wall.GetWallLength());
                        if (changeMagnitude % Wall.GetWallLength() > 0.7f * Wall.GetWallLength()) wantsRemoveCount++;
                        if (wantsRemoveCount > chain.Length) wantsRemoveCount = chain.Length;
                        
                        var thrownObjects = new List<GameObject>(wantsRemoveCount);
                        for (int i = 0; i < wantsRemoveCount; i++) thrownObjects.Add(chain[chain.Length - 1 - i]);
                        var newChain = new GameObject[chain.Length - thrownObjects.Count];
                        for (int i = 0; i < newChain.Length; i++) newChain[i] = chain[i];
                        chain = newChain;
                        Utility.RemoveChildChildGameObjectsFromPrefab(wall.gameObject, thrownObjects, LD_Window.GetActiveRootPrefabPath());
                        UpdateManipulators();
                        UpdateCapPosition();
                    }
                }
            }
        }
        public  override bool IsGameObjectManaged(GameObject gameObject) {
            if (gameObject == cap) return true;
            for (int i = 0; i < chain.Length; i++) if (chain[i] == gameObject) return true;
            return false;
        }
        public override bool CheckAndFixUnexpectedChangesAfterUndo() {
            var unexpectedChanges = false;
            if (cap != null) {
                if (cap.transform.localPosition != DesiredCapPosition()) {
                    UpdateCapPosition();
                    unexpectedChanges = true;
                }
            }
            
            return unexpectedChanges;
        }
        private static   void RemoveWingGameObject(GameObject wallGameObject, GameObject deletingChild) {
            if (deletingChild == null) return;
            if (PrefabUtility.IsAddedGameObjectOverride(deletingChild)) {
                GameObject.DestroyImmediate(deletingChild, false);
            } else {
                Utility.DestroyAddedOverridesInside(deletingChild);
                string rootPrefabPath = LD_Window.GetActiveRootPrefabPath();
                var rootGO = PrefabUtility.LoadPrefabContents(rootPrefabPath);
                GameObject deletingObject = rootGO.transform.GetChild(wallGameObject.transform.GetSiblingIndex()).GetChild(deletingChild.transform.GetSiblingIndex()).gameObject;
                GameObject.DestroyImmediate(deletingObject, false);
                PrefabUtility.SaveAsPrefabAsset(rootGO, rootPrefabPath);
                PrefabUtility.UnloadPrefabContents(rootGO);
            }
        }
    }
    
    [Serializable]
    public class WingData : ManagedObjectData<WingData> {
        public Vector3 growDirection;
        public long[]  chainFileIDs;
        public long    capFileID;

        public WingData(Vector3 growDirection, long[] chainFileIDs, long capFileID) {
            this.growDirection = growDirection;
            this.chainFileIDs = chainFileIDs;
            this.capFileID = capFileID;
        }
        public WingData(WingData other) {
            this.growDirection = other.growDirection;
            this.chainFileIDs = other.chainFileIDs == null ? null : (long[])other.chainFileIDs.Clone();
            this.capFileID = other.capFileID;
        }
        public override WingData Clone() {
            return new WingData(this);
        }
        public override bool Equals(WingData other) {
            if (!capFileID.Equals(other.capFileID)) return false;
            if (!growDirection.Equals(other.growDirection)) return false;
            // if (!chainFileIDs.SequenceEqual(other.chainFileIDs)) return false;
            if (!Utility.ValueTypeSequenceEqual(chainFileIDs, other.chainFileIDs)) return false;
            return true;
        }
    }
}