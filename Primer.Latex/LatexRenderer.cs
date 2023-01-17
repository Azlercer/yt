using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Primer.Latex
{
    [ExecuteAlways]
    [HideMonoScript]
    [AddComponentMenu("Primer / Latex Renderer")]
    public class LatexRenderer : MonoBehaviour
    {
        internal readonly LatexProcessor processor = LatexProcessor.GetInstance();
        internal LatexExpression expression = new();

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("groupIndexesInternal")]
        internal List<int> groupIndexes = new();

        [SerializeField]
        [PropertyOrder(2)]
        [HideLabel, Title("LaTeX")]
        [PropertySpace(SpaceBefore = 32, SpaceAfter = 32)]
        [Multiline(10)]
        internal string latex = "";

        [FoldoutGroup("Details", order: 10)]
        public Material material;

        [SerializeField]
        [FoldoutGroup("Details")]
        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        internal List<string> headers = LatexInput.GetDefaultHeaders();

        [FoldoutGroup("Details")]
        public UnityEvent<LatexExpression> onChange = new();


        private LatexTransitionState stateCache;
        internal LatexTransitionState state => stateCache ??= new LatexTransitionState(
            this,
            expression.Split(groupIndexes)
        );

        public LatexInput config => new(latex, headers);

        internal bool isRunning => processor.state == LatexProcessingState.Processing;
        internal bool isCancelled => processor.state == LatexProcessingState.Cancelled;


        private async void OnValidate()
        {
            await Process(config);

            // Comment this to disable automatic children update
            if (this)
                UpdateChildren();
        }

        public async Task Process(LatexInput input)
        {
            var prevExpression = expression;
            expression = await processor.Process(input);

            if (!prevExpression.IsSame(expression))
                onChange.Invoke(expression);
        }


#if UNITY_EDITOR
        // This needs to be private (or internal) because SpriteDirectRenderer is internal
        [SerializeField]
        [FoldoutGroup("Details")]
        [Tooltip("Which mesh features to visualize. Gizmos are only ever visible in the Unity editor.")]
        internal LatexGizmoMode gizmos = LatexGizmoMode.Nothing;

        public List<(int start, int end)> ranges => expression.CalculateRanges(groupIndexes);

        private void OnDrawGizmos()
        {
            foreach (var character in expression)
                character.DrawWireGizmos(transform, gizmos);
        }

        private void Reset()
        {
            // A default preset will automatically get applied when we're reset.
            // If we unconditionally set material here, we'll blow away the value it set.
            var presets = Preset.GetDefaultPresetsForObject(this);

            if (presets.All(preset => preset.excludedProperties.Contains("material"))) {
                material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            }
        }

        [ResponsiveButtonGroup(Order = 1)]
        [Button("Open Build Directory")]
        private void OpenBuildDir() => processor.OpenBuildDir();

        [DisableIf(nameof(isRunning))]
        [ResponsiveButtonGroup]
        [Button("Cancel Rendering Task")]
        private void Cancel() => processor.Cancel();

        [ResponsiveButtonGroup]
        [Button("Update children")]
        public void UpdateChildren()
        {
            if (gameObject.IsPreset())
                return;

            var zero = expression.GetCenter();
            var groupGameObjects = new ChildrenDeclaration(transform);

            foreach (var (start, end) in expression.CalculateRanges(groupIndexes)) {
                var chunk = expression.Slice(start, end);
                var group = groupGameObjects.Next($"Group (chars {start} to {end - 1})");
                var children = new ChildrenDeclaration(group);
                var center = chunk.GetCenter();

                group.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));
                group.localScale = Vector3.one;

                foreach (var character in chunk) {
                    var charTransform = children.Next($"LatexChar {character.position}");
                    charTransform.localPosition = character.position - group.localPosition; //- center + zero;

                    var meshFilter = charTransform.GetOrAddComponent<MeshFilter>();
                    meshFilter.sharedMesh = character.symbol.mesh;

                    var meshRenderer = charTransform.GetOrAddComponent<MeshRenderer>();
                    meshRenderer.material = material;
                }

                children.Apply();
            }

            groupGameObjects.Apply();
        }
#endif
    }
}
