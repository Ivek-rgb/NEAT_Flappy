using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class FeatherScript : MonoBehaviour
{

    private Rigidbody2D _featherRigidbody;
    private BoxCollider2D _featherCollider;
    private SpriteRenderer _featherSpriteRenderer; 

    public float timeToLive = 1f;
    public float delay = 0.5f;
    private float _currentDelayTimer = 0f; 
    private float _currentLiveTime = 0f; 
    
    void Awake() { }
    
    void Start()
    {
        _featherRigidbody = GetComponent<Rigidbody2D>();
        _featherCollider = GetComponent<BoxCollider2D>();
        _featherSpriteRenderer = GetComponent<SpriteRenderer>(); 
        
        // hardcoded
        float featherGravityScale = Random.Range(-0.5f, 0.5f);
        float featherSpeed = Random.Range(-3f, -2f);
        float featherTorque = Random.Range(80f, 100f); 
        
        _featherRigidbody.gravityScale = featherGravityScale;
        _featherRigidbody.linearVelocityX = featherSpeed; 
        _featherRigidbody.AddTorque(featherTorque);
        
    }

    void Update()
    {
        
        if (_currentDelayTimer < delay)
        {
            _currentDelayTimer += Time.deltaTime;
            return; 
        }
        
        _currentLiveTime += Time.deltaTime;
        float opacityChange = 1f - _currentLiveTime / timeToLive;

        Color spriteColor = _featherSpriteRenderer.color;
        spriteColor.a = opacityChange;

        _featherSpriteRenderer.color = spriteColor;

        if (opacityChange <= 0f)
        {
            Destroy(gameObject);
        }

    }

}
