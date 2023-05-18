using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {
        public Vector3 position {
            get => transform.position;
            set => transform.position = value;
        }

        public Vector3 localPosition {
            get => transform.localPosition;
            set => transform.localPosition = value;
        }

        public Quaternion rotation {
            get => transform.rotation;
            set => transform.rotation = value;
        }

        public Quaternion localRotation {
            get => transform.localRotation;
            set => transform.localRotation = value;
        }

        public float scale {
            get => transform.localScale.x;
            set => transform.localScale = Vector3.one * value;
        }

        public Vector3 localScale {
            get => transform.localScale;
            set => transform.localScale = value;
        }

        public Vector3 lossyScale => transform.lossyScale;

        public PrimerBehaviour primerCache;
        public PrimerBehaviour primer => primerCache != null ? primerCache : transform.GetPrimer();

        public IEnumerable<Transform> children => usedChildren;

        public IEnumerable<Transform> GetChildren()
        {
            return usedChildren;
        }

        public T GetComponent<T>(bool forceCreate = false) where T : Component
        {
            if (forceCreate)
                return transform.gameObject.AddComponent<T>();

            return transform.GetComponent<T>() ?? transform.gameObject.AddComponent<T>();
        }
    }
}