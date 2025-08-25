using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Core;
using AI.NEAT.Core.ModelParts;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class NeatDrawer : MonoBehaviour
    {

        public DynamicGraphViewport dynamicGraphViewport;

        public Sprite nodeGraphic;
        private RectTransform _content;
        private RectTransform _viewPortRectTransform;

        public float nodeSize = 2f; 
        public float nodeSpacing = 3f;
        public float layerSpacing = 10f;
        public float connectionThickness = 20f; 
        
        private List<GameObject> _drawnContent;
        public TextMeshProUGUI nodeInfoText;

        private Image _previousClickedImg;
        private Color _previousClickedColor; 
        
        private static void AnchorTopLeft(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = Vector2.zero;
        }

        private void Start()
        {
            
            dynamicGraphViewport = GetComponent<DynamicGraphViewport>();
            _content = dynamicGraphViewport.content; 
            
            _viewPortRectTransform = dynamicGraphViewport.transform as RectTransform;
            _drawnContent = new List<GameObject>();

            dynamicGraphViewport.resetPadding = nodeSize / 2; 
        }

        public void DrawNetwork(Genome network)
        {

            ClearNetwork();
            nodeInfoText.text = "No item selected"; 

            _previousClickedImg = null; 
            network.EnsureLayerAssignment();
            
            float[] orderedConnectionsWeights = network.EdgeInnoLookup.Count > 0
                ? network.EdgeInnoLookup.Values.OrderByDescending(c => c.Weight).Select(c => c.Weight).ToArray()
                : new[] { -1f, -1f }; 
            
            float maxConnectionWeight = -1f;             
            float minConnectionWeight = -1f;

            if (network.EdgeInnoLookup.Count > 0)
            {
                var weights = network.EdgeInnoLookup.Values.Select(c => c.Weight).ToArray();
                maxConnectionWeight = weights.Max();
                minConnectionWeight = weights.Min(); 
            }

            var layersGrouped = network.NodeIdLookup.Values.GroupBy(n => n.Layer).OrderBy(g => g.Key).ToArray(); 

            Dictionary<int, GameObject> idToNode = new Dictionary<int, GameObject>();

            float layerSpacingValue = nodeSize + layerSpacing;
            float nodeSpacingValue = nodeSize + nodeSpacing;

            int maxLayerSize = layersGrouped.Select(g => g.Count()).Max(); 
            
            int i = 0; 
            foreach (var group in layersGrouped)
            {
                float j = (maxLayerSize - group.Count()) / 2f;
                foreach (var node in group)
                {
                    Vector2 position = new Vector2(
                        layerSpacingValue * i + nodeSize / 2,
                        -(nodeSpacingValue * j + nodeSize / 2)
                    );
                    
                    GameObject nodeRep = new GameObject("Node", typeof(RectTransform), typeof(Image));
                    RectTransform rtOfNode = nodeRep.GetComponent<RectTransform>(); 
                   
                    nodeRep.transform.SetParent(_content, false);
                    
                    Image sr = nodeRep.GetComponent<Image>();
                    sr.sprite = nodeGraphic;
                    
                    /*
                    sr.color = node.Type switch
                    {
                        NodeType.Input => Color.blue,
                        NodeType.Hidden => Color.white,
                        NodeType.Output => Color.yellow,
                        NodeType.Bias => Color.gray
                    };
                    */

                    sr.color = Color.white; 
                    

                    Button btn = nodeRep.AddComponent<Button>(); 
                    
                    btn.onClick.AddListener(() =>
                    {
                        if (_previousClickedImg != null) _previousClickedImg.color = _previousClickedColor;

                        Color c = Color.green;
                        c.a = sr.color.a;
                        _previousClickedColor = sr.color; 
                        
                        sr.color = c;

                        _previousClickedImg = sr; 
                        
                        nodeInfoText.text = $"Element ID: {node.ID}\nElement type: Node ({node.Type})\nNode layer: {node.Layer}\nNode activation: {node.ActivationFunc.Method.Name}";
                    });

                    rtOfNode.sizeDelta = new Vector2(nodeSize, nodeSize); 
                   
                    AnchorTopLeft(nodeRep.transform as RectTransform);
                    rtOfNode.anchoredPosition = position; 
                    
                    idToNode.Add(node.ID, nodeRep);
                    _drawnContent.Add(nodeRep);
                    
                    j++; 
                }
                i++; 
                
            }

            foreach (var connection in network.EdgeInnoLookup.Values)
            {
                
                RectTransform startNode = idToNode[connection.FromNode].transform as RectTransform;
                RectTransform endNode = idToNode[connection.ToNode].transform as RectTransform;

                Vector2 nodeCorrector = new Vector2(nodeSize / 2, -nodeSize / 2 + connectionThickness / 2); 
                
                Vector2 start = startNode.anchoredPosition + nodeCorrector;
                Vector2 end = endNode.anchoredPosition + nodeCorrector; 
                
                GameObject line = new GameObject("Connection", typeof(Image));
                line.transform.SetParent(_content, false);
                
                Image img = line.GetComponent<Image>();

                img.color = connection.Enabled ? connection.Weight >= 0 ? Color.white : Color.red : Color.yellow; 
                
                Color c = img.color;
                c.a = (Mathf.Abs(connection.Weight) - minConnectionWeight + 0.001f) / (maxConnectionWeight - minConnectionWeight + 0.001f); 
                img.color = c;
                
                
                Button btn = line.AddComponent<Button>(); 
                btn.onClick.AddListener(() =>
                {
                    if (_previousClickedImg) _previousClickedImg.color = _previousClickedColor;

                    Color c = Color.green;
                    c.a = img.color.a;
                    _previousClickedColor = img.color; 
                    img.color = c;

                    _previousClickedImg = img; 
                        
                    nodeInfoText.text = $"Element ID: {connection.InnovationNumber}\nElement type: Connection\nEnabled: {connection.Enabled}\nConnection weight: {connection.Weight}\nFrom node: {connection.FromNode}\nTo node: {connection.ToNode}";
                });
                
                RectTransform rt = line.GetComponent<RectTransform>();
                AnchorTopLeft(rt);
                
                Vector2 differenceVector = end - start;
                rt.sizeDelta = new Vector2(differenceVector.magnitude, connectionThickness);
                rt.anchoredPosition = start;
                float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
                rt.rotation = Quaternion.Euler(0, 0, angle);

                _drawnContent.Add(line);
                
            }

            foreach (GameObject go in idToNode.Values)
                go.transform.SetAsLastSibling();
            
            dynamicGraphViewport.ZoomToShowAllContent();
            
        }

        public void ClearNetwork()
        {
            foreach (var drawnObject in _drawnContent)
            {
                Destroy(drawnObject);
            }
            
            _drawnContent.Clear();
        }

    }
}