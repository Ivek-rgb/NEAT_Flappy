using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utils;
using Random = System.Random;

namespace AI.NEAT.Core.ModelParts
{
    
    public class Genome
    {

        public Dictionary<int, NodeGene> NodeIdLookup { get; set; } = new();
        public Dictionary<int, EdgeGene> EdgeInnoLookup { get; set; } = new();
        public Dictionary<int, List<int>> NodeIdToConnections { get; set; } = new();

        public float Fitness { get; set; }
        
        private readonly Random _rand = RandomUtils.Rand;
        public InnovationTracker Tracker { get; set; }

        private bool _isLayerDirty = false;

        public int SpeciesId { get; set; }

        public void EnsureLayerAssignment()
        {
            if (_isLayerDirty)
            {
                TopologicalSort();
                _isLayerDirty = false; 
            }
        }

        public void ForceLayerAssignment()
        {
            TopologicalSort();
            _isLayerDirty = false; 
        }
        
        private void TopologicalSort()
        {

            Dictionary<int, int> connectedTo = new Dictionary<int, int>();

            foreach (var node in NodeIdLookup.Values)
            {
                if (!NodeIdToConnections.ContainsKey(node.ID))
                    NodeIdToConnections.Add(node.ID, new List<int>());

                connectedTo[node.ID] = 0; 
            }


            foreach (var connection in EdgeInnoLookup.Values)
            {
                if (!connection.Enabled) continue;
                connectedTo[connection.ToNode]++; 
            }

            Queue<int> searchQueue = new Queue<int>(); 
            
            foreach (var node in NodeIdLookup.Values)
            {
                if (connectedTo[node.ID] == 0 && node.Type != NodeType.Output)
                {
                    searchQueue.Enqueue(node.ID);
                    node.Layer = 0; 
                }
            }

            while (searchQueue.Count > 0)
            {

                var nodeId = searchQueue.Dequeue();
                var currentLayer = NodeIdLookup[nodeId].Layer;

                foreach (var neighbor in NodeIdToConnections[nodeId])
                {

                    connectedTo[neighbor]--; 
                    
                    NodeIdLookup[neighbor].Layer = Math.Max(NodeIdLookup[neighbor].Layer, currentLayer + 1); 
                    
                    if(connectedTo[neighbor] == 0)
                        searchQueue.Enqueue(neighbor);
                    
                }

            }
            
            int maxLayer = (NodeIdLookup.Count > 0 ? NodeIdLookup.Values.Select(n => n.Layer).Max() : 0) + 1;
            
            foreach (var node in NodeIdLookup.Values.Where(n => n.Type == NodeType.Output))
                node.Layer = maxLayer; 
            
        }

        private void RecordConnectionTowardsGenome(int formId, int toId)
        {

            if (NodeIdToConnections.ContainsKey(formId))
                NodeIdToConnections[formId].Add(toId);
            else
                NodeIdToConnections.Add(formId, new List<int>(){toId});

        }

        

        public static Genome CreateNodesOnlyMinimal(int inputNodes, int outputNodes, InnovationTracker innovationTracker, int[] hiddenLayers = null)
        {
            
            var retGenome = new Genome();
            int nodeId = 0;

            var bias = new NodeGene(nodeId++, NodeType.Bias, 0, ActivationFunction.Identity);
            bias.Value = 1; 
            retGenome.NodeIdLookup.Add(nodeId - 1, bias);

            for (int i = 0; i < inputNodes; i++) {
                retGenome.NodeIdLookup.Add(nodeId, new NodeGene(
                    nodeId++,
                    NodeType.Input,
                    0f,
                    0,
                    ActivationFunction.Identity
                ));
            }

            int layerCounter = 1; 
            if (hiddenLayers is { Length: > 0 })
            {
                for (; layerCounter <= hiddenLayers.Length; layerCounter++) {
                    for (int j = 0; j < hiddenLayers[layerCounter - 1]; j++)
                        retGenome.NodeIdLookup.Add(nodeId, new NodeGene(
                                nodeId++,
                                NodeType.Hidden,
                                0f,
                                layerCounter,
                                ActivationFunction.Sigmoid
                            ));
                    
                }
            }

            for (int i = 0; i < outputNodes; i++) {
                retGenome.NodeIdLookup.Add(nodeId, new NodeGene(
                    nodeId++,
                    NodeType.Output,
                    1f,
                    layerCounter,
                    ActivationFunction.Sigmoid
                ));
            }
            
            retGenome.Tracker = innovationTracker;
            retGenome._isLayerDirty = false; 
            return retGenome; 

        }

        public static Genome CreateFullyConnectedMinimal(int inputNodes, int outputNodes, InnovationTracker innovationTracker, int[] hiddenLayers = null)
        {

            var retGenome = CreateNodesOnlyMinimal(inputNodes, outputNodes, innovationTracker, hiddenLayers);
            
            int layersLength = 2;
            if (hiddenLayers != null)
                layersLength += hiddenLayers.Length; 
            
            int[] layerCount = new int[layersLength];

            layerCount[0] = inputNodes + 1;
            layerCount[^1] = outputNodes; 
            
            if(hiddenLayers != null)
                for (int i = 0; i < hiddenLayers.Length; i++)
                    layerCount[i + 1] = hiddenLayers[i];

            int passedNodes = 0;

            for (int i = 0; i < layerCount.Length - 1; i++)
            {
                int currTracker = passedNodes; 
                passedNodes += layerCount[i];
                for (int j = 0; j < layerCount[i]; j++) {
                    for (int k = 0; k < layerCount[i + 1]; k++) {
                        
                        int fromId = retGenome.NodeIdLookup.Values.ElementAt(currTracker + j).ID;
                        int toId = retGenome.NodeIdLookup.Values.ElementAt(passedNodes + k).ID;

                        int innovationNum = innovationTracker.GetInnovationNumber(fromId, toId);  
                        
                        retGenome.EdgeInnoLookup.Add(innovationNum, new EdgeGene(
                            fromId, toId,
                            true, innovationNum
                        ));

                    }
                }

            }

            retGenome._isLayerDirty = true; 
            return retGenome;
        }

        public void MutateAddConnection()
        {

            EnsureLayerAssignment();
            
            var validPairs = new List<(NodeGene, NodeGene)>();
            var connectionSet = new HashSet<(int, int)>(EdgeInnoLookup.Values.Select(c => (FromGene: c.FromNode, ToGene: c.ToNode)));
            
            foreach (var from in NodeIdLookup.Values)
            {
                if (from.Type == NodeType.Output) continue; 
                
                foreach (var to in NodeIdLookup.Values)
                {
                    
                    if (from.Layer >= to.Layer || to.Type == NodeType.Input || from.ID == to.ID || connectionSet.Contains((from.ID, to.ID))) continue;
                    
                    validPairs.Add((from, to));
                    
                }
                
            }
            
            if (validPairs.Count == 0) return; 
            var (fromNode, toNode) = validPairs[_rand.Next(validPairs.Count)];
            
            int innovation = Tracker.GetInnovationNumber(fromNode.ID, toNode.ID);
            EdgeInnoLookup.Add(innovation, new EdgeGene(fromNode.ID, toNode.ID, RandomUtils.RandomFloatRange(-1f, 1f), true, innovation));
            RecordConnectionTowardsGenome(fromNode.ID, toNode.ID);
            
            _isLayerDirty = true; 

        }

        public void MutateAddNode()
        {
            
            var enabledConnections = EdgeInnoLookup.Values.Where(c => c.Enabled).ToList();
            
            if (enabledConnections.Count < 1) return; 
            
            var connection = enabledConnections[_rand.Next(enabledConnections.Count)];
            EdgeInnoLookup.Remove(connection.InnovationNumber); 

            int newNodeId = NodeIdLookup.Values.Select(n => n.ID).Max() + 1;
            var newNode = new NodeGene(newNodeId, NodeType.Hidden); 
            NodeIdLookup.Add(newNodeId, newNode);

            int inno1 = Tracker.GetInnovationNumber(connection.FromNode, newNodeId);
            int inno2 = Tracker.GetInnovationNumber(newNodeId, connection.ToNode); 
            
            EdgeInnoLookup.Add(inno1, new EdgeGene(connection.FromNode, newNodeId, 1f, true, inno1));
            EdgeInnoLookup.Add(inno2, new EdgeGene(newNodeId, connection.ToNode, connection.Weight, true, inno2));
            
            RecordConnectionTowardsGenome(connection.FromNode, newNodeId);
            RecordConnectionTowardsGenome(newNodeId, connection.ToNode);
            
            _isLayerDirty = true; 
        }
        
        public void MutateWeights(float mutationRate = 0.8f, float perturbChance = 0.9f)
        {

            foreach (var conn in EdgeInnoLookup.Values)
            {
                
                if (_rand.NextDouble() < mutationRate)
                    conn.Weight = _rand.NextDouble() < perturbChance
                        ? conn.Weight + RandomUtils.RandomFloatRange(-0.5f, 0.5f)
                        : RandomUtils.RandomFloatRange(-1f, 1f);

            }

        }

        public Genome Crossover(Genome other)
        {
            var child = new Genome();
            
            var (dominant, nonDominant) = Fitness > other.Fitness ? (this, other) : (other, this);
            bool equalFitness = Math.Abs(Fitness - other.Fitness) < 1e-6; 

            var usedNodes = new HashSet<int>();

            var edgesInno1 = dominant.EdgeInnoLookup.Values.ToDictionary(e => e.InnovationNumber);
            var edgesInno2 = nonDominant.EdgeInnoLookup.Values.ToDictionary(e => e.InnovationNumber);

            var allInnovationNumbers = new HashSet<int>(edgesInno1.Keys.Concat(edgesInno2.Keys));

            foreach (var node in dominant.NodeIdLookup.Values)
            {
                usedNodes.Add(node.ID); 
            }
            
            Dictionary<int, NodeGene> allNodeLookupTable = new Dictionary<int, NodeGene>(NodeIdLookup);

            foreach (var pair in other.NodeIdLookup) {
                if (!allNodeLookupTable.ContainsKey(pair.Key))
                    allNodeLookupTable.Add(pair.Key, pair.Value);                    
            }

            foreach (var innoNumber in allInnovationNumbers.OrderBy(i => i))
            {
                
                bool has1 = edgesInno1.TryGetValue(innoNumber, out var gene1);
                bool has2 = edgesInno2.TryGetValue(innoNumber, out var gene2);

                EdgeGene selectedGene; 

                if (has1 && has2)
                {
                    
                    selectedGene = _rand.NextDouble() < 0.5
                        ? gene1.Clone()
                        : gene2.Clone();
                    
                    if (!selectedGene.Enabled && gene2.Enabled)
                        selectedGene.Enabled = _rand.NextDouble() < 0.25; 
                    else if (selectedGene.Enabled || gene2.Enabled)
                        selectedGene.Enabled = true;
                    else
                        selectedGene.Enabled = false;
                    
                }
                else
                {

                    if (equalFitness && !has1 && has2)
                        selectedGene = gene2.Clone(); 
                    else if (has1)
                        selectedGene = gene1.Clone();
                    else continue;
                    
                }
                
                child.EdgeInnoLookup.Add(selectedGene.InnovationNumber, selectedGene);
                child.RecordConnectionTowardsGenome(selectedGene.FromNode, selectedGene.ToNode);
                usedNodes.Add(selectedGene.FromNode);
                usedNodes.Add(selectedGene.ToNode); 

            }
            
            foreach (var node in usedNodes)
                child.NodeIdLookup.Add(node, allNodeLookupTable[node].Clone());

            child.ForceLayerAssignment();
            
            return child; 
        }
        
        public Genome Clone()
        {
            
            var newGenome = new Genome
            {
                Fitness = Fitness,
                NodeIdLookup = NodeIdLookup.ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value.Clone()
                    ), 
                EdgeInnoLookup = EdgeInnoLookup.ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value.Clone()
                    ),
                Tracker = Tracker,
                _isLayerDirty = _isLayerDirty
            };
            
            return newGenome; 
        }

    }

}
