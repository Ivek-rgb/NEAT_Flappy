using System;
using UnityEngine;

namespace Game.Utils
{
    public class DisableCollidersOutsideCameraRange : MonoBehaviour
    {

        public Camera mainCamera;
        private Collider2D[] _objectColliders;
        public float refreshRate = 0.1f;
        private float _refreshInternalTimer = 0f;
        public Vector2 minOffset = new(0, 0);
        public Vector2 maxOffset = new(0, 0); 
        
        private void Awake()
        {
            
            _objectColliders = GetComponentsInChildren<Collider2D>();
            DisableColliders();

            if(mainCamera == null)
                mainCamera = FindAnyObjectByType<Camera>();

        }

        private void OnDisable()
        {
            DisableColliders();
        }


        private void DisableColliders()
        {
            for (int i = 0; i < _objectColliders.Length; i++)
                _objectColliders[i].enabled = false;
        }

        private void Update()
        {
            
            _refreshInternalTimer += Time.deltaTime;

            if (!(_refreshInternalTimer >= refreshRate)) return;
            
            CheckInBounds();
            _refreshInternalTimer = 0f;

        }

        private void CheckInBounds()
        {

            Bounds colliderBounds = new Bounds(transform.position, Vector3.zero); 
            for (int i = 0; i < _objectColliders.Length; i++) {
                colliderBounds.Encapsulate(_objectColliders[i].bounds);
            }
            
            Vector2 minViewPositionCamera = mainCamera.WorldToViewportPoint(colliderBounds.min);
            Vector2 maxViewPositionCamera = mainCamera.WorldToViewportPoint(colliderBounds.max);

            bool isVisible = minViewPositionCamera.x <= 1 + minOffset.x && maxViewPositionCamera.x >= 0 - maxOffset.x &&
                             minViewPositionCamera.y <= 1 + minOffset.y  && maxViewPositionCamera.y >= 0 - maxOffset.y;  
            
            if (isVisible)
            {
                for (int i = 0; i < _objectColliders.Length; i++)
                    _objectColliders[i].enabled = true; 
            }else DisableColliders();

        }
       
    }
}