using System;

namespace GistLevelDesigner {
    public enum Identity {UNDEFINED, INSTANCE, ASSET}
    public enum WallType {WALL_I, WALL_L, WALL_T, WALL_X}
    
    [Serializable]
    public struct GameObjectIdentity {
        public Identity identity;
        public int      instanceID;
        public string   GUID;
        public long     fileID;
        public string   rootGUID;
    }
    
    public struct HandleEventInfo {
        public enum Type {PRESS, RELEASE, CLICK}
        public bool affected;
        public Type eventType;
        public int  eventButton;
    }
}