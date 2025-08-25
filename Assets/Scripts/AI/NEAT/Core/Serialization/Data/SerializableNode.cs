using System;
using AI.NEAT.Core.ModelParts;

namespace AI.NEAT.Core.Serialization.Data
{
    
    [Serializable]
    public class SerializableNode
    {
        
        public int id;
        public NodeType type; 
        public int layer;
        public string activationFunction;
        
    }
    
}