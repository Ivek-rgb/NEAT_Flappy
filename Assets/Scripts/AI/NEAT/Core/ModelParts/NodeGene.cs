using System;
using UnityEngine;

namespace AI.NEAT.Core.ModelParts
{

    public enum NodeType
    {
        Input, Hidden, Output, Bias
    }

    public static class ActivationFunction
    {
        public static float Tanh(float x)
        {
            return (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x));
        }

        public static float Sigmoid(float x)
        {
            return 1f / (1f + Mathf.Exp(-x));
        }

        public static float ReLU(float x)
        {
            return Mathf.Max(x, 0);
        }

        public static float Identity(float x)
        {
            return x;
        }
        
    }

    public class NodeGene
    {

        public int ID { get; set; }
        public NodeType Type { get; set; }
        public float Value { get; set; }
        public int Layer { get; set; } = 0;

        public Func<float, float> ActivationFunc { get; set; } = ActivationFunction.Tanh;

        public float ActivatedValue => ActivationFunc(Value); 
        
        public NodeGene(int id, NodeType type)
        {
            
            ID = id;
            Type = type;
            Value = 0f;
            
        }
        
        public NodeGene(int id, NodeType type, int layer, Func<float, float> activationFunc)
        {
            
            ID = id;
            Type = type;
            Layer = layer;
            ActivationFunc = activationFunc; 
            
        }
        
        public NodeGene(int id, NodeType type, float value, int layer, Func<float, float> activationFunc)
        {
            
            ID = id;
            Type = type;
            Value = value;
            Layer = layer;
            ActivationFunc = activationFunc; 
            
        }

        public NodeGene Clone() => new NodeGene(ID, Type, Value, Layer, ActivationFunc);

    }
    
}
