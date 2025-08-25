using UnityEngine;
using Utils;

public class GameManager : MonoBehaviour
{
    public SpawnManager mainGameSpawnManager;

    [SerializeField] public bool useFixedSeed = false; 
    [SerializeField] public int fixedSeed = 12345;
    
    private int _seed; 
    
    void Start()
    {

        _seed = useFixedSeed ?  fixedSeed : Random.Range(0, int.MaxValue);
        Random.InitState(_seed);
        Permutations.SetSeed(_seed); 

    }

}
