using System;
using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Core.ModelParts;
using AI.NEAT.Core.Serialization.Data;

namespace AI.NEAT.Core.Serialization.Mappers
{
    public static class NodeMapper
    {
        
        private static readonly Func<float, float> DefaultActivation = ActivationFunction.Tanh;

        private static Func<float, float> GetActivationByName(string name)
        {
            return name switch
            {
                nameof(ActivationFunction.Tanh)    => ActivationFunction.Tanh,
                nameof(ActivationFunction.Sigmoid) => ActivationFunction.Sigmoid,
                nameof(ActivationFunction.ReLU)    => ActivationFunction.ReLU,
                nameof(ActivationFunction.Identity)=> ActivationFunction.Identity,
                _ => DefaultActivation
            };
        }
        
        public static SerializableNode ToSerializable(NodeGene neuronNode)
        {   
            
            return new SerializableNode
            {
                id = neuronNode.ID,
                type = neuronNode.Type,
                layer = neuronNode.Layer, 
                activationFunction = neuronNode.ActivationFunc.Method.Name
            };
            
        }

        public static NodeGene FromSerializable(SerializableNode dto)
        {
            return new NodeGene(dto.id, dto.type, dto.layer, GetActivationByName(dto.activationFunction)); 
        }
        
        public static List<SerializableNode> ToSerializableList(IEnumerable<NodeGene> neurons)
            => neurons.Select(ToSerializable).ToList();
        
        public static List<NodeGene> FromSerializableList(IEnumerable<SerializableNode> dtos)
            => dtos.Select(FromSerializable).ToList();

    }
}