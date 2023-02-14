using UnityEngine;

namespace Primer
{
    public static class TransformExtensions
    {
        public static void RemoveAllChildren(this Transform transform)
        {
            var children = GetChildren(transform);

            foreach (var child in children)
                child.Dispose();
        }

        public static Transform[] GetChildren(this Transform transform)
        {
            var children = new Transform[transform.childCount];

            for (var i = 0; i < transform.childCount; i++)
                children[i] = transform.GetChild(i);

            return children;
        }

        public static void SetPosition(this Transform transform, Vector3 newPosition, bool global = false)
        {
            if (global) {
                transform.position = newPosition;
            }
            else {
                transform.localPosition = newPosition;
            }
        }
    }
}
