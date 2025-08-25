using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Core.ModelParts;
using AI.NEAT.Core.Serialization.Data;

namespace AI.NEAT.Core.Serialization.Mappers
{
    public static class ConnectionMapper
    {
        
        public static SerializableConnection ToSerializable(EdgeGene conn)
        {
            return new SerializableConnection
            {
                fromGene = conn.FromNode,
                toGene = conn.ToNode,
                weight = conn.Weight,
                enabled = conn.Enabled,
                innovationNumber = conn.InnovationNumber
            };
        }
        
        public static EdgeGene FromSerializable(SerializableConnection dto)
        {
            return new EdgeGene(
                dto.fromGene,
                dto.toGene,
                dto.weight,
                dto.enabled,
                dto.innovationNumber
            );
        }
        
        public static List<SerializableConnection> ToSerializableList(IEnumerable<EdgeGene> conns)
            => conns.Select(ToSerializable).ToList();
    
        public static List<EdgeGene> FromSerializableList(IEnumerable<SerializableConnection> dtos)
            => dtos.Select(FromSerializable).ToList();
        
    }
}