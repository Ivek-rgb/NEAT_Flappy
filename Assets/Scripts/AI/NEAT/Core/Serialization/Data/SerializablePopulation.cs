using System;
using System.Collections.Generic;

namespace AI.NEAT.Core.Serialization.Data
{
    
    [Serializable]
    public class SerializablePopulation
    {
        
        public int generation;
        public int populationSize;
        public int inputSize;
        public int outputSize;
        
        public SerializableInnovationTracker innovationTracker; // for speciation  
        
        public List<SerializableGenome> population;
        public float deltaThreshold;  // for speciation 

    }
    
}