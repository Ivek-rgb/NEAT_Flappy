using System;
using AI.NEAT.Core;
using AI.NEAT.Core.ModelParts;
using AI.NEAT.Core.RuntimeNetwork;
using AI.NEAT.Interfaces;
using Game.Player;
using UnityEngine;

namespace Game.NEAT
{
    public class NeatAgentWrapper : MonoBehaviour
    {

        public Genome genome;
        private NeatNetwork genomeNetwork; 
        private INeatAgent agent;
        public event Action<NeatAgentWrapper> OnDeactivated;

        [NonSerialized] public int trainerIdx;
        [NonSerialized] public int speciesId; 
        
        void Start()
        {
            agent = GetComponent<INeatAgent>();
            agent.OnDeactivated += OnAgentDeactivatedCallback;
        }

        public void ChangeGenome(Genome newGenome)
        {
            genome = newGenome;
            genomeNetwork = new NeatNetwork(genome);
            speciesId = genome.SpeciesId; 
        }

        private void OnAgentDeactivatedCallback()
        {
            OnDeactivated?.Invoke(this);
            GetFitness(); 
        }

        public void ResetAgent()
        {
            agent.ResetAgent();            
        }

        public void ObserveAndReact()
        {
            
            if (agent.IsDone()) return; 

            float[] obs = agent.GetObservations();
            float[] results = genomeNetwork.FeedForward(obs); 
            
            agent.ApplyAction(results);

        }

        public bool IsAgentActive() => !agent.IsDone();  

        public float GetFitness()
        {
            
            float fitness = agent.GetFitness();
            genome.Fitness = fitness;
            return fitness; 
            
        }

    }
    
}