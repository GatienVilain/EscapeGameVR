using UnityEngine;
using UnityEditor;

namespace GistLevelDesigner {
    public static class Styles {
        private static readonly RectOffset zeroOffset = new RectOffset(0, 0, 0, 0);
        public static GUIStyle lockButton;
        public static GUIStyle createButton;
        public static GUIStyle objectPreviewButton;
        public static GUIStyle newSObutton;
        public static GUIStyle foldStyle;
        public static GUIStyle foldStylem12;
        public static GUIStyle foldStylem12marginFixed;
        public static float    foldHeight;
        public static GUIStyle toggleStylem22;
        public static GUIStyle toggleStylem39;
        public static GUIStyle toggleStylem56;
        public static GUIStyle settingsOptionsLabel;
        public static GUIStyle settingsOptionsValue;
        
        static Styles() {
            Texture2D emptyTex = new Texture2D(1, 1);
            emptyTex.SetPixels32(new Color32[]{new Color(0, 0, 0, 0)});
            emptyTex.Apply();
            
            lockButton = "IN LockButton";
            createButton = new GUIStyle(GUI.skin.button) {
                padding = new RectOffset(25, 25, 7, 7),
                margin = zeroOffset
            };
            newSObutton = new GUIStyle(GUI.skin.button) {
                fixedWidth = 50,
                fixedHeight = 18
            };
            objectPreviewButton = new GUIStyle(GUI.skin.button) {
                padding = zeroOffset,
                margin = new RectOffset(2, 2, 0, 0)
            };
            objectPreviewButton.normal.background = emptyTex;
            foldStyle = new GUIStyle(EditorStyles.foldoutHeader) {
                margin = zeroOffset
            };
            foldStylem12 = new GUIStyle(EditorStyles.foldoutHeader) {
                margin = new RectOffset(12, 0, 0, 0)
            };
            foldStylem12marginFixed = new GUIStyle(foldStylem12) {
                margin = new RectOffset(26, 0, 0, 0)
            };
            
            foldHeight = foldStyle.CalcHeight(new GUIContent(), 0);
            toggleStylem22 = ToggleStyleMarginLeft(22);
            toggleStylem39 = ToggleStyleMarginLeft(39);
            toggleStylem56 = ToggleStyleMarginLeft(56);
            
            settingsOptionsLabel = new GUIStyle(GUI.skin.label);
            settingsOptionsLabel.margin = new RectOffset(28, 0, 2, 2);
            settingsOptionsLabel.padding = new RectOffset(0, 2, 1, 1);
            settingsOptionsLabel.fixedWidth = 160;
            
            settingsOptionsValue = new GUIStyle(GUI.skin.textField);
            settingsOptionsValue.fixedWidth = 50;
        }
        private static GUIStyle ToggleStyleMarginLeft(int marginLeft) {
            var toggleStyle = new GUIStyle(GUI.skin.toggle) {
                margin = new RectOffset(marginLeft, 2, 3, 0)
            };
            return toggleStyle;
        }
    }
}