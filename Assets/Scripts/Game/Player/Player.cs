using UnityEngine;

namespace Game.Player
{
    public class Player : MonoBehaviour
    {
        public Rigidbody2D playerRb;
        public Animator playerAnimator; 
        public float tiltSmooth = 5f;
        public float maxUpRotation = 30f;
        public float maxDownRotation = -60f;
        public float jumpForceAdd = 10f;
    
        void Awake()
        {
            if (playerRb == null)
                playerRb = GetComponent<Rigidbody2D>();
        
            if(playerRb == null)
                Debug.LogError("LOG ERROR: could not find rigidbody for player instance");

            if (playerAnimator == null)
                playerAnimator = GetComponentInChildren<Animator>();
        
            if(playerAnimator == null)
                Debug.LogError("LOG ERROR: no animator could be found for wing instance");
        
        }

        void Start() { }

        void FixedUpdate()
        {
            float tilt = Mathf.Lerp(maxDownRotation, maxUpRotation, (playerRb.linearVelocityY + 5f) / 7f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, tilt), tiltSmooth * Time.fixedDeltaTime);
            playerRb.linearVelocityY = Mathf.Clamp(playerRb.linearVelocityY, -7f, 7f); 
        }


    }
}
