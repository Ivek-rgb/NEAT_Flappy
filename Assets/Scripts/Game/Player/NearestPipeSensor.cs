using System;
using System.Runtime.CompilerServices;
using Game.Environment.Background;
using UnityEngine;

namespace Game.Player
{
    public class NearestPipeSensor : MonoBehaviour
    {
        
        public string obstacleTag = "Obstacle";
        public float scanDistance = 7f;  
        
        public LayerMask sensorMask;

        private static bool _visualSensorCreated = false; 
        
        private static GameObject _visualSensor;
        private static Sprite _collisionSprite;

        public bool canDrawSensor = true; 

        public void SetOnThisInstanceSensor()
        {
            _visualSensor.transform.SetParent(transform);
        }

        public void DrawSensor()
        {
            
        }

        public float[] DetectNextObstacle()
        {

            float[] scannerResults = new float[4]; 
            
            var hit = Physics2D.Raycast(transform.position, Vector2.right, scanDistance, sensorMask);

            if (hit.collider)
            {
                
                Transform parent = hit.collider.transform;

                while (parent.parent && !parent.CompareTag(obstacleTag))
                {
                    parent = parent.parent;
                }

                if (parent && parent.CompareTag(obstacleTag))
                {

                    float pipeYSpeed = 0;
                    LinearMovingElement speedInfo = parent.GetComponent<LinearMovingElement>(); 
                    
                    if (speedInfo)
                        pipeYSpeed = speedInfo.recordedPositionDiff.y; 
                    
                    scannerResults[0] = (parent.transform.position.y - transform.position.y);
                    scannerResults[1] = Vector2.Distance(transform.position, parent.transform.position);
                    scannerResults[2] = pipeYSpeed;
                    scannerResults[3] = parent.transform.rotation.z; 

                }
                
            }

            return scannerResults; 

        }

    }
    
    
}