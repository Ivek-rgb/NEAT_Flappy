using System;
using System.Collections;
using System.Collections.Generic;
using Game.Player;
using Unity.VisualScripting;
using UnityEngine;

public class GrassMovementTrigger : MonoBehaviour
{
    [Range(0f, 1f)] public float externalInfluenceStrength = 0.25f; 
    public float easeInTime = 0.15f;
    public float easeOutTime = 0.15f;

    public float velocityThreshold = 5f; 
    
    private bool _easeInCoroutineRunning;
    private bool _easeOutCoroutineRunning;

    private Material _material;

    private float _startingXVelocity;
    private float _lastFrameVelocity;

    private int _externalInfluence = Shader.PropertyToID("_ExternalInfluence"); 
    
    void Start()
    {
        _material = GetComponent<SpriteRenderer>().material;
        _startingXVelocity = _material.GetFloat(_externalInfluence); 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        PlayerController playerController = other.GetComponent<PlayerController>();
        Rigidbody2D playerRigidbody = other.GetComponent<Rigidbody2D>(); 
        
        if (playerController && playerRigidbody)
        {
             int dir = other.transform.position.x < transform.position.x ? -1 : 1; 
            if (!_easeInCoroutineRunning && Mathf.Abs(playerRigidbody.linearVelocityY) > Mathf.Abs(velocityThreshold))
            {
                StartCoroutine(EaseIn( Mathf.Abs(playerRigidbody.linearVelocityY) * dir * externalInfluenceStrength)); 
            }
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController)
        {
            StartCoroutine(EaseOut()); 
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();
        Rigidbody2D playerRigidbody = other.GetComponent<Rigidbody2D>();
        if (playerController && playerRigidbody)
        {
            int dir = other.transform.position.x < transform.position.x ? -1 : 1; 
            
            if (Mathf.Abs(_lastFrameVelocity) > Mathf.Abs(velocityThreshold) &&
                Mathf.Abs(playerRigidbody.linearVelocityY) < Mathf.Abs(velocityThreshold))
            {
                StartCoroutine(EaseOut()); 
            }else if (Mathf.Abs(_lastFrameVelocity) < Mathf.Abs(velocityThreshold) &&
                       Mathf.Abs(playerRigidbody.linearVelocityY) > Mathf.Abs(velocityThreshold))
            {
                StartCoroutine(EaseIn(Mathf.Abs(playerRigidbody.linearVelocityY) * dir * externalInfluenceStrength)); 
            }else if (!_easeInCoroutineRunning && _easeOutCoroutineRunning && 
                      Mathf.Abs(playerRigidbody.linearVelocityY) > Mathf.Abs(velocityThreshold))
            {
                _material.SetFloat(_externalInfluence, Mathf.Abs(playerRigidbody.linearVelocityY) * dir * externalInfluenceStrength);
            }
            _lastFrameVelocity = playerRigidbody.linearVelocityY; 
        }
    }

    private IEnumerator EaseIn(float yVelocity)
    {
        _easeInCoroutineRunning = true;
        float elapsedTime = 0f;
        while (elapsedTime < easeInTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedAmount = Mathf.Lerp(_startingXVelocity, yVelocity, elapsedTime / easeInTime);
            _material.SetFloat(_externalInfluence, lerpedAmount);

            yield return null; 
        }
        _easeInCoroutineRunning = false; 
    }

    private IEnumerator EaseOut()
    { 
        _easeOutCoroutineRunning = true;
        float currentInfluence = _material.GetFloat(_externalInfluence);
        float elapsedTime = 0f; 
        
        while (elapsedTime < easeOutTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedAmount = Mathf.Lerp(currentInfluence, _startingXVelocity, elapsedTime / easeOutTime);
            _material.SetFloat(_externalInfluence, lerpedAmount); 
            
            yield return null; 
        }

        _easeOutCoroutineRunning = false; 
    }
    
}