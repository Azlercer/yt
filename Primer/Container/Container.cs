using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public class Container : Container<Transform>
    {
        // These static methods simplify the creation of a container from a component
        public static Container From(Transform component) => new(component);

        public Container(string name, Component parent = null) : base(name, parent) {}
        public Container(Transform t) : base(t) {}

        public static implicit operator Transform(Container container) => container.transform;
    }

    public partial class Container<TComponent> : IDisposable
        where TComponent : Component
    {
        public static Container<T> From<T>(T component) where T : Component => new(component);

        public Action<Transform> onCreate;
        public int childCount => usedChildren.Count;
        public Transform transform { get; }
        public TComponent component { get; }
        public IEnumerable<Transform> extraChildren => new List<Transform>(unusedChildren);

        public Container(string name, Component parent = null)
        {
            transform = parent is null
                ? GetRootTransform(name)
                : GetDirectChild(parent.transform, name);

            component = GetComponent<TComponent>(transform);
            unusedChildren = transform.GetChildren().ToList();
            transform.SetActive(true);
        }

        public Container(TComponent component)
        {
            this.component = component;
            transform = component.transform;
            unusedChildren = transform.GetChildren().ToList();
            transform.SetActive(true);
        }

        // Automatic conversion to TComponent, Component and Transform
        public static implicit operator TComponent(Container<TComponent> container) => container.component;
        public static implicit operator Component(Container<TComponent> container) => container.component;
        public static implicit operator Transform(Container<TComponent> container) => container.transform;
        public static implicit operator Container(Container<TComponent> container) => container as Container;
    }
}
