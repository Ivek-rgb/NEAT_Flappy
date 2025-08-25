using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class CollapsibleWindow : MonoBehaviour
    {

        public string name;
        private RectTransform _content;
        public TextMeshProUGUI windowTitle;
        public Button collapseButton;

        private Vector2 _originalSize; 
        public bool isCollapsed = true;

        private RectMask2D _elementCulling; 
        
        private void Awake()
        {

            _content = transform.Find("Content").transform as RectTransform;
            windowTitle.text = name;

            _originalSize = _content.sizeDelta;
            _elementCulling = gameObject.AddComponent<RectMask2D>(); 

            ToggleCollapse();
            collapseButton.onClick.AddListener(ToggleCollapse); 

        }

        public void ToggleCollapse()
        {
            
            if (_content == null) return;

            if (isCollapsed)
            {
                Destroy(_elementCulling);
            }
            else
            {
                _elementCulling = gameObject.AddComponent<RectMask2D>(); 
            }
            
            collapseButton.GetComponentInChildren<TextMeshProUGUI>().text = isCollapsed ?  "\u25b2" : "\u25bc"; 
            
            isCollapsed = !isCollapsed;
        }


    }
}