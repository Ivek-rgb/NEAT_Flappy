using Game.GameManagement;
using UnityEngine;

namespace Game.Environment
{
    public class GrassRendering : MonoBehaviour, IShiftable
    {
        public AreaObject designatedSpawnArea;

        public ObjectPooler grassPooler; // we grab it from here 
        private GameObject[] _grassPool; // instantiated pool 
    
        public int minGrassAmount = 1; 
        public int maxGrassAmount = 5;

        private int _lastSiblingIndex = -1;  
        
        // primitive way of doing this 
        private void ResetGrassPatches()
        {
            foreach (GameObject grassObj in _grassPool)
            {
                grassPooler.ReturnToPool(grassObj.gameObject);
            }
            _grassPool = null; 
            InitializeGrass();
            
        }

        private void InitializeGrass()
        {

            int numberOfSpawn = Random.Range(minGrassAmount, maxGrassAmount + 1);
            _grassPool = new GameObject[numberOfSpawn];
        
            for (int i = 0; i < numberOfSpawn; i++)
            {
            
                GameObject gottenGrass = grassPooler.GetFromPool();
                gottenGrass.transform.SetParent(transform);
                
                gottenGrass.transform.position = designatedSpawnArea.GetRandomPosition();
                float spriteHalfHeight = gottenGrass.GetComponent<SpriteRenderer>().bounds.size.y / 2;
                gottenGrass.transform.position = new Vector3(gottenGrass.transform.position.x, gottenGrass.transform.position.y + spriteHalfHeight, gottenGrass.transform.position.z);

                _grassPool[i] = gottenGrass; 

            }

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InitializeGrass();
            _lastSiblingIndex = transform.GetSiblingIndex(); 

        }

        public void OnShiftCallback()
        {
            ResetGrassPatches();
        }
        
    }
}
