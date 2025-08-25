using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class DynamicGraphViewport : MonoBehaviour, IScrollHandler, IDragHandler
    {

        public RectTransform content;
        private RectTransform _canvasRect; 
        
        public float zoomSpeed = 0.1f;
        public float minZoom = 0.2f;
        public float maxZoom = 5f;

        private LineRenderer _xRenderer;
        private LineRenderer _yRenderer;

        public int gridSize = 50;
        public int halfRange = 10; 
        
        private float _currentZoom = 1f;

        public bool drawCoordSystem;
        public Color axisColor = Color.white;

        public float resetPadding; 

        public void OnScroll(PointerEventData eventData)
        {
            
            float scroll = eventData.scrollDelta.y;

            
            float newZoom = Mathf.Clamp(_currentZoom + scroll * zoomSpeed, minZoom, maxZoom);
            
            RectTransform rt = transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out var localPoint);

            Vector2 pivot = new Vector2(
                    (localPoint.x - rt.rect.x) / rt.rect.width,
                    (localPoint.y - rt.rect.y) / rt.rect.height
                );
            
           ZoomOnTarget(newZoom, pivot);
        }

        public void ZoomOnTarget(float newZoom, Vector2 pivot)
        {
            content.pivot = pivot; 
            content.localScale = Vector3.one * newZoom;
            _currentZoom = newZoom;
        }
        
        private Vector2 GetRealContentSize()
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (RectTransform child in content)
            {
                Vector3[] corners = new Vector3[4];
                child.GetWorldCorners(corners);

                foreach (var corner in corners)
                {
                    Vector3 local = content.InverseTransformPoint(corner);
                    minX = Mathf.Min(minX, local.x);
                    maxX = Mathf.Max(maxX, local.x);
                    minY = Mathf.Min(minY, local.y);
                    maxY = Mathf.Max(maxY, local.y);
                }
            }

            return new Vector2(maxX - minX, maxY - minY);
        }

        public void ZoomToShowAllContent()
        {
            
            Canvas.ForceUpdateCanvases();
            RectTransform thisTransform = transform as RectTransform;

            Vector2 realContentSize = GetRealContentSize();  
            
            float widthScale = thisTransform.rect.width / (realContentSize.x + resetPadding); 
            float heightScale = thisTransform.rect.height / (realContentSize.y + resetPadding);

            float min = Mathf.Min(widthScale, heightScale);
            content.anchoredPosition = new Vector2(0, 0); 

            ZoomOnTarget(min, new Vector2(0f, 1f));
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            content.anchoredPosition += eventData.delta;
        }
        
        private void Start()
        {
            _canvasRect = GetComponent<RectTransform>(); 
        }

        private void DrawGrid()
        {
            for (int i = -halfRange; i <= halfRange; i++)
            {
                CreateLine(new Vector2(i * gridSize, -halfRange * gridSize),
                    new Vector2(i * gridSize, halfRange * gridSize),
                    axisColor);

                CreateLine(new Vector2(-halfRange * gridSize, i * gridSize),
                    new Vector2(halfRange * gridSize, i * gridSize),
                    axisColor);
            }
        }
        
        void CreateLine(Vector2 start, Vector2 end, Color color)
        {
            
            GameObject lineObj = new GameObject("Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            lineObj.transform.SetParent(content, false);

            var image = lineObj.GetComponent<UnityEngine.UI.Image>();
            image.color = color;

            var rt = lineObj.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

            Vector2 dir = (end - start).normalized;
            float dist = Vector2.Distance(start, end);

            rt.sizeDelta = new Vector2(dist, 1f); // thickness = 1px
            rt.anchoredPosition = (start + end) / 2;
            rt.rotation = Quaternion.FromToRotation(Vector3.right, end - start);
            
        }



    }
}