using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public record PrimerAnimation(float duration = 0.5f, EaseMode ease = EaseMode.Cubic);


    /// <summary>
    ///     These methods are implemented as extensions so they can be called even on `null` objects
    /// </summary>
    public static class PrimerAnimationExtensions {
        private static readonly PrimerAnimation @default = new();

        public static T Lerp<T>(this PrimerAnimation config, T a, T b, float t)
        {
            var animation = config ?? @default;
            var lerp = GetLerpMethod<T>();

            var tEased = Easing.ApplyNormalizedEasing(t, animation.ease);
            var lerped = lerp.Invoke(null, new object[] { a, b, tEased });

            return (T)lerped;
        }

        public async static IAsyncEnumerable<T> Tween<T>(this PrimerAnimation config, T initial, T target,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var startTime = Time.time;
            var animation = config ?? @default;
            var lerp = GetLerpMethod<T>();

            while (!ct.IsCancellationRequested && Time.time < startTime + animation.duration) {
                var t = (Time.time - startTime) / animation.duration;
                var tEased = Easing.ApplyNormalizedEasing(t, animation.ease);
                var lerped = lerp.Invoke(null, new object[] {
                    initial, target, tEased
                });

                yield return (T)lerped;
                await UniTask.DelayFrame(1, cancellationToken: ct);
            }

            if (!ct.IsCancellationRequested)
                yield return target;
        }

        private static MethodInfo GetLerpMethod<T>()
        {
            var lerp = typeof(T).GetMethod("Lerp");

            if (lerp == null) {
                throw new ArgumentException(
                    $"AnimationConfig.tween() couldn't find static .Lerp() in {typeof(T).FullName}");
            }

            return lerp;
        }
    }
}