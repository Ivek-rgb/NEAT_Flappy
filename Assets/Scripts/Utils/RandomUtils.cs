using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Utils
{
    
    public static class RandomUtils
    {
        
        public static readonly System.Random Rand = new();  
        
        public static float RandomFloatRange(float min, float max)
        {
            double val = Rand.NextDouble();
            return (float)(min + val * (max - min)); 
        }

        public static T RandomListItemBasedOnWeight <T>(IList<T> list, IList<float> weightedProbabilities)
        {

            float totalWeight = 0f;
            for (int i = 0; i < weightedProbabilities.Count; i++)
                totalWeight += weightedProbabilities[i];

            float randomValue = Random.Range(0f, totalWeight);

            float cumulative = 0f;
            for (int i = 0; i < weightedProbabilities.Count; i++)
            {
                cumulative += weightedProbabilities[i];
                if (randomValue < cumulative)
                    return list[i]; 
            }

            return list[^1];
            
        }
        
    }
    
}