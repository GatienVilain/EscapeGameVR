using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GistLevelDesigner {
    public static class CustomHandles {
        private static readonly int handlesHint = (Prefs.levelDesignObjectName + ".LD_Handles").GetHashCode();
        private static readonly int colorMaterialCullPropertyID = Shader.PropertyToID("_Cull");
        private static Material colorMaterial;
        private static Vector2  mouseStart;
        private static Vector3  hotPosition;
        private static Vector3  worldStart;
        private static bool     movedAfterFirstPress;
        private static float    hotAngle;
        private static float    angleOffset;
        private static bool     altPressed;
        private static float    handleMaxSizeK = 1f / HandlesStyles.handleMaxSizeBasis;
        
        public  static void     SetHotPosition(Vector3 position) {
            hotPosition = position;
        }
        public  static void     AddAngleOffset(float angleDeg) {
            angleOffset += angleDeg;
        }
        public  static void     SetMaxSize(float maxSize, float maxSizeBasis) {
            CustomHandles.handleMaxSizeK = maxSize / maxSizeBasis;
        }
        
        private static void     InitColorMaterial() {
            if (colorMaterial == null) {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                colorMaterial = new Material(shader) {
                    hideFlags = HideFlags.HideAndDontSave
                };
                colorMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                colorMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                SetColorMaterialCulling(CullMode.Back);
                colorMaterial.SetInt("_ZTest", 0);
                colorMaterial.SetInt("_ZWrite", 0);
            }
        }
        private static void     SetColorMaterialCulling(CullMode cullMode) {
            colorMaterial.SetInt(colorMaterialCullPropertyID, (int)cullMode);
        }
        
        private static void     GetControlInfo(out int controlID, out EventType eventType) {
            controlID = GUIUtility.GetControlID(handlesHint, FocusType.Passive);
            eventType = Event.current.GetTypeForControl(controlID);
        }
        private static bool     GeneralDragBlock(int controlID, EventType eventType, ref GeneralHandleDesc generalHandleDesc, ref HandleEventInfo eventInfo) {
            eventInfo.affected = false;
            switch (eventType) {
                case EventType.MouseMove:
                    break;
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.LeftAlt) altPressed = true;
                    break;
                case EventType.KeyUp:
                    if (Event.current.keyCode == KeyCode.LeftAlt) altPressed = false;
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID) {
                        var draggedScreenPosition = HandleUtility.WorldToGUIPointWithDepth(worldStart) + (Vector3)(Event.current.mousePosition - mouseStart);
                        var draggedWorldPosition = HandleUtility.GUIPointToWorldRay(draggedScreenPosition).GetPoint(draggedScreenPosition.z);
                        
                        var camForward = SceneView.lastActiveSceneView.camera.transform.forward;
                        if (camForward == Vector3.forward || camForward == -Vector3.forward) draggedWorldPosition.z = generalHandleDesc.position.z;
                        if (camForward == Vector3.up || camForward == -Vector3.up) draggedWorldPosition.y = generalHandleDesc.position.y;
                        if (camForward == Vector3.right || camForward == -Vector3.right) draggedWorldPosition.x = generalHandleDesc.position.x;
                        generalHandleDesc.position = draggedWorldPosition;
                        movedAfterFirstPress = true;
                        
                        SetHotPosition(draggedWorldPosition);
                        GUI.changed = true;
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.MouseDown:
                    if (!altPressed && HandleUtility.nearestControl == controlID) {
                        if (Event.current.button == 0) {
                            GUIUtility.hotControl = controlID;
                            mouseStart = Event.current.mousePosition;
                            worldStart = generalHandleDesc.position;
                            movedAfterFirstPress = false;
                            SetHotPosition(worldStart);
                            Event.current.Use();
                            EditorGUIUtility.SetWantsMouseJumping(1);
                        }
                        eventInfo.eventType = HandleEventInfo.Type.PRESS;
                        eventInfo.eventButton = Event.current.button;
                        eventInfo.affected = true;
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID) {
                        if (Event.current.button == 0) {
                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                            EditorGUIUtility.SetWantsMouseJumping(0);
                        }
                        eventInfo.eventType = movedAfterFirstPress ? HandleEventInfo.Type.RELEASE : HandleEventInfo.Type.CLICK;
                        eventInfo.eventButton = Event.current.button;
                        eventInfo.affected = true;
                    }
                    break;
            }
            return false;
        }
        private static void     GeneralDescBlock(int controlID, GeneralHandleDesc generalHandleDesc, out Vector3 drawPosition, out Color color, out Color outlineColor) {
            if (GUIUtility.hotControl == controlID) {
                drawPosition = hotPosition;
                color = generalHandleDesc.activeColor;
                outlineColor = generalHandleDesc.activeOutlineColor;
            } else {
                drawPosition = generalHandleDesc.position;
                if (GUIUtility.hotControl == 0 && HandleUtility.nearestControl == controlID) {
                    color = generalHandleDesc.hoverColor;
                    outlineColor = generalHandleDesc.hoverOutlineColor;
                } else {
                    color = generalHandleDesc.color;
                    outlineColor = generalHandleDesc.outlineColor;
                }
            }
        }
        
        public  static bool     DragHandle(ref VerticesMeshHandleDesc handleDesc, ref HandleEventInfo eventInfo) {
            GetControlInfo(out int controlID, out EventType eventType);
            Quaternion vertsRotation;
            
            switch (eventType) {
                case EventType.Layout:
                    if (handleDesc.vertices == null) break;
                    bool currentHotControl = GUIUtility.hotControl == controlID;
                    Vector3 handlePosition = currentHotControl ? hotPosition : handleDesc.general.position;
                    Matrix4x4 TRS = handleDesc.vertsLocalToWorld;
                    
                    float handleSize = 1f;
                    if (handleDesc.staticSize) {
                        handleSize = Mathf.Min(HandleUtility.GetHandleSize(currentHotControl ? handlePosition : TRS.MultiplyPoint3x4(handlePosition)), handleMaxSizeK);
                    }
                    handleDesc.cache.handleSize = handleSize;
                    
                    if (GUIUtility.hotControl != 0) {
                        HandleUtility.AddControl(controlID, float.MaxValue);
                        break;
                    }
                    
                    Vector3[] verts = handleDesc.vertices;
                    int[] order = handleDesc.verticesOrder;
                    Vector2 mousePos = Event.current.mousePosition;
                    float distance = float.PositiveInfinity;
                    int vertI;
                    if (handleDesc.renderType == VerticesMeshHandleDesc.RenderType.QUADS) {
                        vertI = 0;
                        vertsRotation = handleDesc.verticesRotation;
                        while (vertI < order.Length) {
                            Vector2 quad0 = HandleUtility.WorldToGUIPoint(TRS.MultiplyPoint3x4(handlePosition + vertsRotation * (handleSize * verts[order[vertI]])));
                            Vector2 quad1 = HandleUtility.WorldToGUIPoint(TRS.MultiplyPoint3x4(handlePosition + vertsRotation * (handleSize * verts[order[vertI + 1]])));
                            Vector2 quad2 = HandleUtility.WorldToGUIPoint(TRS.MultiplyPoint3x4(handlePosition + vertsRotation * (handleSize * verts[order[vertI + 2]])));
                            Vector2 quad3 = HandleUtility.WorldToGUIPoint(TRS.MultiplyPoint3x4(handlePosition + vertsRotation * (handleSize * verts[order[vertI + 3]])));
                            
                            if (   IsPointInTriangle(mousePos, quad0, quad1, quad2)
                                || IsPointInTriangle(mousePos, quad2, quad3, quad0)) {
                                distance = 0;
                                break;
                            } else {
                                distance = Mathf.Min(distance, (Closest(mousePos, quad0, quad1) - mousePos).sqrMagnitude);
                                distance = Mathf.Min(distance, (Closest(mousePos, quad1, quad2) - mousePos).sqrMagnitude);
                                distance = Mathf.Min(distance, (Closest(mousePos, quad2, quad3) - mousePos).sqrMagnitude);
                                distance = Mathf.Min(distance, (Closest(mousePos, quad3, quad0) - mousePos).sqrMagnitude);
                                distance = Mathf.Min(distance, (Closest(mousePos, quad0, quad2) - mousePos).sqrMagnitude);
                                distance = Mathf.Min(distance, (Closest(mousePos, quad1, quad3) - mousePos).sqrMagnitude);
                            }
                            vertI += 4;
                        }
                    } else {
                        vertI = 0;
                        vertsRotation = handleDesc.verticesRotation;
                        while (vertI < order.Length) {
                            Vector2 tri0 = HandleUtility.WorldToGUIPoint(TRS.MultiplyPoint3x4(handlePosition + vertsRotation * (handleSize * verts[order[vertI]])));
                            Vector2 tri1 = HandleUtility.WorldToGUIPoint(TRS.MultiplyPoint3x4(handlePosition + vertsRotation * (handleSize * verts[order[vertI + 1]])));
                            Vector2 tri2 = HandleUtility.WorldToGUIPoint(TRS.MultiplyPoint3x4(handlePosition + vertsRotation * (handleSize * verts[order[vertI + 2]])));
                            
                            if (IsPointInTriangle(mousePos, tri0, tri1, tri2)) {
                                distance = 0;
                                break;
                            } else {
                                distance = Mathf.Min(distance, (Closest(mousePos, tri0, tri1) - mousePos).sqrMagnitude);
                                distance = Mathf.Min(distance, (Closest(mousePos, tri1, tri2) - mousePos).sqrMagnitude);
                                distance = Mathf.Min(distance, (Closest(mousePos, tri2, tri0) - mousePos).sqrMagnitude);
                            }
                            vertI += 3;
                        }
                    }
                    HandleUtility.AddControl(controlID, Mathf.Sqrt(distance));
                    break;
                case EventType.Repaint:
                    if (handleDesc.vertices == null) break;
                    
                    GeneralDescBlock(controlID, handleDesc.general, out Vector3 drawPosition, out Color color, out Color outlineColor);
                    
                    InitColorMaterial();
                    colorMaterial.SetPass(0);
                    SetColorMaterialCulling(handleDesc.cullMode);
                    
                    GL.PushMatrix();
                    GL.MultMatrix(Matrix4x4.identity);
                    TRS = handleDesc.vertsLocalToWorld;
                    if (GUIUtility.hotControl == controlID) {
                        drawPosition = TRS.inverse.MultiplyPoint3x4(drawPosition);
                    }
                    
                    verts = handleDesc.vertices;
                    order = handleDesc.verticesOrder;
                    int verticesBlock;
                    if  (handleDesc.renderType == VerticesMeshHandleDesc.RenderType.QUADS ) {
                        verticesBlock = 4;
                        GL.Begin(GL.QUADS);
                    } else {
                        verticesBlock = 3;
                        GL.Begin(GL.TRIANGLES);
                    }
                    
                    // handleSize = handleDesc.staticSize ? Mathf.Min(handleDesc.cache.handleSize, handleMaxSizeK) : 1f;
                    handleSize = handleDesc.cache.handleSize;
                    GL.Color(color);
                    
                    vertI = 0;
                    vertsRotation = handleDesc.verticesRotation;
                    while (vertI < order.Length) {
                        for (int vertOffset = 0; vertOffset < verticesBlock; vertOffset++) {
                            Vector3 vertex = TRS.MultiplyPoint3x4(drawPosition + vertsRotation * (handleSize * handleDesc.vertices[order[vertI + vertOffset]]));
                            GL.Vertex3(vertex.x, vertex.y, vertex.z);
                        }
                        vertI += verticesBlock;
                    }                    
                    GL.End();
                    
                    if (handleDesc.wireframe) {
                        GL.Begin(GL.LINES);
                        GL.Color(outlineColor);
                        vertI = 0;
                        while (vertI < order.Length) {
                            for (int vertOffset = 0; vertOffset < verticesBlock; vertOffset++) {
                                Vector3 vertex1 = TRS.MultiplyPoint3x4(drawPosition + vertsRotation * (handleSize * verts[order[vertI + vertOffset]]));
                                Vector3 vertex2 = TRS.MultiplyPoint3x4(drawPosition + vertsRotation * (handleSize * verts[order[vertI + ((vertOffset + 1) % verticesBlock)]]));
                                GL.Vertex3(vertex1.x, vertex1.y, vertex1.z);
                                GL.Vertex3(vertex2.x, vertex2.y, vertex2.z);
                            }
                            vertI += verticesBlock;
                        }
                        
                        GL.End();
                    }
                                    
                    GL.PopMatrix();
                    break;
            }
            
            bool generalDragBlock = GeneralDragBlock(controlID, eventType, ref handleDesc.general, ref eventInfo);
            
            if (eventType == EventType.MouseDown) {
                if (GUIUtility.hotControl == controlID) {
                    worldStart = handleDesc.vertsLocalToWorld.MultiplyPoint3x4(handleDesc.general.position);
                    SetHotPosition(worldStart);
                }
            }
            
            return generalDragBlock;
        }
        public  static bool     RotationHandle(ref CircleHandleDesc handleDesc) {
            GetControlInfo(out int controlID, out EventType eventType);
            
            switch (eventType) {
                case EventType.Layout:
                    Vector3 handlePosition = handleDesc.general.position;
                    Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    Plane discPlane = new Plane(handleDesc.normal, handlePosition);
                    if (discPlane.Raycast(mouseRay, out float discPlaneDistance)) {
                        Vector3 mouseDiscPlanePoint = mouseRay.GetPoint(discPlaneDistance);
                        Vector3 mouseVectorFromDisc = mouseDiscPlanePoint - handlePosition;
                        if (mouseVectorFromDisc.sqrMagnitude > 0) {
                            float handleSizeCalc = Mathf.Min(HandleUtility.GetHandleSize(handleDesc.general.position), handleMaxSizeK) * handleDesc.size;
                            Vector3 discOutlineClosestPoint = handlePosition + mouseVectorFromDisc.normalized * handleSizeCalc;
                            float distance = (Event.current.mousePosition - HandleUtility.WorldToGUIPoint(discOutlineClosestPoint)).magnitude;
                            handleDesc.cache.handleSize = handleSizeCalc;
                            HandleUtility.AddControl(controlID, distance);
                        }
                    }
                    break;
                case EventType.Repaint:
                    Color handlesColor = Handles.color;
                    
                    GeneralDescBlock(controlID, handleDesc.general, out Vector3 _, out Color color, out Color _);
                    
                    // float handleSize = Mathf.Min(handleDesc.cache.handleSize, handleDesc.size * handleMaxSizeK);
                    float handleSize = handleDesc.cache.handleSize;
                    if (GUIUtility.hotControl == controlID) {
                        float angle = hotAngle;
                        if (angle > Mathf.PI) angle = - (Mathf.PI * 2f - angle);
                        Handles.color = new Color(color.r, color.g, color.b, color.a * 0.4f);
                        Handles.DrawSolidArc(handleDesc.general.position, handleDesc.normal, worldStart - handleDesc.general.position, angle * Mathf.Rad2Deg, handleSize);
                    }
                    Handles.color = color;
                    Handles.DrawWireDisc(handleDesc.general.position, handleDesc.normal, handleSize);
                    
                    Handles.color = handlesColor;
                    break;
                case EventType.MouseMove:
                    break;
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.LeftAlt) altPressed = true;
                    break;
                case EventType.KeyUp:
                    if (Event.current.keyCode == KeyCode.LeftAlt) altPressed = false;
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID) {
                        mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        discPlane = new Plane(handleDesc.normal, handleDesc.general.position);
                        if (discPlane.Raycast(mouseRay, out float distance)) {
                            Vector3 mouseRotationPlanePoint = mouseRay.GetPoint(distance);
                            Vector3 targeting = mouseRotationPlanePoint - handleDesc.general.position;
                            Vector3 original = worldStart - handleDesc.general.position;
                            hotAngle = Angle(original, targeting);
                            handleDesc.angle = Mathf.Repeat(hotAngle - angleOffset * Mathf.Deg2Rad, Mathf.PI * 2f);
                            GUI.changed = true;
                            Event.current.Use();
                            return true;
                        }
                    }
                    break;
                case EventType.MouseDown:
                    if (!altPressed && HandleUtility.nearestControl == controlID && Event.current.button == 0) {
                        Vector2 mousePosition  = Event.current.mousePosition;
                        mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        discPlane = new Plane(handleDesc.normal, handleDesc.general.position);
                        if (discPlane.Raycast(mouseRay, out float distance)) {
                            GUIUtility.hotControl = controlID;
                            mouseStart = mousePosition;
                            worldStart = mouseRay.GetPoint(distance);
                            hotAngle = 0;
                            angleOffset = 0;
                            handleDesc.angle = 0;
                            Event.current.Use();
                            EditorGUIUtility.SetWantsMouseJumping(1);
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && Event.current.button == 0) {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(0);
                    }
                    break;
            }

            return false;
        }
        
        private static float    Angle(Vector3 startVector, Vector3 endVector) {
            float startAngle = Mathf.Atan2(startVector.z, startVector.x);
            float endAngle = Mathf.Atan2(endVector.z, endVector.x);
            return Mathf.Repeat(startAngle - endAngle, Mathf.PI * 2f);
        }
        private static bool     IsPointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2) {
            float Area = 0.5f *(-p1.y*p2.x + p0.y*(-p1.x + p2.x) + p0.x*(p1.y - p2.y) + p1.x*p2.y);
            float s = 1f/(2f*Area)*(p0.y*p2.x - p0.x*p2.y + (p2.y - p0.y)*p.x + (p0.x - p2.x)*p.y);
            float t = 1f/(2f*Area)*(p0.x*p1.y - p0.y*p1.x + (p0.y - p1.y)*p.x + (p1.x - p0.x)*p.y);
            float oneMST = 1f - s - t;
            
            if (s > 0 && t > 0 && oneMST > 0) return true;
            return false;
        }
        private static Vector2  Closest(Vector2 p, Vector2 p1, Vector2 p2) {
            Vector2 AP = p - p1;
            Vector2 AB = p2 - p1;
            
            float magnitudeAB = AB.sqrMagnitude;
            float ABAProduct = Vector2.Dot(AP, AB);
            float distance = ABAProduct / magnitudeAB;
            
            if (distance < 0) {
                return p1;
            } else if (distance > 1) {
                return p2;
            } else {
                return p1 + AB * distance;
            }
        }
    }
    
    public struct HandleDescCacheStorage {
        public float handleSize;
    }
    public class GeneralHandleDesc {
        public Vector3      position;
        public Color        color;
        public Color        outlineColor;
        public Color        hoverColor;
        public Color        hoverOutlineColor;
        public Color        activeColor;
        public Color        activeOutlineColor;
    }
    public struct DiscHandleDesc {
        public GeneralHandleDesc general;
        public HandleDescCacheStorage cache;
        public Vector3 normal;
        public float   size;
    }
    public struct VerticesMeshHandleDesc {
        public enum RenderType {TRIANGLES, QUADS}
        public GeneralHandleDesc general;
        public HandleDescCacheStorage cache;
        public Vector3[]  vertices;
        public int[]      verticesOrder;
        public Quaternion verticesRotation;
        public Matrix4x4  vertsLocalToWorld;
        public RenderType renderType;
        public CullMode   cullMode;
        public bool       wireframe;
        public bool       staticSize;
    }
    public struct CircleHandleDesc {
        public GeneralHandleDesc general;
        public HandleDescCacheStorage cache;
        public float      angle;
        public Vector3    normal;
        public float      size;
    }
}