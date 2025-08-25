using System;
using GD.MinMaxSlider;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Environment.Background
{
    public class RandomRotationOfObj : MonoBehaviour
    {
        
        [MinMaxSlider(-180f, 180f)]
        public Vector2 range = new(-180f, 180f);
        
        private void Awake()
        {
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(range.x, range.y)); 
        }
        
    }
    
}