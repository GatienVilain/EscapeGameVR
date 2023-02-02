using UnityEngine;
using UnityEditor;

namespace GistLevelDesigner {
    public struct Prefs {
        public const string version = "1.3.3";
        public const string levelDesignerPackageName = "Gist Level Designer";
        public const string levelDesignerMenuPath = "Window/Twice Better/" + levelDesignerPackageName;
        public const string levelDesignerWindowTitle = "Level Designer";
        public const string levelDesignerMainSOassetName = "LevelDesigner.asset";
        public const string levelDesignObjectName = "LevelDesign";
        public static class Foldouts {
            public struct Foldout {
                public string name;
                public bool   value;
                public string editorPrefName;
                public void Init(bool defaultValue = true) {
                    value = EditorPrefs.GetBool(editorPrefName, defaultValue);
                }
                public void SetValue(bool value) {
                    this.value = value;
                    EditorPrefs.SetBool(editorPrefName, value);
                }
            }
            public static Foldout walls      = new Foldout{name = "WALLS", editorPrefName = Prefs.levelDesignObjectName + nameof(walls)};
            public static Foldout objects    = new Foldout{name = "OBJECTS", editorPrefName = Prefs.levelDesignObjectName + nameof(objects)};
            public static Foldout floors     = new Foldout{name = "FLOORS", editorPrefName = Prefs.levelDesignObjectName + nameof(floors)};
            public static Foldout settings   = new Foldout{name = "SETTINGS", editorPrefName = Prefs.levelDesignObjectName + nameof(settings)};
            public static Foldout sets       = new Foldout{name = "Sets", editorPrefName = Prefs.levelDesignObjectName + nameof(sets)};
            public static Foldout options    = new Foldout{name = "Options", editorPrefName = Prefs.levelDesignObjectName + nameof(options)};
            public static Foldout visibility = new Foldout{name = "Visibility", editorPrefName = Prefs.levelDesignObjectName + nameof(visibility)};
            static Foldouts() {
                walls.Init();
                objects.Init();
                floors.Init();
                settings.Init();
                sets.Init();
                options.Init(false);
                visibility.Init(false);
            }
        }
        public static class Icons {
            public static Texture2D wall_I;
            public static Texture2D wall_L;
            public static Texture2D wall_T;
            public static Texture2D wall_X;
        }
    }
}