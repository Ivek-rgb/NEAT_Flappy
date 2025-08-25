using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Player
{
    
    public class PlayerController : MonoBehaviour
    {
        
        private static readonly int Flap = Animator.StringToHash("Flap");

        private GameObject featherInstance; 
        
        private Rigidbody2D _playerRb;
        public Player playerInstance;

        // add half a second of cooldown on jump, might variate 
        public float jumpCooldown = 0.1f;
        private float _jumpCooldownTimer = 0f;

        // just for decoration when the bird flaps         
        [Range(0, 3)]
        public int numOfFeathersReleased = 2;

        public bool featherGeneration = true;

        public bool playerControllable = true;

        public event Action<PlayerController> OnDeath;

        public int Score { get; set; } = 0;
        public float YDistanceFromSafeCenterOnDeath { get; set; } = 0f;

        public bool animateBird = false; 

        void Start()
        {
            if (playerInstance == null)
                playerInstance = GetComponent<Player>(); 
            
            if(playerInstance == null)
                Debug.LogError("LOG ERROR: player instance not found, controller will not function properly");
        
            _playerRb = playerInstance.playerRb;

            _jumpCooldownTimer = 0.5f;

            featherInstance = Resources.Load<GameObject>("Prefabs/BirdParts/Feather");

        }
        
        void Update()
        {
            _jumpCooldownTimer += Time.deltaTime;
            
            if (playerControllable && (jumpCooldown < _jumpCooldownTimer) && Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

        }

        public void Die()
        {
            
            OnDeath?.Invoke(this);
            gameObject.SetActive(false);

            // later add something cinematic for this stuff -- leave blood on pipes or env and explode or spawn  dented bird 
            if (playerControllable)
            { }

        }

        public void Jump()
        {

            if (animateBird)
            {
                playerInstance.playerAnimator.SetTrigger(Flap);
            }

            if (featherGeneration)
            {
                for (int i = 0; i < numOfFeathersReleased; i++)
                    Instantiate(featherInstance, transform.position, transform.rotation);  
            }
            
            _playerRb.AddForce(new Vector2(0f, playerInstance.jumpForceAdd), ForceMode2D.Impulse);
            _jumpCooldownTimer = 0;
            
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Obstacle")) return;
            YDistanceFromSafeCenterOnDeath =
                0.5f * Mathf.Exp(-0.5f * Mathf.Abs(other.gameObject.transform.position.y - transform.position.y));
            Die();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("AddScore"))
                Score++; 
        }
        
        
    }
    
}
