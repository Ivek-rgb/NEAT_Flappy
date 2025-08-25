using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public enum ObstacleType
{
    SinglePipe,
    MovingPipe,
    PipeWithRotation
}

public class ObstaclePattern
{

    public ObstacleType[] containedObstacles;     
    public float environmentSpacing;

    public static float[] weightedProbabilities = new[] { 0.6f, 0.2f, 0.2f}; 
    
    public ObstaclePattern(ObstacleType[] envRepresentation, float envSpacing)
    {
        containedObstacles = envRepresentation; 
        environmentSpacing = envSpacing; 
    }

    public static ObstaclePattern GeneratePattern(int envCount, float envSpacing)
    {
        
        var envs = new ObstacleType[envCount];
        
        for(int i = 0; i < envs.Length; i++)
            envs[i] = RandomUtils.RandomListItemBasedOnWeight((ObstacleType[])Enum.GetValues(typeof(ObstacleType)), weightedProbabilities); 

        return new ObstaclePattern(envs, envSpacing);
        
    }

}
