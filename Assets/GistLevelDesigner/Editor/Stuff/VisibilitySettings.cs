using System;
using System.Reflection;

namespace GistLevelDesigner {
    [Serializable]
    public class VisibilitySettings : IEquatable<VisibilitySettings>, ICloneable {
        private static readonly FieldInfo[] fields = typeof(VisibilitySettings).GetFields(BindingFlags.Instance | BindingFlags.Public);
        public bool movement;
        public bool rotation;
        public bool walls;
        public bool wallsMovement;
        public bool wallsRotation;
        public bool wings;
        public bool wingsGrowth;
        public bool wingsLines;
        public bool objects;
        public bool floors;
        public bool hidden;
        
        public object Clone() {
            var clone = new VisibilitySettings();
            for (int i = 0; i < fields.Length; i++) fields[i].SetValue(clone, fields[i].GetValue(this));
            return clone;
        }
        public bool Equals(VisibilitySettings other) {
            if (ReferenceEquals(this, other)) return true;
            for (int i = 0; i < fields.Length; i++) if (!fields[i].GetValue(this).Equals(fields[i].GetValue(other))) return false;
            return true;
        }
        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as VisibilitySettings);
        }
        public static bool operator==(VisibilitySettings vis1, VisibilitySettings vis2) {
            return vis1.Equals(vis2);
        }
        public static bool operator!=(VisibilitySettings vis1, VisibilitySettings vis2) {
            return !vis1.Equals(vis2);
        }
        public override int GetHashCode() {
            throw new NotImplementedException("No GetHashCode() implementation");
        }
    }
    
    [Serializable]
    public class OptionsSettings : IEquatable<OptionsSettings>, ICloneable {
        private static readonly FieldInfo[] fields = typeof(OptionsSettings).GetFields(BindingFlags.Instance | BindingFlags.Public);
        public float moveSnap;
        public float rotationSnap;
        public float wallsManipulatorsHeight;
        public bool  hoverResponsiveHandles;
        
        public object Clone() {
            var clone = new OptionsSettings();
            for (int i = 0; i < fields.Length; i++) fields[i].SetValue(clone, fields[i].GetValue(this));
            return clone;
        }
        public bool Equals(OptionsSettings other) {
            if (ReferenceEquals(this, other)) return true;
            for (int i = 0; i < fields.Length; i++) if (!fields[i].GetValue(this).Equals(fields[i].GetValue(other))) return false;
            return true;
        }
        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as OptionsSettings);
        }
        public static bool operator==(OptionsSettings vis1, OptionsSettings vis2) {
            return vis1.Equals(vis2);
        }
        public static bool operator!=(OptionsSettings vis1, OptionsSettings vis2) {
            return !vis1.Equals(vis2);
        }
        public override int GetHashCode() {
            throw new NotImplementedException("No GetHashCode() implementation");
        }
    }
}