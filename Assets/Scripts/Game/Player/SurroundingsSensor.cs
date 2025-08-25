using System;
using GD.MinMaxSlider;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class SurroundingsSensor : MonoBehaviour
{
    
    /*
     * TODO:
     * clamp line renderers to static for class later if we are able to just use it on this specific bird 
     * optimize and clean up if the training stage stutters a lot
     * 
     */
    
    [Range(0, 100)]
    public int numberOfRays = 5;
    public float rayDistance = 5f;

    private float _deltaAngle = 0f;
    
    [MinMaxSlider(0f, 360f)]
    public Vector2 range = new(0f, 360f);

    private Vector2[] _savedAngles;  
    
    [Range(0, 360)]
    public float offsetDegree = 0f;
    
    [Header("Defines number of raycast checks in intervals")]
    public float raycastInterval = 0.2f;

    private float _raycastCheckTimer = 0f;
    
    [Header("What layer do you want to detect on?")]
    public LayerMask sensorMask;

    private static GameObject _staticSensorContainer; 
    private static LineRenderer[] _rayLines;
    private static SpriteRenderer[] _circleRenderers; 
    
    public Material rayMaterial;
    public float lineWidth = 0.05f;

    [Header("Line circle head settings")] 
    public float circleScale = 1f;
    public GameObject circlePrefab;

    public bool sensorEnabled = true; 
    
    public void OnValidate()
    {
        _deltaAngle = (range.y - range.x) / numberOfRays; 
    }

    public void Awake()
    {
        
        _deltaAngle = (range.y - range.x) / numberOfRays; 
        
        _savedAngles = new Vector2[numberOfRays];
        for (int i = 0; i < _savedAngles.Length; i++)
        {
            float currAngle = (range.x + i * _deltaAngle + offsetDegree) * Mathf.Deg2Rad;
            _savedAngles[i] = new Vector2(Mathf.Cos(currAngle), Mathf.Sin(currAngle)); 
            
        }
        
        _rayLines = new LineRenderer[numberOfRays];
        _circleRenderers = new SpriteRenderer[numberOfRays];

        if (_staticSensorContainer != null) return;  
        
        _staticSensorContainer = new GameObject($"LineContainer");
        
        for (int i = 0; i < numberOfRays; i++)
        {
            
            GameObject lineObj = new GameObject($"RayLine_{i}"); 
            lineObj.transform.SetParent(_staticSensorContainer.transform);

            GameObject circleObj = Instantiate(circlePrefab, lineObj.transform, false);

            circleObj.transform.localScale = new Vector2(circleScale, circleScale);
            
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
            
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = rayMaterial;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 2;

            lineRenderer.sortingLayerName = "Player";
            lineRenderer.sortingOrder = 0;
  
            _rayLines[i] = lineRenderer;
            _circleRenderers[i] = circleObj.GetComponent<SpriteRenderer>();
            
            // TODO: don't forget to change this things back to normal 
            lineRenderer.enabled = false;
            _circleRenderers[i].color = new Color(0, 0, 0, 0); 
        }
        
    }

    public void FixedUpdate()
    {

        if (sensorEnabled)
        {
            _raycastCheckTimer += Time.fixedDeltaTime;

            if (_raycastCheckTimer >= raycastInterval)
            {
                _raycastCheckTimer = 0;
                SensorFire(false); 
            }
        }

    }

    private static void SetLineColor(LineRenderer lineRenderer, Color c)
    {
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }

    public (float, string)[] SensorFire(bool drawSensorLines = true)
    {

        var sensorInfo = new (float, string)[_savedAngles.Length]; 
        
        for(int i = 0; i < _savedAngles.Length; i++)
        {
                
            RaycastHit2D hit = Physics2D.Raycast(transform.position, _savedAngles[i], rayDistance, sensorMask);

            if (hit.collider)
            {
                sensorInfo[i] = (hit.distance, hit.collider.tag);
            }

            if (!drawSensorLines) continue;
            
            Vector2 end = hit.collider ? hit.point : new Vector2(_savedAngles[i].x * rayDistance + transform.position.x, _savedAngles[i].y * rayDistance + transform.position.y);
            _rayLines[i].SetPosition(0, transform.position);
            _rayLines[i].SetPosition(1, end);

            _circleRenderers[i].transform.position = _rayLines[i].GetPosition(1); 

            float proximity = hit.collider ? hit.distance / rayDistance : 1f;
            
            SetGradientByProximity(_rayLines[i], _circleRenderers[i], proximity);
        }

        return sensorInfo; 
    }
    
    void SetGradientByProximity(LineRenderer line, SpriteRenderer circle, float proximity)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

        Color color = Color.Lerp(Color.red, Color.white, proximity);

        colorKeys[0].color = color;
        colorKeys[0].time = 0.0f;
        colorKeys[1].color = color;
        colorKeys[1].time = 1.0f;

        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;

        gradient.SetKeys(colorKeys, alphaKeys);
        line.colorGradient =  gradient;

        circle.color = color; 
    }
    
    private void OnDrawGizmosSelected()
    {

        if (Application.isPlaying)
            return; 

        Gizmos.color = Color.white; 
        for (int i = 0; i < numberOfRays; i++)
        {
            float currAngle = (range.x + i * _deltaAngle + offsetDegree) * Mathf.Deg2Rad;
            Gizmos.DrawLine(transform.position, new Vector2(Mathf.Cos(currAngle) * rayDistance + transform.position.x, Mathf.Sin(currAngle) * rayDistance + transform.position.y));
        }
        
    }
    
    
}
