using System;
using UnityEditor.UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class AreaObject : MonoBehaviour
{
    private BoxCollider2D _defBox;
    public Vector2 size;
    public Vector2 offset; 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _defBox = GetComponent<BoxCollider2D>();
        if (_defBox)
        {
            offset = _defBox.offset;
            size = _defBox.size; 
        }
    }
    
    public Vector2 GetRandomPosition()
    {
        Vector2 center = (Vector2)transform.position + offset;
        Vector2 localSize = this.size;
        float x = Random.Range(center.x - localSize.x / 2f, center.x + localSize.x / 2f);
        float y = Random.Range(center.y - localSize.y / 2f, center.y + localSize.y / 2f);
        return new Vector2(x, y); 
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; 
        if (_defBox)
        {
            offset = _defBox.offset;
            size = _defBox.size; 
        }
        Gizmos.DrawWireCube(transform.position + (Vector3)offset, size);
    }
    
}
