using UnityEngine;

namespace FTG
{
    [CreateAssetMenu(fileName = "Billboard Settings", menuName = "FTG/Billboard Settings")]
    public class BillboardSettings : ScriptableObject
    {
        public enum Number { One = 1, Four = 4, Sixteen = 16 };

        public Number numberOfRenders;
        public int foV;
        public float extraPadding;
        public Color backgroundColor;
        public int squareResolution;
        public LayerMask layer;
        // [Tooltip("Disable post-processing, because otherwise, your billboards will be double post-processed (once whenrendering the billboard atlas, another one when the billboards are on the scene)")]
        // public bool disablePostProcess;
        // public int mipmapCount;

        private void Reset()
        {
            extraPadding = 1;
            numberOfRenders = Number.Sixteen;
            foV = 30;
            backgroundColor = new Color(0, 0, 0, 0);
            layer = LayerMask.NameToLayer("Default");
            squareResolution = 256;
            // mipmapCount = 4;
        }
    }
}