using UnityEngine;
using UnityEditor;
using System;

namespace GistLevelDesigner {
    public abstract class SceneObject {
        protected static Plane           floorPlane;
        protected static Matrix4x4       worldToRootMatrix;
        protected static Matrix4x4       rootToWorldMatrix;
        protected static Vector3         rootUpWorld;
        protected static Vector3         cameraPosition;
        protected static HandleEventInfo eventInfo;

        protected bool modified = false;
        public GameObject gameObject;
        
        protected SceneObject(GameObject gameObject){
            this.gameObject = gameObject;
        }
        
        public virtual void SceneGUI(){}
        public virtual void SortObjects(){}
        public virtual bool IsGameObjectManaged(GameObject gameObject) {
            if (gameObject == null) return false;
            return this.gameObject == gameObject;
        }
        public virtual void UpdateSceneGUIdrawingData() {}
        public virtual bool CheckAndFixUnexpectedChangesAfterUndo() {
            return false;
        }
        
        public static  void    SetRootTransform(Transform rootTransform) {
            Matrix4x4 localToWorldCache = rootTransform.localToWorldMatrix;
            if (rootToWorldMatrix != localToWorldCache) {
                rootToWorldMatrix = rootTransform.localToWorldMatrix;
                worldToRootMatrix = rootTransform.worldToLocalMatrix;
                rootUpWorld = rootToWorldMatrix.MultiplyVector(Vector3.up);
                Vector3 floorPlaneInPoint = rootToWorldMatrix.MultiplyPoint3x4(Vector3.zero);
                floorPlane = new Plane(rootUpWorld, floorPlaneInPoint);
            }
        }
        public static  Plane   GetFloorPlane() {
            return floorPlane;
        }
        public static  void    SampleCameraPosition() {
            cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
        }
        public virtual float   MoveSnapValue() {
            return LD_Window.OptionsSettings().moveSnap;
        }
        public virtual float   RotationSnapValue() {
            return LD_Window.OptionsSettings().rotationSnap;
        }
        public static  Vector3 GetSampledCameraPosition() {
            return cameraPosition;
        }
    }
    
    public interface IManagedObject<DataType> where DataType : ManagedObjectData<DataType> {
        DataType GatherData(string rootPrefabPath);
    }
    public abstract class ManagedObjectData<DataType> where DataType : class {
        public abstract DataType Clone();
        public abstract bool Equals(DataType other);
        
        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as DataType);
        }
        public static bool operator==(ManagedObjectData<DataType> mod1, ManagedObjectData<DataType> mod2) {
            return mod1.Equals(mod2);
        }
        public static bool operator!=(ManagedObjectData<DataType> mod1, ManagedObjectData<DataType> mod2) {
            return !mod1.Equals(mod2);
        }
        public override int GetHashCode() {
            throw new NotImplementedException("No GetHashCode() implementation");
        }
    }
}