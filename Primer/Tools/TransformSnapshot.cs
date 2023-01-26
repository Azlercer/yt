using UnityEngine;

namespace Primer
{
    public class TransformSnapshot
    {
        private readonly Transform target;
        private readonly Transform parent;

        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly Vector3 localScale;


        public TransformSnapshot(Transform target)
        {
            this.target = target;
            parent = target.parent;
            position = target.localPosition;
            rotation = target.localRotation;
            localScale = target.localScale;
        }


        public void Restore() => ApplyTo(target);

        public void ApplyTo(Transform other, Vector3? offsetPosition = null)
        {
            if (other.parent != parent)
                other.parent = parent;

            other.localRotation = rotation;
            other.localScale = localScale;

            if (offsetPosition is not null)
                other.localPosition = position + (rotation * offsetPosition.Value);
            else
                other.localPosition = position;

        }


        public override string ToString()
            => $"pos({position}) rot({rotation}) scale({localScale})";
    }
}
