using System.Linq;
using Game.Environment;
using Game.GameManagement;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class BackgroundScrolling : MonoBehaviour
{

    public GameObject[] backgroundLayers;
    public Camera mainCamera;
    private Vector2 _screenBounds;
    public float choke; 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _screenBounds =
            mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height,
                mainCamera.transform.position.z));
        foreach (GameObject layer in backgroundLayers)
        {
            LoadChildObject(layer);
        }
    }

    private void LoadChildObject(GameObject obj)
    {
        
        float objectWidth = obj.GetComponent<SpriteRenderer>().bounds.size.x - choke;
        int childrenNeeded = Mathf.CeilToInt((_screenBounds.x * 2) / objectWidth);
        float startPos = -_screenBounds.x;
        GameObject clone = Instantiate(obj);
        
        for (int i = 0; i <= childrenNeeded; i++)
        {
            GameObject c = Instantiate(clone, transform, true);
            c.transform.position = new Vector3(startPos + objectWidth * i, obj.transform.position.y, obj.transform.position.z);
            c.name = obj.name + i; 
        }
        
        Destroy(clone); 
        Destroy(obj);
    }

    private void ShiftChildObject(GameObject obj)
    {
        SpriteRenderer[] children = GetComponentsInChildrenWithDepth.GetComponentsInChildrenMaxDepthBFS<SpriteRenderer>(gameObject, 1, false).ToArray();

        if (children.Length > 1)
        {

            GameObject firstChild = children[0].gameObject; 
            GameObject lastChild = children[^1].gameObject;
            float halfObjectWidth = children[^1].bounds.extents.x - choke;

            if (_screenBounds.x > lastChild.transform.position.x + halfObjectWidth)
            {

                firstChild.GetComponent<IShiftable>()?.OnShiftCallback();
                firstChild.transform.SetAsLastSibling();
                firstChild.transform.position = new Vector3(lastChild.transform.position.x + 2*halfObjectWidth, lastChild.transform.position.y, lastChild.transform.position.z);

            }

        }

    }

    void LateUpdate()
    {
        foreach (GameObject obj in backgroundLayers)
        {
            ShiftChildObject(obj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
