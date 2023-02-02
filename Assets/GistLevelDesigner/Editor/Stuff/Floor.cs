using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace GistLevelDesigner {
    public class Floor : SimpleObject, IManagedObject<FloorData> {
        private VerticesMeshHandleDesc floorMoveDesc = HandlesStyles.floorMoveHandleDesc;
        private CircleHandleDesc       floorRotationDesc = HandlesStyles.floorRotationHandleDesc;
        
        private Vector3[] rowSideHandleVertices;
        private Vector3[] colSideHandleVertices;
        private VerticesMeshHandleDesc[] sideHandles;
        private Vector3[] sideHandlesPositions;
        
        private readonly float  size;
        private readonly string prefabGUID;
        private GameObject[,]   tiles;
        private int             startingRow;
        private int             startingCol;
        
        public                  Floor(GameObject gameObject, FloorEntry floorEntry) : base(gameObject) {
            ignoreObjectsVisibility = true;
            size = floorEntry.size;
            prefabGUID = null;
            tiles = null;
            startingRow = 0;
            startingCol = 0;
            if (floorEntry.floorPrefab != null) {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(floorEntry.floorPrefab, out string guid, out long _)) {
                    prefabGUID = guid;
                    var firstTile = (GameObject)PrefabUtility.InstantiatePrefab(floorEntry.floorPrefab, gameObject.transform);
                    firstTile.name = floorEntry.floorPrefab.name;
                    firstTile.transform.localPosition = Vector3.zero;
                    firstTile.transform.localRotation = Quaternion.identity;
                    
                    tiles = new GameObject[1,1];
                    tiles[0,0] = firstTile;
                }
            }
            if (tiles == null) tiles = new GameObject[0,0];
            InitHandles();
        }
        public                  Floor(GameObject gameObject, FloorData floorData, string rootPrefabPath) : base(gameObject) {
            this.ignoreObjectsVisibility = true;
            size = floorData.size;
            prefabGUID = floorData.prefabGUID;
            
            if (floorData.tilesFileIDs != null && floorData.tilesFileIDs.Length > 0 && floorData.tilesFileIDsWidth > 0) {
                var floorChildsDict = new Dictionary<long,GameObject>(gameObject.transform.childCount);
                for (int i = 0; i < gameObject.transform.childCount; i++) {
                    var floorChild = gameObject.transform.GetChild(i).gameObject;
                    var floorChildInRoot = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(floorChild, rootPrefabPath);
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(floorChildInRoot, out string _, out long fileID)) {
                        floorChildsDict.Add(fileID, floorChild);
                    }
                }
                int colsNum = floorData.tilesFileIDsWidth;
                int rowsNum = floorData.tilesFileIDs.Length / colsNum;
                tiles = new GameObject[rowsNum, colsNum];
                for (int r = 0; r < rowsNum; r++) {
                    int rowOffset = r * colsNum;
                    for (int c = 0; c < colsNum; c++) {
                        if (floorChildsDict.TryGetValue(floorData.tilesFileIDs[rowOffset + c], out GameObject floorChildGameObject)) {
                            tiles[r,c] = floorChildGameObject;
                        } else {
                            tiles[r,c] = null;
                        }
                    }
                }
            } else {
                tiles = new GameObject[0,0];
            }
            
            startingRow = floorData.startingRow;
            startingCol = floorData.startingCol;
            InitHandles();
        }
        public new    FloorData GatherData(string rootPrefabPath) {
            var simpleObjectData = base.GatherData(rootPrefabPath);
            if (simpleObjectData == null) return null;
            
            int rowsNum = tiles.GetLength(0);
            int colsNum = tiles.GetLength(1);
            var tilesFileIDs = new long[rowsNum * colsNum];
            for (int r = 0; r < rowsNum; r++) {
                int rowOffset = r * colsNum;
                for (int c = 0; c < colsNum; c++) {
                    var tileInRoot = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(tiles[r,c], rootPrefabPath);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tileInRoot, out string _, out long tileFileID);
                    tilesFileIDs[rowOffset + c] = tileFileID;
                }
            }
            
            return new FloorData(simpleObjectData, size, prefabGUID, tilesFileIDs, colsNum, startingRow, startingCol);
        }
        public static Floor     TryPickUp(FloorData floorData, Dictionary<long,GameObject> possibleFloorsList, string rootPrefabPath) {
            if (possibleFloorsList.TryGetValue(floorData.simpleObjectData.fileID, out GameObject gameObject)) {
                return new Floor(gameObject, floorData, rootPrefabPath);
            }
            return null;
        }
        

        public static new float GetMoveSnap() {
            return LD_Window.OptionsSettings().moveSnap;
        }
        
        private void          InitHandles() {
            rowSideHandleVertices = new Vector3[4];
            colSideHandleVertices = new Vector3[4];
            sideHandlesPositions = new Vector3[4];
            sideHandles = new VerticesMeshHandleDesc[4];
            for (int i = 0; i < sideHandles.Length; i++) sideHandles[i] = HandlesStyles.floorObjectSideHandleDesc;
            sideHandles[0].vertices = rowSideHandleVertices;
            sideHandles[0].verticesRotation = Quaternion.Euler(0, 0, 0);
            sideHandles[1].vertices = colSideHandleVertices;
            sideHandles[1].verticesRotation = Quaternion.Euler(0, 90, 0);
            sideHandles[2].vertices = rowSideHandleVertices;
            sideHandles[2].verticesRotation = Quaternion.Euler(0, 180, 0);
            sideHandles[3].vertices = colSideHandleVertices;
            sideHandles[3].verticesRotation = Quaternion.Euler(0, 270, 0);
            UpdateHandles();
        }
        private void          UpdateHandles() {
            int rowsNum = tiles.GetLength(0);
            int colsNum = tiles.GetLength(1);
            float rowHandleSize = colsNum * size * 0.25f;
            float colHandleSize = rowsNum * size * 0.25f;
            const float sideHandleThin = 0.2f;
            
            rowSideHandleVertices[0] = new Vector3(-rowHandleSize, 0, 0);
            rowSideHandleVertices[1] = new Vector3( rowHandleSize, 0, 0);
            rowSideHandleVertices[2] = new Vector3( rowHandleSize -sideHandleThin, 0, -sideHandleThin);
            rowSideHandleVertices[3] = new Vector3(-rowHandleSize +sideHandleThin, 0, -sideHandleThin);
            
            colSideHandleVertices[0] = new Vector3(-colHandleSize, 0, 0);
            colSideHandleVertices[1] = new Vector3( colHandleSize, 0, 0);
            colSideHandleVertices[2] = new Vector3( colHandleSize -sideHandleThin, 0, -sideHandleThin);
            colSideHandleVertices[3] = new Vector3(-colHandleSize +sideHandleThin, 0, -sideHandleThin);
            
            sideHandlesPositions[0] = new Vector3((startingCol - (colsNum - 1) * 0.5f) * size, 0, (startingRow + 0.5f) * size);
            sideHandlesPositions[1] = new Vector3((startingCol + 0.5f) * size, 0, ((startingRow - (rowsNum - 1) * 0.5f)) * size);
            sideHandlesPositions[2] = new Vector3((startingCol - (colsNum - 1) * 0.5f) * size, 0, (startingRow - rowsNum + 0.5f) * size);
            sideHandlesPositions[3] = new Vector3((startingCol - colsNum + 0.5f) * size, 0, ((startingRow - (rowsNum - 1) * 0.5f)) * size);
        }
        public  override void SceneGUI() {
            if (!LD_Window.VisibilitySettings().floors) return;

            MoveManipulator(ref floorMoveDesc);
            RotationManipulator(ref floorRotationDesc);
            
            if (size <= 0) return;
            
            Matrix4x4 localToWorld = gameObject.transform.localToWorldMatrix;
            
            for (int i = 0; i < sideHandles.Length; i++) {
                floorMoveDesc.general.position = sideHandlesPositions[i];
                sideHandles[i].vertsLocalToWorld = localToWorld;
                if (CustomHandles.DragHandle(ref sideHandles[i], ref eventInfo)) CheckFloorChange(i);
            }
            
        }
        
        private void          CheckFloorChange(int handleI) {
            Ray handleRay = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(floorMoveDesc.general.position));
            Matrix4x4 floorLocalToWorld = gameObject.transform.localToWorldMatrix;
            Plane floorPlane = new Plane(floorLocalToWorld.MultiplyVector(Vector3.up), gameObject.transform.position);
            if (floorPlane.Raycast(handleRay, out float distance)) {
                Vector3 pointingWorld = handleRay.GetPoint(distance);
                Vector3 sideDirection = floorLocalToWorld.MultiplyVector(Quaternion.Euler(0, 90 * handleI, 0) * Vector3.forward);
                Vector3 startingPosition = floorLocalToWorld.MultiplyPoint3x4(sideHandlesPositions[handleI]);
                Vector3 nearestPointOnDirection = Utility.NearestPointOnLine(startingPosition, sideDirection, pointingWorld);
                
                CustomHandles.SetHotPosition(nearestPointOnDirection);
                
                Vector3 change = nearestPointOnDirection - startingPosition;
                if (change.magnitude < 0.7f * size) return;
                
                var tilePrefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
                if (string.IsNullOrEmpty(tilePrefabPath)) return;
                var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(tilePrefabPath);
                if (tilePrefab == null) return;
                
                bool manipulateRow = handleI == 0 || handleI == 2;
                int rowsNum = tiles.GetLength(0);
                int colsNum = tiles.GetLength(1);
                
                float changeMagnitude = change.magnitude;
                int changeCount = (int)(changeMagnitude / size);
                if (changeMagnitude % size > 0.7f * size) changeCount++;
                int changeRowMin = -1;
                int changeRowMax = -1;
                int changeColMin = -1;
                int changeColMax = -1;
                int newRowsNum = rowsNum;
                int newColsNum = colsNum;
                
                bool adding = Vector3.Dot(change, sideDirection) >= 0;
                
                if (manipulateRow) {
                    newRowsNum += adding ? changeCount : -changeCount;
                    if (!adding && newRowsNum < 1) return;
                    if (handleI == 0) {
                        changeRowMin = 0;
                        changeRowMax = changeRowMin + changeCount - 1;
                        startingRow += adding ? changeCount : -changeCount;
                    } else {
                        if (adding) {
                            changeRowMin = rowsNum;
                            changeRowMax = changeRowMin + changeCount - 1;
                        } else {
                            changeRowMax = rowsNum - 1;
                            changeRowMin = changeRowMax - changeCount + 1;
                        }
                    }
                } else {
                    newColsNum += adding ? changeCount : -changeCount;
                    if (!adding && newColsNum < 1) return;
                    if (handleI == 1) {
                        changeColMin = 0;
                        changeColMax = changeColMin + changeCount - 1;
                        startingCol += adding ? changeCount : -changeCount;
                    } else {
                        if (adding) {
                            changeColMin = colsNum;
                            changeColMax = changeColMin + changeCount - 1;
                        } else {
                            changeColMax = colsNum - 1;
                            changeColMin = changeColMax - changeCount + 1;
                        }
                    }
                }

                GameObject[,] newTiles;
                if (adding) {
                    newTiles = new GameObject[newRowsNum,newColsNum];
                    int rowOffset = 0;
                    for (int r = 0; r < newRowsNum; r++) {
                        if (r >= changeRowMin && r <= changeRowMax) rowOffset++;
                        int colOffset = 0;
                        for (int c = 0; c < newColsNum; c++) {
                            if (c >= changeColMin && c <= changeColMax) colOffset++;
                            if ((r >= changeRowMin && r <= changeRowMax) || (c >= changeColMin && c <= changeColMax)) {
                                // add new tile
                                var addedTile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, gameObject.transform);
                                addedTile.name = tilePrefab.name;
                                addedTile.transform.localPosition = new Vector3((startingCol - c) * size, 0, (startingRow - r) * size);
                                addedTile.transform.localRotation = Quaternion.identity;
                                newTiles[r,c] = addedTile;
                            } else {
                                // copy tile
                                newTiles[r,c] = tiles[r-rowOffset,c-colOffset];
                            }
                        }
                    }
                    tiles = newTiles;
                } else {
                    newTiles = new GameObject[newRowsNum,newColsNum];
                    var thrownTiles = new List<GameObject>();
                    int rowOffset = 0;
                    for (int r = 0; r < rowsNum; r++) {
                        if (r >= changeRowMin && r <= changeRowMax) rowOffset++;
                        int colOffset = 0;
                        for (int c = 0; c < colsNum; c++) {
                            if (c >= changeColMin && c <= changeColMax) colOffset++;
                            if ((r >= changeRowMin && r <= changeRowMax) || (c >= changeColMin && c <= changeColMax)) {
                                thrownTiles.Add(tiles[r,c]);
                            } else {
                                newTiles[r-rowOffset,c-colOffset] = tiles[r,c];
                            }
                        }
                    }
                    tiles = newTiles;
                    Utility.RemoveChildChildGameObjectsFromPrefab(gameObject, thrownTiles, LD_Window.GetActiveRootPrefabPath());
                }
                
                UpdateHandles();
            }
        }
        public  override bool IsGameObjectManaged(GameObject gameObject) {
            if (base.IsGameObjectManaged(gameObject)) return true;
            if (tiles != null) {
                var rowsNum = tiles.GetLength(0);
                var columnsNum = tiles.GetLength(1);
                for (int i = 0; i < rowsNum; i++) {
                    for (int j = 0; j < columnsNum; j++) {
                        if (tiles[i,j] == gameObject) return true;
                    }
                }
            }
            return false;
        }
        
    }
    
    [Serializable]
    public class FloorData : ManagedObjectData<FloorData> {
        public SimpleObjectData simpleObjectData;
        public string           prefabGUID;
        public float            size;
        public int              tilesFileIDsWidth;
        public int              startingRow;
        public int              startingCol;
        public long[]           tilesFileIDs;
        
        public FloorData(SimpleObjectData simpleObjectData, float size, string prefabGUID, long[] tilesFileIDs, int tilesFileIDsWidth, int startingRow, int startingColumn) {
            this.simpleObjectData = simpleObjectData;
            this.prefabGUID = prefabGUID;
            this.size = size;
            this.tilesFileIDsWidth = tilesFileIDsWidth;
            this.startingRow = startingRow;
            this.startingCol = startingColumn;
            this.tilesFileIDs = tilesFileIDs;
        }
        public FloorData(FloorData other) {
            this.simpleObjectData = other.simpleObjectData?.Clone();
            this.prefabGUID = other.prefabGUID;
            this.size = other.size;
            this.tilesFileIDsWidth = other.tilesFileIDsWidth;
            this.startingRow = other.startingRow;
            this.startingCol = other.startingCol;
            this.tilesFileIDs = other.tilesFileIDs == null ? null : (long[])other.tilesFileIDs.Clone();
        }
        public override FloorData Clone() {
            return new FloorData(this);
        }
        public override bool Equals(FloorData other) {
            if (!ReferenceEquals(simpleObjectData, other.simpleObjectData) && !simpleObjectData.Equals(other.simpleObjectData)) return false;
            if (!string.Equals(prefabGUID, other.prefabGUID)) return false;
            if (!size.Equals(other.size)) return false;
            if (!tilesFileIDsWidth.Equals(other.tilesFileIDsWidth)) return false;
            if (!startingRow.Equals(other.startingRow)) return false;
            if (!startingCol.Equals(other.startingCol)) return false;
            if (!Utility.ValueTypeSequenceEqual(tilesFileIDs, other.tilesFileIDs)) return false;
            return true;
        }
    }
}