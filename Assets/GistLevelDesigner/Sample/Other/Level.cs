#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GistLevelDesigner;

namespace GistLevelDesignerSample {
    [ExecuteInEditMode]
    public class Level : MonoBehaviour {
        private static bool opened;
        void Awake() {
            opened = false;
        }
        void Update() {
            if (!opened && !Application.isPlaying) {
                LD_Window.OpenWindowOnce();
                Tools.current = Tool.None;
                Selection.activeGameObject = gameObject;
                LD_Window.FocusWindowIfItsOpen<LD_Window>();
                opened = true;
            }
        }
    }
}
#endif