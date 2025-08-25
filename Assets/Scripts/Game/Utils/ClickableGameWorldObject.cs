using System;
using Mono.Cecil.Cil;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Utils
{
    public class ClickableGameWorldObject : MonoBehaviour
    {
        
        public event Action OnPlayerClick;
        public LayerMask clickableLayer; 

        void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity);

            if (hit.collider != null)
            {
                if (hit.transform == transform)
                {
                    OnClicked();
                }
            }
        }

        void OnClicked()
        {
            OnPlayerClick?.Invoke();
        }

    }
}