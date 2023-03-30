using UnityEngine;

namespace Primer
{
    public class TransformSnapshot
    {
        private readonly Transform target;
        private readonly Transform parent;

        public Vector3 position { init; get; }
        public Quaternion rotation { init; get; }
        public Vector3 scale { init; get; }


        public TransformSnapshot(Transform target)
        {
            this.target = target;
            parent = target.parent;
            position = target.localPosition;
            rotation = target.localRotation;
            scale = target.localScale;
        }


        public void Restore() => ApplyTo(target);

        public void ApplyTo(Transform other, Vector3? offsetPosition = null)
        {
            if (other.parent != parent)
                other.parent = parent;

            other.localPosition = offsetPosition is null
                ? position
                : position + rotation * Vector3.Scale(offsetPosition.Value, scale);

            other.localRotation = rotation;
            other.localScale = scale;
        }


        public override string ToString()
        {
            return $"pos({position}) rot({rotation}) scale({scale})";
        }
    }
}