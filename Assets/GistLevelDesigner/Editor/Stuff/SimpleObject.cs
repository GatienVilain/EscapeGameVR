using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace GistLevelDesigner {
    public class SimpleObject : SceneObject, IManagedObject<SimpleObjectData> {
        protected VerticesMeshHandleDesc moveHandleDesc     = HandlesStyles.simpleObjectMoveHandleDesc;
        protected CircleHandleDesc       rotationHandleDesc = HandlesStyles.simpleObjectRotationHandleDesc;
        
        protected bool ignoreObjectsVisibility;
        
        public SimpleObject(GameObject gameObject) : base(gameObject) {}
        
        public    override void SceneGUI() {
            if (!ignoreObjectsVisibility && !LD_Window.VisibilitySettings().objects) return;
            if (modified) modified = false;
            
            MoveManipulator(ref moveHandleDesc);
            RotationManipulator(ref rotationHandleDesc);
        }
        protected          void MoveManipulator(ref VerticesMeshHandleDesc moveHandleDesc) {
            if (!gameObject.activeInHierarchy) return;
            
            if (LD_Window.VisibilitySettings().movement) {
                moveHandleDesc.general.position = Vector3.zero;
                moveHandleDesc.vertsLocalToWorld = gameObject.transform.localToWorldMatrix;
                if (CustomHandles.DragHandle(ref moveHandleDesc, ref eventInfo)) {
                    CheckPositionChange(ref moveHandleDesc);
                }
                if (eventInfo.affected) {
                    if (eventInfo.eventType == HandleEventInfo.Type.PRESS && eventInfo.eventButton == 1) {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove"), false, () => {LD_Window.Remove(this);});
                        menu.ShowAsContext();
                    }
                }
            }
        }
        protected          void RotationManipulator(ref CircleHandleDesc rotationHandleDesc) {
            if (!gameObject.activeInHierarchy) return;
            
            if (LD_Window.VisibilitySettings().rotation) {
                rotationHandleDesc.normal = rootUpWorld;
                rotationHandleDesc.general.position = gameObject.transform.position;
                if (CustomHandles.RotationHandle(ref rotationHandleDesc)) CheckRotationChange(ref rotationHandleDesc);
            }
        }
        private            void CheckPositionChange(ref VerticesMeshHandleDesc moveHandleDesc) {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(moveHandleDesc.general.position));
            if (floorPlane.Raycast(mouseRay, out float distance)) {
                Vector3 current = gameObject.transform.localPosition;
                Vector3 pointing = mouseRay.GetPoint(distance);
                Vector3 resulting = current;

                CustomHandles.SetHotPosition(pointing);
                pointing = worldToRootMatrix.MultiplyPoint3x4(pointing);
                var moveSnap = MoveSnapValue();
                if (moveSnap > 0) {
                    Vector3 targeting = new Vector3(Mathf.Round(pointing.x / moveSnap) * moveSnap, 0, Mathf.Round(pointing.z / moveSnap) * moveSnap);
                    
                    if ((targeting - current).sqrMagnitude > 0.1f * moveSnap && ((pointing - targeting).magnitude < 0.4f * moveSnap || (pointing - current).sqrMagnitude > 1.4f * moveSnap)) {
                        resulting = targeting;
                    }
                } else resulting = pointing;
                
                if (resulting != current) {
                    Undo.RecordObject(gameObject.transform, "Move " + gameObject.name);
                    gameObject.transform.localPosition = resulting;
                    EditorUtility.SetDirty(gameObject.transform);
                    modified = true;
                }
            }
        }
        private            void CheckRotationChange(ref CircleHandleDesc rotationHandleDesc) {
            Quaternion current = gameObject.transform.localRotation;
            Quaternion pointing = Quaternion.Euler(0, Mathf.Rad2Deg * rotationHandleDesc.angle, 0) * current;
            // float startingEulerY = localRotation.y;
            float targetingEulerY = pointing.eulerAngles.y;
            var rotationSnap = RotationSnapValue();
            if (rotationSnap > 0) targetingEulerY = Mathf.Round(targetingEulerY / rotationSnap) * rotationSnap;
            Quaternion resulting = Quaternion.Euler(0, targetingEulerY, 0);
            float targetingAngle = Quaternion.Angle(current, resulting);
            if (targetingAngle > rotationSnap * 0.1f) {
                CustomHandles.AddAngleOffset(rotationHandleDesc.angle < Mathf.PI ? targetingAngle : -targetingAngle);
                if (resulting != current) {
                    Undo.RecordObject(gameObject.transform, "Rotate " + gameObject.name);
                    gameObject.transform.localRotation = resulting;
                    EditorUtility.SetDirty(gameObject.transform);
                    modified = true;
                }
            }
        }
        
        public static SimpleObject     TryPickUp(SimpleObjectData simpleObjectData, Dictionary<long,GameObject> possibleGameObjectsList) {
            if (possibleGameObjectsList.TryGetValue(simpleObjectData.fileID, out GameObject gameObject)) {
                return new SimpleObject(gameObject);
            }
            return null;
        }
        public        SimpleObjectData GatherData(string rootPrefabPath) {
            var gameObjectInRoot = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(gameObject, rootPrefabPath);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(gameObjectInRoot, out string _, out long fileID);
            return new SimpleObjectData(fileID);
        }

        public static float    GetMoveSnap() {
            return LD_Window.OptionsSettings().moveSnap;
        }
    }
    
    [Serializable]
    public class SimpleObjectData : ManagedObjectData<SimpleObjectData> {
        public long fileID;
        
        public                           SimpleObjectData(long fileID) {
            this.fileID = fileID;
        }
        public                           SimpleObjectData(SimpleObjectData other) {
            this.fileID = other.fileID;
        }
        public override SimpleObjectData Clone() {
            return new SimpleObjectData(this);
        }
        public override bool             Equals(SimpleObjectData other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!fileID.Equals(other.fileID)) return false;
            return true;
        }
    }
}