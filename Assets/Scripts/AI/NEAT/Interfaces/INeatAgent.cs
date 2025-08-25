using System;
using Unity.VisualScripting;
using UnityEngine;

namespace AI.NEAT.Interfaces
{
    
    public interface INeatAgent
    {

        event Action OnDeactivated;  
        void ResetAgent();
        float[] GetObservations();
        void ApplyAction(float[] outputs);
        bool IsDone();
        float GetFitness(); 
        
    }
    
}