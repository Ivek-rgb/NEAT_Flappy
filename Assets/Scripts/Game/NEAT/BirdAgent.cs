using System;
using AI.NEAT.Interfaces;
using Game.Player;
using UnityEngine;


namespace Game.NEAT
{
    public class BirdAgent : MonoBehaviour, INeatAgent
    {

        public Rigidbody2D birdRigidbody;
        public NearestPipeSensor birdSensor;
        public PlayerController birdController; 
        
        public event Action OnDeactivated;
    
        private float _timeAlive; // can be represented either by time ingame or score or something else 
        private bool _killed;

        public int pipeScoreMultiplier = 100; 

        private void Update()
        {
            if(!_killed)
                _timeAlive += Time.deltaTime; 
        }

        private void Awake()
        {
            
            birdRigidbody = GetComponent<Rigidbody2D>();
            birdSensor = GetComponent<NearestPipeSensor>();
            birdController = GetComponent<PlayerController>();

            birdController.OnDeath += OnBirdDestroyedCallback; 

        }

        private void OnBirdDestroyedCallback(PlayerController source)
        {
            OnDeactivated?.Invoke();
            _killed = true;
        }

        public void ResetAgent()
        {
            transform.position = Vector3.zero; 
            gameObject.SetActive(true);
            _killed = false;
            _timeAlive = 0;
            birdController.Score = 0; 
        }

        public void SetMainPipeSensorOnAgent()
        {
            
            
            
            
        }
        
        public float[] GetObservations()
        {
            var sensorData = birdSensor.DetectNextObstacle();
            float[] inputs = new float[sensorData.Length + 2]; 
            
            for (int i = 0; i < sensorData.Length; i++)
            {
                inputs[i] = sensorData[i]; 
            }

            inputs[sensorData.Length] = birdController.transform.position.y;
            inputs[sensorData.Length + 1] = birdRigidbody.linearVelocityY; 
            
            return inputs; 
        }

        public void ApplyAction(float[] outputs)
        {
            if (outputs.Length <= 0) return; 
            float mainOutput = outputs[0]; 
            if(mainOutput > 0.5f) birdController.Jump();
        }

        public bool IsDone() => _killed; 

        public float GetFitness()
        {
            // centering bonus for now is 10 -- omitted because it just enables staleness to kinda  
            return _timeAlive + pipeScoreMultiplier * birdController.Score; 
        }
        
    }
}