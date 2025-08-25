using System;
using System.Collections.Generic;
using Game.GameManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utils;

public class SpawnManager : MonoBehaviour
{

    public AreaObject cloudSpawnArea;
    public AreaObject obstacleSpawnArea; 
    
    public ObjectPooler cloudPool;
    public ObstacleDatabasePooler obstacleDatabasePooler;

    public float obstacleSpawnDistance = 5f;  

    public float cloudSpawnInterval = 5f;
    public int numberOfSpawnedClouds = 2;

    private float _cloudTimer = 0f;

    private GameObject _lastSpawnedObstacle;

    private float _prevWidthHalf = 0f;

    public float obstacleScrollSpeed = 1.1f;

    void Update()
    {

        _cloudTimer += Time.deltaTime;

        if (_cloudTimer >= cloudSpawnInterval)
        {
            SpawnCloud();
            _cloudTimer = 0; 
        }

        if (!_lastSpawnedObstacle || _lastSpawnedObstacle.transform.position.x + _prevWidthHalf - obstacleSpawnArea.transform.position.x <= -obstacleSpawnDistance)
        {
            
            ObstaclePattern spawnPattern = ObstaclePattern.GeneratePattern(3, 8f);
            var patternParts = spawnPattern.containedObstacles; 
            
            for (int i = 0; i < patternParts.Length; i++)
            {

                _lastSpawnedObstacle = SpawnObstacle(patternParts[i], spawnPattern.environmentSpacing); 
                
            }

        }

    }

    private GameObject SpawnObstacle(ObstacleType obstacleType, float obstacleSpacing = 0)
    {
        
        GameObject obstacle = obstacleDatabasePooler.GetFromPool(obstacleType);
        _prevWidthHalf = SpriteBoundsUtility.GetWidth(_lastSpawnedObstacle) / 2;
        Vector2 spawnLocation = obstacleSpawnArea.GetRandomPosition();
        float lastX = _lastSpawnedObstacle ? _lastSpawnedObstacle.transform.position.x : spawnLocation.x; 
        
        spawnLocation.x = lastX + _prevWidthHalf + obstacleSpacing;

        if (obstacleType == ObstacleType.MovingPipe)
            spawnLocation.y = 0; 
        
        obstacle.transform.position = spawnLocation;

        ObjectScrollEffect o = obstacle.GetComponent<ObjectScrollEffect>();
        if(!o)
            o = obstacle.AddComponent<ObjectScrollEffect>();
        
        o.speed = obstacleScrollSpeed;
        return obstacle;
        
    }

    private void SpawnCloud()
    {
        GameObject cloud = cloudPool.GetFromPool();
        Vector2 spawnLocation = cloudSpawnArea.GetRandomPosition();
        cloud.transform.position = spawnLocation; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ObjectPooler pool = other.transform.GetComponent<PoolableTag>()?.pooler;
        if(pool)
            pool?.ReturnToPool(other.gameObject);
    }
    
}
