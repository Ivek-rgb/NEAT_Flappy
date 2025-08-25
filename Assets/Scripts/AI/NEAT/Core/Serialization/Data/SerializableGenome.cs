using System;
using System.Collections.Generic;

namespace AI.NEAT.Core.Serialization.Data
{
    [Serializable]
    public class SerializableGenome
    {
        public float fitness;
        public List<SerializableNode> neurons;
        public List<SerializableConnection> connections;
    }
}