using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utils
{
    public class GameObjectGrabber : MonoBehaviour
    {

        [Serializable]
        public class UIReference
        {
            public string key;
            public GameObject gameObject; 
        }

        private UIReference[] _uiReferenceCached; 

        [SerializeField] private UIReference[] uiReferences; 
        private Dictionary<string, GameObject> _uiObjects = new Dictionary<string, GameObject>(); 

        private void OnValidate()
        {
            CheckObjectNames();
        }

        private void CheckObjectNames()
        {
            foreach (var t in uiReferences)
            {
                var key = t.key;
                if (key != null || key!.Length == 0)
                {
                    t.key = t.gameObject.name; 
                }
            }
        }
        

        private void Awake()
        {
            CheckObjectNames();
            
            foreach (var reference in uiReferences)
                if (!_uiObjects.ContainsKey(reference.key))
                {
                    _uiObjects.Add(reference.key, reference.gameObject);
                }
        }


        public T Get<T>(string key) where T : Component
        {
            if (_uiObjects.TryGetValue(key, out var obj))
            {
                return obj.GetComponent<T>(); 
            }
            return null; 
        }



    }
    
}