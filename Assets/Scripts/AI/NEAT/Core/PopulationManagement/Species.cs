using System;
using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Core.ModelParts;
using Unity.VisualScripting;
using Utils;

namespace AI.NEAT.Core
{
    public class Species
    {

        public Genome Representative;
        public List<Genome> Members { get; set; } = new();
        public float BestFitness = float.MinValue;
        
        public int Staleness = 0;
        
        public Species(Genome representative)
        {
            Representative = representative.Clone();
            Members.Add(representative); 
        }

        public void AddGenome(Genome genome)
        {
            Members.Add(genome);
        }

        public void Reset()
        {
            if (Members.Count <= 0) return; 
            SetRandRepresentative();
            Members.Clear(); 
        }

        public void SetRandRepresentative()
        {
            if (Members.Count <= 0) return;
            Representative = Members[RandomUtils.Rand.Next(Members.Count)];
        }

        public void SetFitRepresentative()
        {
            if (Members.Count <= 0) return;
            Representative = GetChampion(); 
        }

        public Genome GetChampion()
        {
            return Members.OrderByDescending(m => m.Fitness).First(); 
        }

        public float AverageFitness()
        {
            return Members.Average(m => m.Fitness); 
        }

        public float AdjustedFitnessSum()
        {
            return Members.Sum(m => m.Fitness / Members.Count);
        }


    }
    
}