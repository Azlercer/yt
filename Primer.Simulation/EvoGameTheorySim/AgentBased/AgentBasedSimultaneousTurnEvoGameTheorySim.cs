using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using Primer.Simulation.Genome.Strategy;
using Sirenix.Utilities;
using UnityEngine;

namespace Simulation.GameTheory
{
    public enum HomeOptions
    {
        Random,
        ChooseNearestEveryDay,
        Keep
    }

    public enum TreeSelectionOptions
    {
        Random,
        PreferNearest,
    }
    
    public enum ReproductionType
    {
        Sexual,
        Asexual
    }
    
    public class AgentBasedSimultaneousTurnEvoGameTheorySim : ISimulation, IPrimer, IDisposable
    {
        private HomeOptions _homeOptions;
        private TreeSelectionOptions _treeSelectionOptions;
        private ReproductionType _reproductionType;
        private bool _lowRes;
        
        public int turn = 0;
        
        private Landscape terrain => transform.GetComponentInChildren<Landscape>();
        public FruitTree[] trees => transform.GetComponentsInChildren<FruitTree>();
        public Home[] homes => transform.GetComponentsInChildren<Home>();
        
        private Transform _creatureParent;
        private static Pool creaturePool => Pool.GetPool(CommonPrefabs.Blob);

        private readonly SimultaneousTurnGameAgentHandler _simultaneousTurnGameAgentHandler;

        public Rng rng { get; }
        
        private bool _skipAnimations; 
        public bool skipAnimations { 
            get => _skipAnimations;
            set
            {
                _skipAnimations = value;
                foreach (var tree in trees) tree.skipAnimations = skipAnimations;
                foreach (var agent in creatures) agent.skipAnimations = skipAnimations;
            }
        }
        public Transform transform { get; }
        public Component component => transform;

        public IEnumerable<SimultaneousTurnCreature> creatures =>
            _creatureParent.ChildComponents<SimultaneousTurnCreature>().Where(x => x.gameObject.activeSelf);
        public IEnumerable<SimultaneousTurnStrategyGene> alleles => creatures.SelectMany(x => x.strategyGenes.GetAlleles());
        public float GetFrequency(Type allele) => alleles.Count(x => x.GetType() == allele) / (float)alleles.Count();
        public int currentCreatureCount => _creatureParent.GetActiveChildren().Count();
        
        // Constructor that accepts a list of creatures instead of a dictionary
        public AgentBasedSimultaneousTurnEvoGameTheorySim(
            Transform transform,
            List<SimultaneousTurnCreature> initialBlobs,
            SimultaneousTurnGameAgentHandler simultaneousTurnGameAgentHandler,
            Transform creatureParent,
            Rng rng = null,
            bool skipAnimations = false,
            HomeOptions homeOptions = HomeOptions.Random,
            TreeSelectionOptions treeSelectionOptions = TreeSelectionOptions.Random,
            ReproductionType reproductionType = ReproductionType.Asexual,
            bool lowRes = false
            )
        {
            this.transform = transform;
            _creatureParent = creatureParent;
            _simultaneousTurnGameAgentHandler = simultaneousTurnGameAgentHandler;
            _homeOptions = homeOptions;
            _treeSelectionOptions = treeSelectionOptions;
            _reproductionType = reproductionType;
            _lowRes = lowRes;

            this.rng = rng;
            this.skipAnimations = skipAnimations;

            ConfigureInitialBlobs(initialBlobs);

            foreach (var tree in trees)
            {
                tree.rng = rng;
            }
            foreach (var home in homes)
            {
                home.OrderTreesByDistance();
            }
        }

        private void ConfigureInitialBlobs(List<SimultaneousTurnCreature> initialBlobs)
        {
            _creatureParent.GetChildren().ForEach(x => creaturePool.Return(x));
            
            foreach (var creature in initialBlobs)
            {
                creature.transform.SetParent(_creatureParent.transform);
                creature.gameObject.SetActive(true);
                creature.landscape = terrain;
                creature.rng = rng;
                _simultaneousTurnGameAgentHandler.OnAgentCreated(creature);
            }
            ChooseHomes();
        }

        public Tween CreateFood()
        {
            foreach (var creature in creatures)
            {
                creature.PurgeStomach();
            }
            
            return trees.Select(x => x.GrowRandomFruitsUpToTotal(total: 2, delayRange: 1)).RunInParallel();
        }

        public Tween AgentsGoToTrees()
        {
            // Make creatures each go to a random tree, but a maximum of two per tree
            var trees1 = trees.ToList();
            var trees2 = trees.ToList();

            // var goToTreesTweens = Tween.noop;
            // var deathTweens = Tween.noop;
            
            var goToTreesTweenList = new List<Tween>();
            var deathTweenList = new List<Tween>();
            var currentHomes = new List<Home>();
            
            switch (_treeSelectionOptions)
            {
                case TreeSelectionOptions.Random:
                    // Make creatures each go to a random tree, but a maximum of two per tree
                    var treeSlots = trees.Concat(trees).Shuffle(rng);
                    var shuffledCreatures = creatures.Shuffle(rng);
                    goToTreesTweenList = new List<Tween>();
                    
                    for (var i = 0; i < shuffledCreatures.Count; i++)
                    {
                        var creature = shuffledCreatures[i];
                        if (i < treeSlots.Count)
                        {
                            creature.GoToEat(treeSlots[i]);
                            if (!currentHomes.Contains(creature.home))
                            {
                                currentHomes.Add(creature.home);
                            }
                        }
                        else
                        {
                            deathTweenList.Add(creature.ScaleTo(0).Observe(onComplete: () => creaturePool.Return(creature.transform)));
                        }
                    }
                    
                    // goToTreesTweens = creatures
                    //     .Take(treeSlots.Count)
                    //     .Zip(treeSlots, (creature, tree) => creature.GoToEat(tree))
                    //     .RunInParallel();
                    // deathTweens = shuffledCreatures
                    //     .Skip(treeSlots.Count)
                    //     .Select(creature => creature.ScaleTo(0).Observe(onComplete: () => creature.gameObject.SetActive(false)))
                    //     .RunInParallel();
                    break;
                case TreeSelectionOptions.PreferNearest:
                    foreach (var creature in creatures.Shuffle(rng))
                    {
                        if (!currentHomes.Contains(creature.home))
                        {
                            currentHomes.Add(creature.home);
                        }
                        var foundTree = false;
                        foreach (var tree in creature.home.treesByDistance)
                        {
                            if (!trees1.Contains(tree)) continue;
                            goToTreesTweenList.Add(creature.GoToEat(tree, fruitIndex: 0));
                            trees1.Remove(tree);
                            foundTree = true;
                            break;
                        }
                        if (foundTree) continue;
                        foreach (var tree in creature.home.treesByDistance)
                        {
                            if (!trees2.Contains(tree)) continue;
                            goToTreesTweenList.Add(creature.GoToEat(tree, fruitIndex: 1));
                            trees2.Remove(tree);
                            foundTree = true;
                            break;
                        }
                        if (foundTree) continue;
                        deathTweenList.Add(creature.ScaleTo(0).Observe(onComplete: () => creaturePool.Return(creature.transform)));
                    }
                    break;
                default:
                    Debug.LogError("Tree selection option not implemented");
                    return null;
            }
            return Tween.Parallel(
                currentHomes.Select(x => x.Open()).RunInParallel(),
                goToTreesTweenList.RunInParallel() with {delay = 0.25f},
                deathTweenList.RunInParallel() with {delay = 0.25f},
                currentHomes.Select(x => x.Close() with {delay = 0.5f}).RunInParallel()
            );
        }

        public Tween AgentsEatFood()
        {
            // Make creatures eat food, but only creatures where goingToEat is not null
            return creatures
                .Where(creature => creature.goingToEat != null)
                .GroupBy(x => x.goingToEat)
                .Select(x => Eat(competitors: x.ToList(), tree: x.Key))
                .RunInParallel();
        }

        public Tween AgentsReturnHome()
        {
            ChooseHomes();
            
            // Hax
            var offsets = new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 1),
                new Vector3(-1, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, -1),
                new Vector3(-1, 0, -1),
            };
            offsets.Shuffle(rng);
            
            var returnHomeTweens = new List<Tween>();
            var homesList = new List<Home>();
            var i = 0;
            foreach (var creature in creatures)
            {
                if (!homesList.Contains(creature.home))
                {
                    homesList.Add(creature.home);
                }
                returnHomeTweens.Add(creature.ReturnHome(offset: offsets[i % offsets.Count]));
                i++;
            }

            return Tween.Parallel(
                homesList.Select(x => x.Open()).RunInParallel(),
                returnHomeTweens.RunInParallel(),
                homesList.Select(x => x.Close() with { delay = 0.5f }).RunInParallel()
            );
        }

        public Tween AgentsReproduceOrDie()
        {
            var willReproduce = new List<SimultaneousTurnCreature>();

            foreach (var creature in creatures)
            {
                if (creature.canReproduce) willReproduce.Add(creature);
                else if (!creature.canSurvive) creaturePool.Return(creature.transform);
                
                creature.energy = 0;
                creature.PurgeStomach();
            }

            
            // Get creatures that can reproduce, then group them by home.
            // Then for each group of creatures, choose two creatures at random and make them reproduce.
            // We loop this way even if reproduction is asexual
            var newAgents = new List<SimultaneousTurnCreature>();
            var creaturesByHome = willReproduce.GroupBy(x => x.home);
            foreach (var home in creaturesByHome)
            {
                var creaturesInHome = home.Shuffle(rng).ToList();
                while (creaturesInHome.Count > 1)
                {
                    var (first, second) = creaturesInHome.Take(2).ToList();
                    
                    // Pass the parents in opposite orders so it works in the asexual case
                    newAgents.Add(CreateChild(first, second));
                    newAgents.Add(CreateChild(second, first));
                    
                    creaturesInHome.Remove(first);
                    creaturesInHome.Remove(second);
                }
                
                // Check if anyone is left over
                if (creaturesInHome.Count == 1)
                {
                    var creature = creaturesInHome.First();
                    newAgents.Add(CreateChild(creature, null));
                }
            }
            
            return newAgents.Select(x => x.ScaleTo(1)).RunInParallel();
        }

        private SimultaneousTurnCreature CreateChild(SimultaneousTurnCreature firstParent, SimultaneousTurnCreature secondParent)
        {
            var child = _creatureParent.GetPrefabInstance(CommonPrefabs.Blob).GetOrAddComponent<SimultaneousTurnCreature>();
            if (_lowRes) child.blob.SwapMesh();
            else child.blob.SwapMesh(PrimerBlob.MeshType.HighPolySkinned);
            
            child.home = firstParent.home;
            
            // Inheritance depends on reproduction type
            if (_reproductionType == ReproductionType.Asexual || secondParent == null)
            {
                child.strategyGenes = firstParent.strategyGenes;
            }
            else
            {
                child.strategyGenes = firstParent.strategyGenes.SexuallyReproduce(secondParent.strategyGenes);
                // case ReproductionType.SexualHaploid:
                // {
                //     var strategyGenes = firstParent.strategyGenes
                //         .Zip(secondParent.strategyGenes, (a, b) => rng.rand.NextDouble() < 0.5 ? a : b).ToArray();
                //
                //     child.strategyGenes = strategyGenes;
                //     break;
                // }
                // case ReproductionType.SexualDiploid:
                // {
                //     var numGenes = firstParent.strategyGenes.Length;
                //     if (numGenes % 2 != 0)
                //     {
                //         Debug.LogError("Number of genes must be even for diploid reproduction");
                //         return null;
                //     }
                //     var strategyGenes = new Type[numGenes];
                //     for (var i = 0; i < numGenes / 2; i++)
                //     {
                //         strategyGenes[i] = rng.rand.NextDouble() < 0.5 ? firstParent.strategyGenes[i]
                //             : firstParent.strategyGenes[i + numGenes / 2];
                //     }
                //     for (var i = numGenes / 2; i < numGenes; i++)
                //     {
                //         strategyGenes[i] = rng.rand.NextDouble() < 0.5 ? secondParent.strategyGenes[i]
                //             : secondParent.strategyGenes[i - secondParent.strategyGenes.Length / 2];
                //     }
                //     child.strategyGenes = strategyGenes;
                //     break;
                // }
            }

            child.landscape = terrain;
            child.transform.localPosition = firstParent.transform.localPosition;
            _simultaneousTurnGameAgentHandler.OnAgentCreated(child);
            child.rng = rng;
            child.transform.localScale = Vector3.zero;
            child.energy = 0;

            return child;
        }

        private Tween Eat(List<SimultaneousTurnCreature> competitors, FruitTree tree)
        {
            switch (competitors.Count) {
                case 1:
                {
                    var eatTweens = new List<Tween>();
                    competitors[0].energy++;
                    eatTweens.Add(competitors[0].EatAnimation(tree));

                    if (!tree.hasFruit) return eatTweens.RunInParallel();
                    competitors[0].energy++;
                    eatTweens.Add(competitors[0].EatAnimation(tree));
                    return Tween.Series(eatTweens);
                }
                
                case > 1:
                    return _simultaneousTurnGameAgentHandler.Resolve(competitors, tree);
                
                default:
                    throw new ArgumentException("Cannot eat without creatures", nameof(competitors));
            }
        }

        private void ChooseHomes()
        {
            switch (_homeOptions)
            {
                case HomeOptions.Random:
                    creatures.ForEach(x => x.home = homes.RandomItem(rng));
                    break;
                case HomeOptions.ChooseNearestEveryDay:
                    creatures.ForEach(x => x.home = homes.OrderBy(y => (x.transform.position - y.transform.position).sqrMagnitude).First());
                    break;
                case HomeOptions.Keep:
                    creatures.ForEach(x => x.home ??= homes.RandomItem(rng));
                    break;
                default:
                    Debug.LogError("Home option not implemented");
                    break;
            }
        }

        public void Dispose()
        {
            _creatureParent.GetChildren().ForEach(x => creaturePool.Return(x));
            transform.gameObject.SetActive(false);
        }
    }
}
