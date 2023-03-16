using System;
using Cysharp.Threading.Tasks;
using Primer.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [Obsolete("Use LatexComponent instead.")]
    [CustomEditor(typeof(DeprecatedLatexRenderer))]
    public class DeprecatedLatexRendererEditor : PrimerEditor<DeprecatedLatexRenderer>
    {
        public static readonly EditorHelpBox targetIsPresetWarning = EditorHelpBox.Warning(
            "You are editing a preset and the LaTeX will not be built until "
            + "you apply the preset to an actual LatexRenderer component."
        );

        internal bool isCancelled => component.processor.state == LatexProcessingState.Cancelled;


        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Convert to LatexComponent")) {
                ConvertToLatexComponent();
                return;
            }

            CacheManagement();

            Space();
            GetStatusBox().Render();

            if (HandleIfPreset())
                return;

            base.OnInspectorGUI();

            if (component.expression is not null && !component.expression.isEmpty)
                RenderGroupDefinition();
        }

        private void ConvertToLatexComponent()
        {
            var go = component.gameObject;
            var newComponent = go.GetOrAddComponent<LatexComponent>();
            newComponent.color = component.color;
            newComponent.material = component.material;
            newComponent.config = component.config;
            newComponent.SetGroupIndexes(component.groupIndexes.ToArray());
            DestroyImmediate(component);
        }

        private void CacheManagement()
        {
            using var scope = new GUILayout.HorizontalScope();

            if (GUILayout.Button("Clear cache")) {
                LatexProcessingCache.ClearCache();
                component.Process(component.config).Forget();
            }

            if (GUILayout.Button("Open cache dir")) {
                LatexProcessingCache.OpenCacheDir();
            }

            LatexProcessingCache.disableCache = GUILayout.Toggle(
                LatexProcessingCache.disableCache,
                "Disable cache"
            );
        }


        private bool HandleIfPreset()
        {
            if (!component.gameObject.IsPreset())
                return false;

            component.expression = new LatexExpression();
            serializedObject.Update();
            targetIsPresetWarning.Render();
            return true;
        }


        private EditorHelpBox GetStatusBox()
        {
            if (isCancelled && component.isRunning)
                return EditorHelpBox.Warning("Cancelling...");

            if (component.isRunning)
                return EditorHelpBox.Warning("Rendering LaTeX...");

            var error = component.processor.renderError;

            return error is null
                ? EditorHelpBox.Info("Ok")
                : EditorHelpBox.Error(error.Message);
        }


        #region Groups controls
        private void RenderGroupDefinition()
        {
            GroupsHeader();

            var serializedGroups = serializedObject.FindProperty(nameof(component.groupIndexes));
            var groupIndexes = serializedGroups.GetIntArrayValue();
            var ranges = component.expression.CalculateRanges(groupIndexes);
            var hasChanges = false;

            for (var i = 0; i < ranges.Count; i++) {
                var (start, end) = ranges[i];

                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label($"Group {i + 1} (chars {start + 1} to {end})");
                    GUILayout.FlexibleSpace();

                    if ((i != 0) && GUILayout.Button("X")) {
                        groupIndexes.RemoveAt(i - 1);
                        hasChanges = true;
                        break;
                    }
                }

                var selected = LatexCharEditor.ShowGroup(component.expression, (start, end));

                if (selected == 0)
                    continue;

                groupIndexes.Insert(i, start + selected);
                hasChanges = true;
            }

            if (hasChanges) {
                serializedGroups.SetIntArrayValue(groupIndexes);
                serializedObject.ApplyModifiedProperties();
                component.InvalidateCache();
            }
        }

        private void GroupsHeader()
        {
            Space();

            using (new GUILayout.HorizontalScope()) {
                GUILayout.Label(
                    "Groups",
                    new GUIStyle {
                        fontSize = 16,
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        normal = { textColor = Color.white },
                    }
                );

                GUILayout.Space(32);
                LatexCharEditor.CharPreviewSize();
            }

            Space();
        }
        #endregion
    }
}