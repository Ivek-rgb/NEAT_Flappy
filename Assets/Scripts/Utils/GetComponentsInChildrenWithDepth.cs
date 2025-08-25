using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Utils
{
    public class GetComponentsInChildrenWithDepth
    {

        public static List<T> GetComponentsInChildrenMaxDepthDFS<T>(GameObject parent, int maxDepth, bool includeParent = true) where T : Component
        {
            
            Stack<(GameObject, int)> searchQueue = new Stack<(GameObject, int)>();
            List<T> retList = new List<T>(); 
            searchQueue.Push((parent, 0));
            
            while (searchQueue.Count > 0)
            {
                (GameObject currentSearchableObject, int currDepth) = searchQueue.Pop();
                if (currDepth > maxDepth) continue; 
                
                T wantedComponent = currentSearchableObject.GetComponent<T>(); 
                if(wantedComponent && (includeParent || currDepth != 0))
                    retList.Add(wantedComponent);
                
                foreach (Transform child in currentSearchableObject.transform)
                {
                    searchQueue.Push((child.gameObject, currDepth + 1));
                }

            }

            return retList; 

        }
        
        
        public static List<T> GetComponentsInChildrenMaxDepthBFS<T>(GameObject parent, int maxDepth, bool includeParent = true) where T : Component
        {
            
            Queue<(GameObject, int)> searchQueue = new Queue<(GameObject, int)>();
            List<T> retList = new List<T>(); 
            searchQueue.Enqueue((parent, 0));
            
            
            while (searchQueue.Count > 0)
            {
                (GameObject currentSearchableObject, int currDepth) = searchQueue.Dequeue();
                if (currDepth > maxDepth) continue; 
                
                T wantedComponent = currentSearchableObject.GetComponent<T>(); 
                if(wantedComponent && (includeParent || currDepth != 0))
                    retList.Add(wantedComponent);
                
                foreach (Transform child in currentSearchableObject.transform)
                {
                    searchQueue.Enqueue((child.gameObject, currDepth + 1));
                }

            }

            return retList; 

        }


    }
}
