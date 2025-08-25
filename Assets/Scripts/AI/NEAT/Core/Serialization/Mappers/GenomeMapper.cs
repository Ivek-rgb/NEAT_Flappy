using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Core.ModelParts;
using AI.NEAT.Core.Serialization.Data;
using Unity.VisualScripting;
using UnityEditor.Overlays;

namespace AI.NEAT.Core.Serialization.Mappers
{
    public class GenomeMapper
    {
        
        public static SerializableGenome ToSerializable(Genome genome)
        {
            return new SerializableGenome
            {
                fitness = genome.Fitness,
                neurons = NodeMapper.ToSerializableList(genome.NodeIdLookup.Values),
                connections = ConnectionMapper.ToSerializableList(genome.EdgeInnoLookup.Values)
            };
        }
        
        public static Genome FromSerializable(SerializableGenome dto)
        {

            Dictionary<int, NodeGene> nodeLookup = NodeMapper.FromSerializableList(dto.neurons).ToDictionary(n => n.ID);
            Dictionary<int, EdgeGene> edgeLookup = ConnectionMapper.FromSerializableList(dto.connections)
                .ToDictionary(e => e.InnovationNumber);

            Dictionary<int, List<int>> nodeIdToConnections = new Dictionary<int, List<int>>();


            foreach (var connection in edgeLookup.Values)
            {
                
                if (nodeIdToConnections.ContainsKey(connection.FromNode))
                    nodeIdToConnections[connection.FromNode].Add(connection.ToNode);
                else nodeIdToConnections.Add(connection.FromNode, new List<int>(){connection.ToNode});

            }

            return new Genome{
                Fitness = dto.fitness,
                NodeIdLookup = nodeLookup,
                EdgeInnoLookup = edgeLookup,
                NodeIdToConnections = nodeIdToConnections
            };
            
        }
        
        public static List<SerializableGenome> ToSerializableList(IEnumerable<Genome> genomes)
            => genomes.Select(ToSerializable).ToList();

        public static List<Genome> FromSerializableList(IEnumerable<SerializableGenome> dtos)
            => dtos.Select(FromSerializable).ToList();
        
    }
}