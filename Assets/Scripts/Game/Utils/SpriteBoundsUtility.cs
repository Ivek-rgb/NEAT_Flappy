using UnityEngine;

namespace Utils
{
    public static class SpriteBoundsUtility
    {
        public static float GetWidth(GameObject root)
        {
            if (!root) return 0;  
            return GetBounds(root).size.x; 
        }
        
        public static float GetHeight(GameObject root)
        {
            if (!root) return 0;  
            return GetBounds(root).size.y; 
        }

        private static Bounds GetBounds(GameObject rootObject)
        {

            SpriteRenderer[] renderers = rootObject.GetComponentsInChildren<SpriteRenderer>();

            if (renderers.Length == 0)
            {
                return new Bounds(rootObject.transform.position, Vector3.zero);
            }

            Bounds combinedBounds = renderers[0].bounds; 

            for(int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            return combinedBounds; 

        }


    }
}