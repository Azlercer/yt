using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Simulation
{
    public class GameTheorySim<T> where T : Enum
    {
        private readonly RewardMatrix<T> rewardMatrix;
        private readonly float baseFitness;
        private readonly float stepSize;

        public GameTheorySim(RewardMatrix<T> rewardMatrix, float baseFitness = 1, float stepSize = 0.1f)
        {
            this.rewardMatrix = rewardMatrix;
            this.baseFitness = baseFitness;
            this.stepSize = stepSize;
        }

        public IEnumerable<AlleleFrequency<T>> Simulate(AlleleFrequency<T> initial, int maxIterations = 1000, float minDelta = 0.01f)
        {
            var last = initial;
            yield return initial;

            for (var i = 0; i < maxIterations; i++) {
                var current = SingleIteration(last);

                yield return current;

                if (last.Delta(current) < minDelta) {
                    Debug.Log($"Delta is below threshold after {i} iterations: {last.Delta(current)} < {minDelta}");
                    break;
                }

                last = current;
            }
        }

        private AlleleFrequency<T> SingleIteration(AlleleFrequency<T> previous)
        {
            var difference = CalculateDifference(previous);
            var result = new AlleleFrequency<T>();
            
            foreach (var (strategy, _) in previous)
            {
                result[strategy] = previous[strategy] + stepSize * difference[strategy];
            }

            result.Normalize();
            return result;
        }

        // Pulled this out to make it easier to make a vector field visualization
        public AlleleFrequency<T> CalculateDifference(AlleleFrequency<T> currentState)
        {
            var list = currentState
                .Select(x => (
                    strategy: x.Key,
                    frequency: x.Value,
                    fitness: CalculateFitness(x.Key, currentState)
                ))
                .ToList();
            
            var avgFitness = list.Average(x => x.fitness);

            var difference = new AlleleFrequency<T>();
            
            foreach (var (strategy, frequency, fitness) in list) {
                difference[strategy] = frequency * (fitness - avgFitness) / avgFitness;
            }

            return difference;
        }

        private float CalculateFitness(T strategy, AlleleFrequency<T> previous)
        {
            return previous.Sum(x => x.Value * rewardMatrix[strategy, x.Key]) + baseFitness;
        }
    }
}
