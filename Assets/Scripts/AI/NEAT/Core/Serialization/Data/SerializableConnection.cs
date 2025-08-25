using System;

namespace AI.NEAT.Core.Serialization.Data
{
    
    [Serializable]
    public class SerializableConnection
    {
        public int id;
        public int fromGene;
        public int toGene;
        public float weight;
        public int innovationNumber;
        public bool enabled; 
    }
}