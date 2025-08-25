using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class ObjectPooler : MonoBehaviour
{
    
    public GameObject[] prefab;
    public int poolSize = 10;

    private Queue<GameObject>[] _pool;

    [Tooltip("Spawn chance per game object (in %). Must total 100")]
    [Range(0, 100)]
    public int[] spawnDistribution;

    protected virtual void AwakeSetup()
    {
        
        _pool = new Queue<GameObject>[prefab.Length];
        for (int i = 0; i < _pool.Length; i++)
        {
            _pool[i] = new Queue<GameObject>(); 
        }
        
    }

    protected virtual void OnChangesValidate()
    {
        
        if (prefab != null)
        {
            if (spawnDistribution == null || spawnDistribution.Length != prefab.Length)
            {
                int[] newDist = new int[prefab.Length];
            
                int defaultValue = prefab.Length > 0 ? Mathf.RoundToInt(100 / prefab.Length) : 0;

                for (int i = 0; i < newDist.Length; i++)
                {
                    newDist[i] = defaultValue;
                }
            
                spawnDistribution = newDist; 
            
            }
        
            int sum = spawnDistribution.Sum();
            if (sum != 100)
                NormalizeDistribution(sum);
        }
     
    }

    void Awake()
    {
        AwakeSetup(); 
    }

    void Start()
    {

        for (int i = 0; i < poolSize; i++)
        {
            int prefabIdx = RandomRollIndex(); 
            
            GameObject obj = InstantiateNewPrefab(prefabIdx); 
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            
            _pool[prefabIdx].Enqueue(obj);
        }
        
    }
    
    private int RandomRollIndex()
    {

        int roll = Random.Range(1, 101);
        int cumulative = 0;

        for (int i = 0; i < spawnDistribution.Length; i++)
        {
            cumulative += spawnDistribution[i];
            if (roll <= cumulative)
                return i; 

        }

        return 0; 
        
    }

    private void OnValidate()
    {
        OnChangesValidate();
    }

    private void NormalizeDistribution(int totalSum)
    {

        if (totalSum == 0) return;

        float multiplier = 100f / totalSum;

        int total = 0; 
        for (int i = 0; i < spawnDistribution.Length; i++)
        {
            spawnDistribution[i] = Mathf.FloorToInt(spawnDistribution[i] * multiplier);
            total += spawnDistribution[i]; 
        }

        if (total < 100)
        {
            spawnDistribution[0] += 100 - total; 
        }

    }


    public GameObject GetFromPool(int prefabIdx = -1)
    {
        GameObject obj; 
        
        
        if (prefabIdx > -1 && _pool[prefabIdx].Count > 0)
        {
            
            obj = _pool[prefabIdx].Dequeue(); 
            obj.SetActive(true);
            return obj; 

        }
        
        if (prefabIdx < 0)
        {
            
            int[] order = Permutations.GenerateRandomPermutation(prefab.Length);
            
            for (int i = 0; i < order.Length; i++)
            {
                
                int idx = order[i];
                if (_pool[idx].Count > 0)
                {
                    
                    obj = _pool[idx].Dequeue(); 
                    obj.SetActive(true);
                    return obj; 
                    
                }

            }
            
        }
        
        obj = InstantiateNewPrefab(prefabIdx); 
        return obj; 
    }


    public GameObject InstantiateNewPrefab(int prefabIdx = -1)
    {
        
        GameObject obj;

        if (prefabIdx < 0)
            prefabIdx = RandomRollIndex(); 
        
        obj = Instantiate(prefab[prefabIdx]); 
            
        PoolableTag poolTag = obj.AddComponent<PoolableTag>();
        poolTag.pooler = this;
        poolTag.prefabIdx = prefabIdx; 

        return obj; 
        
    }

    public void ReturnToPool(GameObject obj)
    {
        
        obj.SetActive(false);
        obj.transform.SetParent(transform);

        PoolableTag objectPoolTag = obj.GetComponent<PoolableTag>(); 
        _pool[objectPoolTag.prefabIdx].Enqueue(obj);
        
    }

}