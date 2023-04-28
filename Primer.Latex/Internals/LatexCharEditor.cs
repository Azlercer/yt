using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Primer.Latex
{
    public static class LatexCharEditor
    {
        private static int charPreviewSize = 16;

        public static void CharPreviewSize()
        {
            var prev = charPreviewSize;
            charPreviewSize = EditorGUILayout.IntSlider(charPreviewSize, 10, 100);

            if (charPreviewSize != prev)
                cache.Clear();
        }

        public static float GetDefaultWidth()
        {
            var rect = EditorGUILayout.GetControlRect();

            if (rect is not { x: 0, y: 0 })
                return rect.width;

            const int defaultWidthFix = 40;
            return Screen.width - defaultWidthFix;
        }

        public static int ShowGroup(LatexExpression expression, float width)
        {
            var realWidth = width is 1 ? Screen.width : width;
            var totalCharSize = charPreviewSize + 20;
            var cols = Mathf.Max(realWidth / totalCharSize, 1);
            var textures = GetPreviewsFor(expression);

            return GUILayout.SelectionGrid(
                0,
                textures,
                Mathf.FloorToInt(cols),
                GUILayout.Width(width)
            );
        }

        #region Internals
        private static Texture[] GetPreviewsFor(LatexExpression expression)
        {
            return expression
                .Select(character => character.mesh)
                .Where(mesh => mesh is not null)
                .Select(mesh => CreateMeshPreviewTexture(mesh, charPreviewSize))
                .ToArray();
        }

        private static readonly Dictionary<Mesh, Texture> cache = new();
        private static Texture CreateMeshPreviewTexture(Mesh mesh, float size = 32)
        {
            if (cache.ContainsKey(mesh))
                return cache[mesh];

            var previewRender = new PreviewRenderUtility();

            previewRender.BeginPreview(new Rect(0, 0, size, size), new GUIStyle());
            previewRender.DrawMesh(mesh, Matrix4x4.identity, whiteMaterial, 0);

            previewRender.camera.transform.position = new Vector3(0, 0, -4);
            previewRender.lights[0].intensity = 0.7f;
            previewRender.camera.Render();

            var texture = (RenderTexture)previewRender.EndPreview();
            var copy = Clone(texture);

            cache.Add(mesh, copy);
            previewRender.Cleanup();
            return copy;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Texture2D Clone(RenderTexture renderTexture)
        {
            if (renderTexture == null) {
                Debug.LogError("RenderTexture is null.");
                return null;
            }

            var previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = previousActive;
            return texture;
        }

        #region private static Material whiteMaterial;
        private static Material whiteMaterialCache;
        private static Material whiteMaterial => whiteMaterialCache ??= GetWhiteMaterial();
        private static Material GetWhiteMaterial()
        {
            var shader = Shader.Find("Standard");
            return shader == null
                ? null
                : new Material(shader) { color = Color.white };
        }
        #endregion

        #endregion
    }
}
#endif
