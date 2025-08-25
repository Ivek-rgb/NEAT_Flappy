using Unity.Mathematics;
using UnityEngine;

namespace Game.Environment
{
    public class RotateObjectIndefinetly : MonoBehaviour
    {

        [Range(0f, 50f)]
        public float turningRate = 5f;

        [Range(0f, 45f)]
        public float rotateByDegs = 5f; 
    
        // Update is called once per frame
        void Update()
        {

            Vector3 nextRotation = transform.rotation.eulerAngles;
            nextRotation.z += rotateByDegs; 
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(nextRotation), turningRate * Time.deltaTime);
        }
    }
}
