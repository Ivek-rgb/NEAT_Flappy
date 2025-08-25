using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Core.ModelParts;
using UnityEngine;
using Utils;

namespace AI.NEAT.Core.PopulationManagement
{
    public class PopulationManager
    {

        public List<Genome> Population { get; set; } = new();
        public InnovationTracker InnovationTracker { get; set; } = new();
        
        public int Generation { get; set; } = 0;
        public int PopulationSize { get; set; }

        public List<Species> SpeciesList { get; set; } = new();

        public float DeltaThreshold;

        public int MaxStaleness { get; set; } = 5;

        public readonly int InputSize;
        public readonly int OutputSize;

        private float _weightChangeChance = 0.8f;
        private float _weightPerturbChance = 0.9f;
        private float _mutateAddConnectionChance = 0.05f;
        private float _mutateAddNodeChance = 0.02f; 
        

        public PopulationManager(int populationSize, int inputSize, int outputSize, int[] hiddenLayers = null, float deltaThreshold = 3.0f, bool fullyConnected = false, int maxStaleness = 5)
        {
            InputSize = inputSize;
            OutputSize = outputSize; 
            MaxStaleness = maxStaleness;
            PopulationSize = populationSize;
            InitializePopulation(InputSize, OutputSize, hiddenLayers, fullyConnected);
            DeltaThreshold = deltaThreshold; 
        }
        
        private PopulationManager(int populationSize, int inputSize, int outputSize, float deltaThreshold = 3.0f)
        {
            InputSize = inputSize;
            OutputSize = outputSize; 
            PopulationSize = populationSize;
            DeltaThreshold = deltaThreshold; 
        }

        public void SetChances(float weightChange, float weightPerturb, float mutateAddConnection, float mutateAddNode)
        {
            _weightChangeChance = weightChange;
            _weightPerturbChance = weightPerturb;
            _mutateAddConnectionChance = mutateAddConnection;
            _mutateAddNodeChance = mutateAddNode; 
        }

        public static PopulationManager LoadUpPopulation(int populationSize, int inputSize, int outputSize, float deltaThreshold, List<Genome> population, InnovationTracker tracker)
        {

            PopulationManager newManager = new PopulationManager(populationSize, inputSize, outputSize, deltaThreshold)
                {
                    Population = population,
                    InnovationTracker = tracker
                };

            foreach (var genome in population)
                genome.Tracker = tracker;

            newManager.SpeciatePopulation();

            return newManager; 
            
        }

        public void SpeciatePopulation(){

            SpeciesList.ForEach(s => s.Reset());

            foreach (var genome in Population)
            {

                bool foundSpecies = false;
                

                for (int i = 0; i < SpeciesList.Count; i++)
                {

                    var species = SpeciesList[i]; 
                    
                    if (GetCompabilityDistance(species.Representative, genome) < DeltaThreshold)
                    {
                        species.Members.Add(genome);
                        foundSpecies = true;
                        break; 
                    }
                    
                }

                if (!foundSpecies)
                {
                    SpeciesList.Add(new Species(genome));
                    genome.SpeciesId = SpeciesList.Count - 1; 
                }

            }

            SpeciesList.RemoveAll(s => s.Members.Count == 0);

            for (int i = 0; i < SpeciesList.Count; i++)
            {
                var species = SpeciesList[i]; 
                foreach (var member in species.Members)
                    member.SpeciesId = i; 
            }

            foreach (var s in SpeciesList)
                s.SetRandRepresentative();
        }

        private static float GetCompabilityDistance(Genome g1, Genome g2)
        {

            const float c1 = 1f, c2 = 1f, c3 = 0.4f;

            var genes1 = g1.EdgeInnoLookup.Values.OrderBy(c => c.InnovationNumber).ToArray();
            var genes2 = g2.EdgeInnoLookup.Values.OrderBy(c => c.InnovationNumber).ToArray();

            int i = 0, j = 0;
            int matching = 0, disjoint = 0;
            float weightDiff = 0f; 

            while (i < genes1.Length && j < genes2.Length)
            {
                
                var gene1 = genes1[i];
                var gene2 = genes2[j];

                if (gene1.InnovationNumber == gene2.InnovationNumber)
                {
                    matching++;
                    weightDiff += Mathf.Abs(gene1.Weight - gene2.Weight);
                    i++;
                    j++; 
                }else if (gene1.InnovationNumber < gene2.InnovationNumber)
                {
                    disjoint++; 
                    i++; 
                }
                else
                {
                    disjoint++;
                    j++; 
                }
               
            }
            
            int excess = genes1.Length - i + genes2.Length - j;
            int N = Mathf.Max(genes1.Length, genes2.Length);
            if (N < 20) N = 1;

            float averageWeightDiff = matching == 0 ? 0 : weightDiff / matching;

            return (c1 * excess / N) + (c2 * disjoint / N) + (c3 * averageWeightDiff); 
        }
 
        private void InitializePopulation(int inputCount, int outputCount, int[] hiddenLayers = null, bool fullyConnected = false)
        {
            for (int i = 0; i < PopulationSize; i++)
            {
                var genome = fullyConnected ? Genome.CreateFullyConnectedMinimal(inputCount, outputCount, InnovationTracker, hiddenLayers) : Genome.CreateNodesOnlyMinimal(inputCount, outputCount, InnovationTracker, hiddenLayers);
                genome.Tracker = InnovationTracker; 
                Population.Add(genome);
            }
            SpeciatePopulation();
            
        }

        public Genome[] TournamentSelection(List<Genome> genomes, int k = 3, int takeFirst = 1)
        {

            k = Mathf.Min(genomes.Count, k);
            takeFirst = Mathf.Min(k, takeFirst);
            
            var tournament = new Genome[k];
            int[] perm = Permutations.GenerateRandomPermutation(genomes.Count, k); 
            
            for (int i = 0; i < k; i++)
                tournament[i] = genomes[perm[i]];

            return tournament.OrderByDescending(g => g.Fitness).Take(takeFirst).ToArray(); 

        }

        public float HighestFitness() => Population.Max(g => g.Fitness); 
        
        public void Evolve(int takeTopNSpecies = -1)
        {
            
            SpeciatePopulation();

            float totalSpeciesAverage = AllSpeciesAverageFitness(); 
            
            foreach (var species in SpeciesList)
            {
                
                float currentBest = species.GetChampion().Fitness;

                if (currentBest > species.BestFitness)
                {
                    species.BestFitness = currentBest;
                    species.Staleness = 0; 
                }
                else
                {
                    species.Staleness++; 
                }

                if (species.AverageFitness() < totalSpeciesAverage)
                {
                    species.Staleness++; 
                }

            }

            if (takeTopNSpecies > 0)
            {
                SpeciesList = SpeciesList.OrderByDescending(s => s.AverageFitness())
                    .Take(takeTopNSpecies)
                    .ToList();
            }


            float highestFitness = HighestFitness();
            
            SpeciesList = SpeciesList.Where(s => s.Staleness < MaxStaleness || s.GetChampion().Fitness >= highestFitness).ToList(); 
            
            List<Genome> nextGeneration = new();
            
            foreach (var species in SpeciesList)
                nextGeneration.Add(species.GetChampion().Clone());


            float totalAdjustedFitness = TotalAdjustedFitness();
            var offspringPlaner = SpeciesList.Select(s =>
            {
                float share = s.AdjustedFitnessSum() / totalAdjustedFitness;
                return Mathf.RoundToInt(share * PopulationSize);
            }).ToArray(); 

            for (int si = 0; si < SpeciesList.Count; si++)
            {

                int offspringCount = Mathf.Clamp(offspringPlaner[si], 0, PopulationSize - nextGeneration.Count);
                
                for (int i = 0; i < offspringCount; i++)
                {

                    Genome parent1 = TournamentSelection(SpeciesList[si].Members, takeFirst:1)[0];
                    Genome parent2 = TournamentSelection(SpeciesList[si].Members, takeFirst:1)[0];

                    Genome child = parent1.Crossover(parent2);

                    child.Tracker = InnovationTracker; 
                    child.MutateWeights(mutationRate: _weightChangeChance, perturbChance: _weightPerturbChance);
                    
                    if (RandomUtils.Rand.NextDouble() < _mutateAddConnectionChance)
                    {
                        child.MutateAddConnection();
                    }

                    if (RandomUtils.Rand.NextDouble() < _mutateAddNodeChance)
                    {
                        child.MutateAddNode();
                    }

                    child.EnsureLayerAssignment();
                    nextGeneration.Add(child);

                    if (nextGeneration.Count >= PopulationSize)
                        break; 

                }

            }
            
            while (nextGeneration.Count < PopulationSize)
            {
                var randomSpecies = SpeciesList[RandomUtils.Rand.Next(SpeciesList.Count)];
                var g = randomSpecies.GetChampion().Clone();
                g.MutateWeights(0.6f, 0.9f);
                nextGeneration.Add(g);
            }

            Population = nextGeneration;
            SpeciatePopulation();
            Generation++; 
        }

        public float AllSpeciesAverageFitness()
        {
            return SpeciesList.Average(s => s.AverageFitness()); 
        }

        public float TotalAdjustedFitness()
        {
            return SpeciesList.Sum(s => s.AdjustedFitnessSum());
        }

        public List<Genome> GetFirstElites(int first) => Population.OrderByDescending(g => g.Fitness).Take(first).ToList(); 

    }
}