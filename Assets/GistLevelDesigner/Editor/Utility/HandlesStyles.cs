using UnityEngine.Rendering;
using UnityEngine;

namespace GistLevelDesigner {
    public static class HandlesStyles {
        public  const  float handleMaxSizeBasis = 0.22f;
        public  const  float rotationHandleSize = handleMaxSizeBasis;
        public  const  float moveHandleSize     = 0.23f * rotationHandleSize;
        private static readonly int[] oneQuadOrder       = new int[]{0,1,2,3};
        
        // Colors
        private static class Colors {
            public const uint simpleObject              = 0x80b3ff;
            public const uint simpleObjectHover         = 0xb3d1ff;
            public const uint simpleObjectOutline       = 0x264d8c;
            public const uint simpleObjectOutlineHover  = 0x376fc8;
            public const uint simpleObjectOutlineActive = 0x739ad9;
            
            public const uint wallObject              = 0xdab625;
            public const uint wallObjectHover         = 0xe5cc66;
            public const uint wallObjectOutline       = 0xc1750b;
            public const uint wallObjectOutlineHover  = 0xf29d26;
            public const uint wallObjectOutlineActive = 0xf7be6e;
            
            public const uint floor              = 0x59e050;
            public const uint floorHover         = 0x99ec93;
            public const uint floorOutline       = 0x20651b;
            public const uint floorOutlineHover  = 0x33a12b;
            public const uint floorOutlineActive = 0x53cf4a;
        }
        
        // Simple
        public  static GeneralHandleDesc      simpleObjectMoveColors = new GeneralHandleDesc {
            position = Vector3.zero,
            color = HexColor(Colors.simpleObject, 0.5f),
            outlineColor = HexColor(Colors.simpleObjectOutline, 0.8f),
            hoverColor = HexColor(Colors.simpleObjectHover, 0.5f),
            hoverOutlineColor = HexColor(Colors.simpleObjectOutlineHover, 0.8f),
            activeColor = HexColor(Colors.simpleObject, 0.5f),
            activeOutlineColor = HexColor(Colors.simpleObjectOutlineActive, 0.8f)
        };
        public  static GeneralHandleDesc      simpleObjectRotationColors = new GeneralHandleDesc {
            position = Vector3.zero,
            color = HexColor(Colors.simpleObjectOutline, 0.8f),
            hoverColor = HexColor(Colors.simpleObjectOutlineHover, 0.8f),
            activeColor = HexColor(Colors.simpleObjectOutlineActive, 0.8f)
        };
        public  static Vector3[]              moveHandleVertices = new Vector3[]{
            new Vector3(-moveHandleSize, 0,  moveHandleSize),
            new Vector3( moveHandleSize, 0,  moveHandleSize),
            new Vector3( moveHandleSize, 0, -moveHandleSize),
            new Vector3(-moveHandleSize, 0, -moveHandleSize),
        };
        public  static VerticesMeshHandleDesc simpleObjectMoveHandleDesc = new VerticesMeshHandleDesc {
            general = simpleObjectMoveColors,
            vertices = moveHandleVertices,
            verticesOrder = oneQuadOrder,
            verticesRotation = Quaternion.identity,
            vertsLocalToWorld = Matrix4x4.identity,
            renderType = VerticesMeshHandleDesc.RenderType.QUADS,
            cullMode = CullMode.Off,
            wireframe = true,
            staticSize = true
        };
        public  static CircleHandleDesc       simpleObjectRotationHandleDesc = new CircleHandleDesc {
            general = simpleObjectRotationColors,
            angle = 0,
            normal = Vector3.up,
            size = HandlesStyles.rotationHandleSize,
        };
        
        // Floor
        public  static GeneralHandleDesc      floorObjectColors = new GeneralHandleDesc {
            position = Vector3.zero,
            color = HexColor(Colors.floor, 0.4f),
            outlineColor = HexColor(Colors.floorOutline, 0.7f),
            hoverColor = HexColor(Colors.floorHover, 0.4f),
            hoverOutlineColor = HexColor(Colors.floorOutlineHover, 0.7f),
            activeColor = HexColor(Colors.floor, 0.4f),
            activeOutlineColor = HexColor(Colors.floorOutlineActive, 0.7f)
        };
        public  static GeneralHandleDesc      floorObjectRotationColors = new GeneralHandleDesc {
            position = Vector3.zero,
            color = HexColor(Colors.floorOutline, 0.8f),
            hoverColor = HexColor(Colors.floorOutlineHover, 0.8f),
            activeColor = HexColor(Colors.floorOutlineActive, 0.8f)
        };
        public  static VerticesMeshHandleDesc floorObjectSideHandleDesc = new VerticesMeshHandleDesc{
            general = floorObjectColors,
            vertices = null,
            verticesOrder = oneQuadOrder,
            verticesRotation = Quaternion.identity,
            vertsLocalToWorld = Matrix4x4.identity,
            renderType = VerticesMeshHandleDesc.RenderType.QUADS,
            cullMode = CullMode.Off,
            wireframe = true,
            staticSize = false
        };
        public  static VerticesMeshHandleDesc floorMoveHandleDesc = new VerticesMeshHandleDesc {
            general = floorObjectColors,
            vertices = moveHandleVertices,
            verticesOrder = oneQuadOrder,
            verticesRotation = Quaternion.identity,
            vertsLocalToWorld = Matrix4x4.identity,
            renderType = VerticesMeshHandleDesc.RenderType.QUADS,
            cullMode = CullMode.Off,
            wireframe = true,
            staticSize = true
        };
        public  static CircleHandleDesc       floorRotationHandleDesc = new CircleHandleDesc {
            general = floorObjectRotationColors,
            angle = 0,
            normal = Vector3.up,
            size = HandlesStyles.rotationHandleSize,
        };
        
        // Wall
        public  static GeneralHandleDesc      wallObjectColors = new GeneralHandleDesc {
            position = Vector3.zero,
            color = HexColor(Colors.wallObject, 0.5f),
            outlineColor = HexColor(Colors.wallObjectOutline, 0.8f),
            hoverColor = HexColor(Colors.wallObjectHover, 0.5f),
            hoverOutlineColor = HexColor(Colors.wallObjectOutlineHover, 0.8f),
            activeColor = HexColor(Colors.wallObject, 0.5f),
            activeOutlineColor = HexColor(Colors.wallObjectOutlineActive, 0.8f)
        };
        public  static GeneralHandleDesc      wallObjectRotationColors = new GeneralHandleDesc {
            position = Vector3.zero,
            color = HexColor(Colors.wallObjectOutline, 0.8f),
            hoverColor = HexColor(Colors.wallObjectOutlineHover, 0.8f),
            activeColor = HexColor(Colors.wallObjectOutlineActive, 0.8f)
        };
        public  static VerticesMeshHandleDesc wallMoveHandleDesc = new VerticesMeshHandleDesc {
            general = wallObjectColors,
            vertices = moveHandleVertices,
            verticesOrder = oneQuadOrder,
            verticesRotation = Quaternion.identity,
            vertsLocalToWorld = Matrix4x4.identity,
            renderType = VerticesMeshHandleDesc.RenderType.QUADS,
            cullMode = CullMode.Off,
            wireframe = true,
            staticSize = true
        };
        public  static CircleHandleDesc       wallRotationHandleDesc = new CircleHandleDesc {
            general = wallObjectRotationColors,
            angle = 0,
            normal = Vector3.up,
            size = HandlesStyles.rotationHandleSize,
        };
        
        // Wall wing
        public  static Color                  wallLineColor = HexColor(Colors.wallObject, 0.2f);
        public  static VerticesMeshHandleDesc wallGrowHandleDesc = new VerticesMeshHandleDesc {
            general = wallObjectColors,
            vertices = new Vector3[] {
                new Vector3(-0.08f,  0.15f, 0.025f),
                new Vector3( 0.08f,  0.15f, 0.025f),
                new Vector3( 0.08f, -0.15f, 0.025f),
                new Vector3(-0.08f, -0.15f, 0.025f)
            },
            verticesOrder = oneQuadOrder,
            verticesRotation = Quaternion.identity,
            vertsLocalToWorld = Matrix4x4.identity,
            renderType = VerticesMeshHandleDesc.RenderType.QUADS,
            cullMode = CullMode.Off,
            wireframe = true,
            staticSize = false
        };
        
        public static Color HexColor(uint color) {
            float r = (float)((color >> 24) & 0xFF) / 255f;
            float g = (float)((color >> 16) & 0xFF) / 255f;
            float b = (float)((color >>  8) & 0xFF) / 255f;
            float a = (float)((color      ) & 0xFF) / 255f;
            
            return new Color(r, g, b, a);
        }
        public static Color HexColor(uint color, float a) {
            float r = (float)((color >> 16) & 0xFF) / 255f;
            float g = (float)((color >>  8) & 0xFF) / 255f;
            float b = (float)((color      ) & 0xFF) / 255f;
            
            return new Color(r, g, b, a);
        }
    }
}