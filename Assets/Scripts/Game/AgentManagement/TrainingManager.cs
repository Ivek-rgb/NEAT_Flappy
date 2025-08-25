using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AI.NEAT.Core;
using AI.NEAT.Core.PopulationManagement;
using AI.NEAT.Core.Serialization;
using Game.NEAT;
using Game.UI;
using Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Slider = UnityEngine.UI.Slider;
using Button = UnityEngine.UI.Button; 
using Toggle = UnityEngine.UI.Toggle; 

namespace Game.AgentManagement
{
    
    public class TrainingManager : MonoBehaviour
    {
        public static TrainingManager Instance { get; private set; }

        [Header("NEAT settings")] 
        public int fixedGenerations = 100;
        public int agentsPerGeneration = 100;

        [SerializeField]
        private bool fullyConnectOnStart; 
        
        [SerializeField]
        private int numOfInputNodes = 8;
        
        [SerializeField]
        private int numOfOutputNodes = 1;
        
        public List<int> hiddenLayersConfig = new List<int>(); 
        
        private PopulationManager _agentPopulation;
        public int activeAgentsCount;

        public GameObject agentPrefab; 
        
        private GameObject[] _agents;

        private NeatAgentWrapper[] _agentWrappers;

        private SpriteRenderer[][] _agentSpriteRenderers;

        [SerializeField]
        private float agentRefreshRate = 0.1f;

        private float _internalTimer = 0f; 
        
        private bool _isLoadingNextGen = false;

        public int generationCounter = 0;

        [SerializeField]
        private int maxSpeciesStaleness = 5;
        
        [Header("Mutation chances mixer")]        
        [Range(0f, 1f)]
        public float chanceOfWeightChange = 0.8f;
        [Range(0f, 1f)]
        public float chanceOfWeightPerturb = 0.9f;
        [Range(0f, 1f)]
        public float chanceOfAddConnection = 0.05f;
        [Range(0f, 1f)]
        public float chanceOfAddNode = 0.02f; 
        
        [Header("UI settings")]
        [Range(0f, 30f)]
        [SerializeField]
        private int maxSimulationSpeed = 2;
        
        [SerializeField]
        private float sliderStepSize = 1f;

        private CanvasGroup _trainingManagerUI; 
        
        private Button _previousAgentButton;
        private Button _nextAgentButton;
        private Button _saveButton;
        private Button _resetButton;
        private Button _pauseButton;

        private Toggle _activeAgentsOnly;
        
        private Slider _simulationSpeedSlider;

        private TextMeshProUGUI _simulationSpeedText;

        private TextMeshProUGUI _infoText;

        private NeatDrawer _testDrawerReference;

        private bool _isUIVisible = true; 

        [Range(0, 30)]
        public int startSimulationSpeed = 1;

        [Header("Serialization settings")] 
        [SerializeField]
        private string savePathInfo;

        [SerializeField]
        private string fileName;
        
        [SerializeField]
        private int currentSelectedAgentIdx;
        
        private void HandleAgentDeactivation(NeatAgentWrapper agent)
        {
            activeAgentsCount--;
        }

        private void CalculateNextAvailableAgentIdx(int step)
        {
            var selectedList = new List<NeatAgentWrapper>();
            int translatedIdx = currentSelectedAgentIdx; 

            for (int i = 0; i < _agentWrappers.Length; i++)
            {
                var agent = _agentWrappers[i];
                if (!_activeAgentsOnly.isOn || agent.IsAgentActive())
                {
                    if (agent == _agentWrappers[currentSelectedAgentIdx])
                        translatedIdx = selectedList.Count; 
                    selectedList.Add(agent);
                }
            }

            if (selectedList.Count == 0) return; 
            
            int selected = ((translatedIdx + step) % selectedList.Count + selectedList.Count) %
                           selectedList.Count; 
            
            currentSelectedAgentIdx = selectedList[selected].trainerIdx;
            HandleAgentChange();
        }

        private void RandomNextAvailableAgent()
        {
            IList<NeatAgentWrapper> selectedList = _activeAgentsOnly.isOn ? _agentWrappers.Where(a => a.IsAgentActive()).ToArray() : _agentWrappers; 
            currentSelectedAgentIdx = selectedList[RandomUtils.Rand.Next(selectedList.Count)].trainerIdx; 
            HandleAgentChange();
        }

        private void SetNextAgentByIdx(int idx)
        {
            currentSelectedAgentIdx = idx; 
            HandleAgentChange();
        }

        private void HandleAgentChange()
        {
            RedrawBySelected();
            ResetInfoBoard();
            _testDrawerReference.DrawNetwork(_agentWrappers[currentSelectedAgentIdx].genome);
        }

        
        private void ResetInfoBoard()
        {
            
            NeatAgentWrapper agent = _agentWrappers[currentSelectedAgentIdx];
            float speciesFitness = _agentPopulation.SpeciesList[agent.speciesId].AverageFitness();
            float agentFitness = agent.GetFitness();
            string agentStatus = agent.IsAgentActive() ? "active" : "<color=red>inactive</color>";

            string message =
                $"Generation: {generationCounter}\nAgent fitness: {agentFitness} \nSpecies average fitness: {speciesFitness} \nSpecies ID: {agent.speciesId} \nAgent ID: {agent.trainerIdx}\nStatus: {agentStatus}\nAverage fitness: {_agentPopulation.AllSpeciesAverageFitness()}";

            _infoText.text = message; 
            
        }

        private void Awake()
        {

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return; 
            }

            SceneManager.sceneLoaded += OnSceneLoaded;

            Instance = this; 
            DontDestroyOnLoad(gameObject);

            int[] layers = hiddenLayersConfig is { Count: > 0 }
                ? hiddenLayersConfig.ToArray()
                : new int[] { 0 }; 
            
            activeAgentsCount = agentsPerGeneration;
            _agents = new GameObject[agentsPerGeneration];
            _agentWrappers = new NeatAgentWrapper[agentsPerGeneration];
            _agentSpriteRenderers = new SpriteRenderer[agentsPerGeneration][]; 
            
            _agentPopulation = new PopulationManager(agentsPerGeneration, numOfInputNodes, numOfOutputNodes, hiddenLayers: layers, fullyConnected: fullyConnectOnStart, maxStaleness: maxSpeciesStaleness);
            
            _agentPopulation.SetChances(chanceOfWeightChange, chanceOfWeightPerturb, chanceOfAddConnection, chanceOfAddNode);
            
            LayerMask playerClickableLayer = LayerMask.NameToLayer("Player");
            GameObject agentStorage = new GameObject("AgentStorage"); 
            agentStorage.transform.SetParent(transform);
            
            for(int i = 0; i < agentsPerGeneration; i++)
            {
                
                _agents[i] = Instantiate(agentPrefab, agentStorage.transform);

                ClickableGameWorldObject cpo = _agents[i].AddComponent<ClickableGameWorldObject>();
                cpo.clickableLayer = playerClickableLayer;
                
                int currentIdx = i;
                cpo.OnPlayerClick += () =>
                {
                    SetNextAgentByIdx(currentIdx);
                }; 

                _agentWrappers[i] = _agents[i].GetComponent<NeatAgentWrapper>(); 
                _agentWrappers[i].ChangeGenome(_agentPopulation.Population[i]);
                _agentWrappers[i].OnDeactivated += HandleAgentDeactivation;
                _agentWrappers[i].trainerIdx = i; 
                
                _agentSpriteRenderers[i] = _agents[i].GetComponentsInChildren<SpriteRenderer>(); 
                
            }
            
            savePathInfo = Path.Combine(Application.dataPath, savePathInfo);
        }

        private void ResetLogic()
        {
            OnNextGenerationAsync();
        }


        private void NextAgent()
        {
            CalculateNextAvailableAgentIdx(1);
        }

        private void PauseLogic()
        {
            OnSliderValueChanged(0f);
        }

        private void PreviousAgent()
        {
            CalculateNextAvailableAgentIdx(-1);
        }
        
        private void Start()
        {

            _trainingManagerUI = GetComponentInChildren<CanvasGroup>(); 
            
            GameObject settingsPanel = transform.Find("TrainingUI/TrainingSettings").gameObject; 
            GameObjectGrabber simSettGrabber = settingsPanel.GetComponent<GameObjectGrabber>();
            
            GameObject infoPanel = transform.Find("TrainingUI/TrainingInfo").gameObject;
            GameObjectGrabber infoPanelGrabber = infoPanel.GetComponent<GameObjectGrabber>();

            _testDrawerReference = transform.Find("TrainingUI/GraphCollapsibleWindow").GetComponentInChildren<NeatDrawer>(); 
            
            _simulationSpeedSlider = simSettGrabber.Get<Slider>("SimulationSpeedSlider");
            _previousAgentButton = simSettGrabber.Get<Button>("Previous");
            _nextAgentButton = simSettGrabber.Get<Button>("Next");
            _simulationSpeedText = simSettGrabber.Get<TextMeshProUGUI>("SimulationSpeedText");

            _saveButton = simSettGrabber.Get<Button>("SaveButton");
            _resetButton = simSettGrabber.Get<Button>("ResetButton");
            _pauseButton = simSettGrabber.Get<Button>("PauseButton");

            _activeAgentsOnly = simSettGrabber.Get<Toggle>("ActiveAgents"); 
            
            _infoText = infoPanelGrabber.Get<TextMeshProUGUI>("CurrentInfo");
            
            _simulationSpeedSlider.minValue = 0f;
            _simulationSpeedSlider.maxValue = maxSimulationSpeed;

            OnSliderValueChanged(Mathf.Max(1f, Mathf.Min(0f, maxSimulationSpeed)));
            OnSliderValueChanged(startSimulationSpeed);
            
            _simulationSpeedSlider.onValueChanged.AddListener(OnSliderValueChanged);
            
            currentSelectedAgentIdx = RandomUtils.Rand.Next(_agentWrappers.Length);
            
            _nextAgentButton.onClick.AddListener(NextAgent);
            _previousAgentButton.onClick.AddListener(PreviousAgent);
            _pauseButton.onClick.AddListener(PauseLogic); 
            _resetButton.onClick.AddListener(ResetLogic);
            _saveButton.onClick.AddListener(SaveLogic);
            
            RedrawBySelected();
            _testDrawerReference.DrawNetwork(_agentWrappers[currentSelectedAgentIdx].genome);

            StartCoroutine(LateStart()); 
        }

        private IEnumerator LateStart()
        {
            yield return null; 
            
            foreach (var agent in _agentWrappers)
                agent.ResetAgent();
        }

        private void RedrawBySelected()
        {
            for (int i = 0; i < _agentSpriteRenderers.Length; i++) {
                if (i == currentSelectedAgentIdx)
                    for (int j = 0; j < _agentSpriteRenderers[currentSelectedAgentIdx].Length; j++) {
                        SpriteRenderer sr = _agentSpriteRenderers[currentSelectedAgentIdx][j];
                        Color c = sr.color;
                        c.a = 1f;
                        sr.color = c; 
                    }
                else 
                    for (int j = 0; j < _agentSpriteRenderers[i].Length; j++) {
                        SpriteRenderer sr = _agentSpriteRenderers[i][j];
                        Color c = sr.color;
                        c.a = 0.2f;
                        sr.color = c; 
                    }
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void ResetAndRefillAgents()
        {
         
            activeAgentsCount = agentsPerGeneration;
            
            for(int i = 0; i < agentsPerGeneration; i++)
            {
                _agentWrappers[i].ResetAgent();
                _agentWrappers[i].ChangeGenome(_agentPopulation.Population[i]);
            }
            
            HandleAgentChange();
        }
         
        // callback that will anyway be called later
        public void OnSliderValueChanged(float value)
        {
            
            float stepped = Mathf.Round(value / sliderStepSize) * sliderStepSize;
            _simulationSpeedText.text = $"{stepped}x"; 
            _simulationSpeedSlider.SetValueWithoutNotify(stepped);
            Time.timeScale = stepped; 
            
        }

        private void AgentMakeDecision()
        {
            foreach (var agent in _agentWrappers)
            {
                agent.ObserveAndReact();
            }
        }

        private void OnNextGenerationAsync()
        {

            // also add the possiblity to start from checkpoint on same seed? this should also grant good leverage on presentation 
            _isLoadingNextGen = true; 
            
            _agentPopulation.Evolve();

            SceneManager.LoadScene("SampleScene");
            
        }

        private void SaveLogic()
        {
            NeatSerializer.SavePopulation(_agentPopulation, $"{savePathInfo}/{fileName}_gen{generationCounter}_{DateTime.Now:yyyyMMddHHmmssfff}.json");
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(DelayedAgentReset());
        }


        private IEnumerator DelayedAgentReset()
        {
            
            var gameManager = FindAnyObjectByType<GameManager>();
            
            gameManager.useFixedSeed = false;
            gameManager.fixedSeed = RandomUtils.Rand.Next(); 
            
            yield return null; 
            ResetAndRefillAgents();
            generationCounter++; 
            
            _internalTimer = 0; 

            _isLoadingNextGen = false;
            
        }

        private void FixedUpdate()
        {

            if (activeAgentsCount <= 0 && !_isLoadingNextGen)
                OnNextGenerationAsync();

            _internalTimer += Time.fixedDeltaTime;

            if (!_isLoadingNextGen && _internalTimer > agentRefreshRate)
            {
                
                for (int i = 0; i < _agentWrappers.Length; i++)
                    _agentWrappers[i].GetFitness();                     

                _internalTimer = 0; 
                AgentMakeDecision();
                ResetInfoBoard();

                if (_activeAgentsOnly.isOn && !_agentWrappers[currentSelectedAgentIdx].IsAgentActive())
                    RandomNextAvailableAgent();

            }
            

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                
                _isUIVisible = !_isUIVisible; 
                _trainingManagerUI.alpha = _isUIVisible  ? 1f : 0f;

                _trainingManagerUI.interactable = _isUIVisible;
                _trainingManagerUI.blocksRaycasts = _isUIVisible;

            }
        }

    }
}