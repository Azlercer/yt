using System;
using UnityEngine;
using Random = System.Random;

namespace Primer
{
    public class Rng
    {
        public static Random staticRandom;
        public static void Initialize(int? seed = null)
        {
            if (seed is null)
            {
                // If seed is null, we don't care what the seed is.
                // So if the staticRandom object already exists, we don't need to do anything.
                // But if it doesn't, create it and set the seed to the current time.
                if (staticRandom is not null) return;
                staticRandom = new Random(Environment.TickCount);
            }
            else
            {
                // If we did give a seed, we should remake the staticRandom object with that seed.
                staticRandom = new Random(seed.Value);
            }
        }
        
        public static int RangeInt(int maxExclusive) => RangeInt(0, maxExclusive);

        public static int RangeInt(int minInclusive, int maxExclusive)
        {
            Initialize();
            return staticRandom.Next(minInclusive, maxExclusive);
        }

        public static float RangeFloat(float maxExclusive) => RangeFloat(0, maxExclusive);

        public static float RangeFloat(float minInclusive, float maxExclusive)
        {
            Initialize();
            return (float) staticRandom.NextDouble() * (maxExclusive - minInclusive) + minInclusive;
        }

        // instance
        public Random rand { get; }

        public Rng(Random rand)
        {
            this.rand = rand;
        }

        public Rng(int seed) : this(new Random(seed))
        {
        }
    }

    // Methods are declared as extension methods so calling on null rngs falls back on static rng with a warning.
    public static class RngExtensions
    {
        public static int RangeInt(this Rng rng, int maxExclusive) => RangeInt(rng, 0, maxExclusive);

        public static int RangeInt(this Rng rng, int minInclusive, int maxExclusive)
        {
            var rand = rng?.rand;
            
            if (rand != null) return rand.Next(minInclusive, maxExclusive);
            Debug.Log("No Rng given, using static rng. If you did this on purpose, use Rng.RangeInt() directly. Otherwise, check that you're passing an the Rng object you want.");
            return Rng.RangeInt(minInclusive, maxExclusive);
        }

        public static float RangeFloat(this Rng rng, float maxExclusive) => RangeFloat(rng, 0, maxExclusive);

        public static float RangeFloat(this Rng rng, float minInclusive, float maxExclusive)
        {
            var rand = rng?.rand;

            if (rand != null) return (float)(rand.NextDouble() * (maxExclusive - minInclusive) + minInclusive);
            Debug.Log("No Rng given, using static rng. If you did this on purpose, use Rng.RangeFloat() directly. Otherwise, check that you're passing an the Rng object you want.");
            return Rng.RangeFloat(minInclusive, maxExclusive);
        }
    }
}
