using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Core.ModelParts;

namespace AI.NEAT.Core.RuntimeNetwork
{
    
    public class NeatNetwork
    {
        
        private readonly Dictionary<int, NodeGene> _nodes;
        private readonly (NodeGene fromNode, NodeGene toNode, float connectionWeight)[] _fastConnections; 
        
        private readonly int[] _inputNodesIds;
        private readonly int[] _outputNodesIds;

        private readonly float[] _outputValues; 
        
        public NeatNetwork(Genome genome)
        {
            
            genome.ForceLayerAssignment();

            _nodes = genome.NodeIdLookup.ToDictionary(n => n.Key, n => n.Value.Clone());
            
            _fastConnections = genome.EdgeInnoLookup.Values
                .Where(c => c.Enabled)
                .OrderBy(c => _nodes[c.FromNode].Layer)
                .ThenBy(c => _nodes[c.FromNode].ID)
                .ThenBy(c => _nodes[c.ToNode].ID)
                .Select(c => (_nodes[c.FromNode], _nodes[c.ToNode], c.Weight))
                .ToArray();

            _inputNodesIds = _nodes.Values.Where(n => n.Type == NodeType.Input).Select(n => n.ID).ToArray(); 
            _outputNodesIds = _nodes.Values.Where(n => n.Type == NodeType.Output).Select(n => n.ID).ToArray();

            _outputValues = new float[_outputNodesIds.Length]; 

        }
        
        private void ResetNodeValues()
        {
            for (int i = 0; i < _nodes.Values.Count; i++) {
                if (_nodes.Values.ElementAt(i).Type is NodeType.Hidden or NodeType.Output)
                    _nodes.Values.ElementAt(i).Value = 0; 
            }
        }

        public float[] FeedForward(float[] inputs)
        {

            for (int i = 0; i < _inputNodesIds.Length; i++)
            {
                int id = _inputNodesIds[i];
                _nodes[id].Value = inputs[i]; 
            }
            
            ResetNodeValues();

            for (int i = 0; i < _fastConnections.Length; i++)
            {
                var (from, to, w) = _fastConnections[i]; 
                to.Value += from.ActivatedValue * w;
            }

            for (int i = 0; i < _outputValues.Length; i++)
                _outputValues[i] = _nodes[_outputNodesIds[i]].ActivatedValue; 

            return _outputValues; 
            
        }

    }
    
}