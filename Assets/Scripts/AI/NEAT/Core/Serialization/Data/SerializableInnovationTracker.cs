using System;
using System.Collections.Generic;

namespace AI.NEAT.Core.Serialization.Data
{
    [Serializable]
    public class SerializableInnovationTracker
    {
        public List<string> keys;   
        public List<int> values;    
        public int currentInnovation;
    }
}