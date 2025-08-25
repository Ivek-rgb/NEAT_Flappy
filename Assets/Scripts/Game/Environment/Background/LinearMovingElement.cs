using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Environment.Background
{
    public class LinearMovingElement : MonoBehaviour
    {
        
        public Transform startPoint;
        public Transform endPoint;
        public GameObject movingObject; 
        
        private Transform _currFinish; 
        
        public float movingSpeed;

        public bool enableRandomSpeed;

        public float minSpeed;
        public float maxSpeed;

        [NonSerialized]
        public Vector3 recordedPositionDiff; 
        
        public void Awake()
        {
            
            movingObject.transform.position = startPoint.position; 
            _currFinish = endPoint;

            if (enableRandomSpeed)
                movingSpeed = Random.Range(minSpeed, maxSpeed); 

        }

        public void FixedUpdate()
        {

            Vector2 sides = _currFinish.position - movingObject.transform.position;
            float distance = Mathf.Sqrt(sides.x * sides.x + sides.y * sides.y);

            if (distance <= 0.1f)
            {
                movingObject.transform.position = _currFinish.transform.position;
                _currFinish = _currFinish == endPoint ? startPoint : endPoint;
                recordedPositionDiff = Vector3.zero; 
            }

            recordedPositionDiff = movingObject.transform.position; 
            movingObject.transform.position = Vector2.MoveTowards(movingObject.transform.position, _currFinish.position, movingSpeed * Time.fixedDeltaTime);

            recordedPositionDiff -= movingObject.transform.position;
            recordedPositionDiff /= Time.fixedDeltaTime; 

        }


    }
}