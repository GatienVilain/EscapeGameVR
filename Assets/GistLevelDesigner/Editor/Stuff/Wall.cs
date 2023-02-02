using System;
using System.Collections.Generic;
using UnityEngine;

namespace GistLevelDesigner {
    public class Wall : SimpleObject, IManagedObject<WallData> {
        private VerticesMeshHandleDesc wallMoveHandleDesc = HandlesStyles.wallMoveHandleDesc;
        private CircleHandleDesc       wallRotationHandleDesc = HandlesStyles.wallRotationHandleDesc;

        private readonly WallType   type;
        private readonly WallWing[] wings;
        private readonly WallWing[] sortedWings;
        
        public                 Wall(GameObject wallGameObject, WallType type, GameObject wallCapPrefab) : base(wallGameObject) {
            this.ignoreObjectsVisibility = true;
            this.type = type;
            List<WallWing> wingsList = new List<WallWing>(4);
            switch (type) {
                case WallType.WALL_I:
                    wingsList.Add(CreateWing(Vector3.forward, wallCapPrefab));
                    wingsList.Add(CreateWing(Vector3.back, wallCapPrefab));
                    break;
                case WallType.WALL_L:
                    wingsList.Add(CreateWing(Vector3.right, wallCapPrefab));
                    wingsList.Add(CreateWing(Vector3.back, wallCapPrefab));
                    break;
                case WallType.WALL_T:
                    wingsList.Add(CreateWing(Vector3.right, wallCapPrefab));
                    wingsList.Add(CreateWing(Vector3.back, wallCapPrefab));
                    wingsList.Add(CreateWing(Vector3.left, wallCapPrefab));
                    break;
                case WallType.WALL_X:
                    wingsList.Add(CreateWing(Vector3.right, wallCapPrefab));
                    wingsList.Add(CreateWing(Vector3.back, wallCapPrefab));
                    wingsList.Add(CreateWing(Vector3.left, wallCapPrefab));
                    wingsList.Add(CreateWing(Vector3.forward, wallCapPrefab));
                    break;
            }
            wings = wingsList.ToArray();
            sortedWings = (WallWing[])wings.Clone();
        }
        public                 Wall(GameObject wallGameObject, WallData wallData, string rootPrefabPath) : base(wallGameObject) {
            this.ignoreObjectsVisibility = true;
            var wallObjects = Utility.GetChildrenListWithFileIDs(wallGameObject, rootPrefabPath);
            
            this.type = wallData.type;
            wings = new WallWing[wallData.wingsData.Length];
            for (int i = 0; i < wings.Length; i++) {
                wings[i] = WallWing.TryPickUp(this, wallObjects, wallData.wingsData[i]);
            }
            sortedWings = (WallWing[])wings.Clone();
        }
        public new    WallData GatherData(string rootPrefabPath) {
            var simpleObjectData = base.GatherData(rootPrefabPath);
            if (simpleObjectData == null) return null;
            // var wallInRoot = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(gameObject, rootPrefabPath);
            // AssetDatabase.TryGetGUIDAndLocalFileIdentifier(wallInRoot, out string guid, out long localFileID);
            
            WingData[] wingsData = new WingData[wings.Length];
            for (int i = 0; i < wings.Length; i++) wingsData[i] = wings[i].GatherData(rootPrefabPath);
            return new WallData(simpleObjectData, type, wingsData);
        }
        private       WallWing CreateWing(Vector3 growDirection, GameObject capPrefab) {
            var wing = new WallWing(this, growDirection, capPrefab);
            
            return wing;
        }
        public static Wall     TryPickUp(WallData wallData, Dictionary<long,GameObject> possibleWallsList, string rootPrefabPath) {
            if (possibleWallsList.TryGetValue(wallData.simpleObjectData.fileID, out GameObject gameObject)) {
                return new Wall(gameObject, wallData, rootPrefabPath);
            }
            return null;
        }
        
        public static     float GetWallLength() {
            var mainSet = LD_Window.GetActiveMainSet();
            if (mainSet != null) {
                return mainSet.wallLength <= 0 ? 1f : mainSet.wallLength;
            } else return 1f;
        }
        public static new float GetMoveSnap() {
            return GetWallLength();
        }
        public override   float MoveSnapValue() {
            return GetWallLength();
        }
        public override   float RotationSnapValue() {
            return 22.5f;
        }
        
        public override void SceneGUI() {
            var visibility = LD_Window.VisibilitySettings();
            if (!visibility.walls) return;
            
            if (visibility.wallsMovement) MoveManipulator(ref wallMoveHandleDesc);
            if (visibility.wallsRotation) RotationManipulator(ref wallRotationHandleDesc);
            
            if (!visibility.wings) return;
            
            if (modified) {
                for (int i = 0; i < sortedWings.Length; i++) sortedWings[i].UpdateManipulators();
                modified = false;
            }
            for (int i = 0; i < sortedWings.Length; i++) sortedWings[i].SceneGUI();
        }
        public          void UpdateWallLinePositions() {
            for (int i = 0; i < wings.Length; i++) wings[i].UpdateWallLinePositions();
        }
        public override void SortObjects() {
            Vector3 cameraPosition = SceneObject.cameraPosition;
            
            for (int i = 0; i < sortedWings.Length; i++) sortedWings[i].UpdateWallLinePositions();
            Vector3 farthest = Vector3.positiveInfinity;
            int farthestI;
            for (int i = 0; i < sortedWings.Length - 1; i++) {
                farthestI = i;
                farthest = cameraPosition - sortedWings[i].GetWallLineEndPosition();
                for (int j = i + 1; j < sortedWings.Length; j++) {
                    Vector3 camVector = cameraPosition - sortedWings[j].GetWallLineEndPosition();
                    if (camVector.sqrMagnitude > farthest.sqrMagnitude) {
                        farthestI = j;
                        farthest = camVector;
                    }
                }
                if (farthestI != i) {
                    WallWing tempWing = sortedWings[i];
                    sortedWings[i] = sortedWings[farthestI];
                    sortedWings[farthestI] = tempWing;
                }
            }
        }
        public override bool IsGameObjectManaged(GameObject gameObject) {
            if (base.IsGameObjectManaged(gameObject)) return true;
            for (int i = 0; i < wings.Length; i++) if (wings[i].IsGameObjectManaged(gameObject)) return true;
            return false;
        }
        public override void UpdateSceneGUIdrawingData() {
            UpdateWallLinePositions();
        }
        public override bool CheckAndFixUnexpectedChangesAfterUndo() {
            var unexpectedChanges = false;
            for (int i = 0; i < wings.Length; i++) {
                if (wings[i].CheckAndFixUnexpectedChangesAfterUndo()) unexpectedChanges = true;
            }
            
            return unexpectedChanges;
        }

    }
    
    [Serializable]
    public class WallData : ManagedObjectData<WallData> {
        public SimpleObjectData simpleObjectData;
        public WallType         type;
        public WingData[]       wingsData;
        
        public WallData(SimpleObjectData simpleObjectData, WallType type, WingData[] wingsData) {
            this.simpleObjectData = simpleObjectData;
            this.type = type;
            this.wingsData = wingsData;
        }
        public WallData(WallData other) {
            this.simpleObjectData = other.simpleObjectData?.Clone();
            this.type = other.type;
            if (other.wingsData == null) {
                this.wingsData = null;
            } else {
                this.wingsData = new WingData[other.wingsData.Length];
                for (int i = 0; i < other.wingsData.Length; i++) this.wingsData[i] = other.wingsData[i].Clone();
            }
        }
        public override WallData Clone() {
            return new WallData(this);
        }
        public override bool Equals(WallData other) {
            if (!ReferenceEquals(simpleObjectData, other.simpleObjectData) && !simpleObjectData.Equals(other.simpleObjectData)) return false;
            if (!type.Equals(other.type)) return false;
            // if (!wingsData.SequenceEqual(other.wingsData)) return false;
            if (!Utility.ManagedObjectDataArrayEquals(wingsData, other.wingsData)) return false;
            return true;
        }
    }
}