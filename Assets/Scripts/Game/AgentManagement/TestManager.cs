using System.IO;
using AI.NEAT.Core;
using AI.NEAT.Core.ModelParts;
using AI.NEAT.Core.PopulationManagement;
using AI.NEAT.Core.Serialization;
using Game.NEAT;
using Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Game.TrainingManagement
{
    public class TestManager : MonoBehaviour
    {
        public static TestManager Instance { get; private set; }

        [Header("NEAT settings")] 
        private PopulationManager _agentPopulation;
        public int activeAgents;

        public GameObject agentPrefab; 
        
        private GameObject[] _agents;

        private NeatAgentWrapper[] _agentWrappers;
        
        public float agentRefreshRate = 0.1f;

        private float _internalTimer = 0f; 
      
        private void HandleAgentDeactivation(NeatAgentWrapper playerController)
        {
            activeAgents--; 
        }

        private void Awake()
        {

            if (Instance != null && Instance != this)
            {
                
                Destroy(gameObject);
                return; 

            }
            
            Instance = this; 
            DontDestroyOnLoad(gameObject);
           
            // TODO: when you finish with all about training just make this more approachable inside the 
            _agentPopulation = NeatSerializer.LoadPopulation(Path.Combine(Application.dataPath,
                "./Scripts/AI/NEAT/Models/first_save_test_52.json"));

            _agents = new GameObject[1];
            _agentWrappers = new NeatAgentWrapper[1];
            
            Genome bestAgent = _agentPopulation.GetFirstElites(1)[0]; 
            
            for(int i = 0; i < _agents.Length; i++)
            {
                
                _agents[i] = Instantiate(agentPrefab, transform);
                _agents[i].transform.position = Vector3.zero; // default player position start 
                _agentWrappers[i] = _agents[i].GetComponent<NeatAgentWrapper>(); 
                _agentWrappers[i].ChangeGenome(_agentPopulation.GetFirstElites(1)[0]);
                _agentWrappers[i].OnDeactivated += HandleAgentDeactivation; 

            }


        }

        private void OnDestroy()
        { }

        private void ResetAndRefillAgents()
        {
         
            activeAgents = _agentPopulation.PopulationSize;
            
            for(int i = 0; i < _agentPopulation.PopulationSize; i++)
            {
                
                _agentWrappers[i].ResetAgent();
                _agents[i].transform.position = Vector3.zero; 
                _agentWrappers[i].ChangeGenome(_agentPopulation.Population[i]);

            }
            
        }
        
        private void AgentMakeDecision()
        {
            foreach (var agent in _agentWrappers)
            {
                agent.ObserveAndReact();
            }
        }
        
        private void FixedUpdate()
        {
            _internalTimer += Time.fixedDeltaTime;

            if (_internalTimer > agentRefreshRate)
            {
                _internalTimer = 0; 
                AgentMakeDecision();
            }

        }
    }
}