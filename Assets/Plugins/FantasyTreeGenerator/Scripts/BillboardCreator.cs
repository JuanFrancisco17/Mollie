#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FTG
{
    public class BillboardCreator
    {
        private Camera tempCamera;
        [SerializeField]
        internal bool renderBillboard;
        [SerializeField]
        internal BillboardSettings settings;
        internal RenderTexture renderTarget;
        private Texture2D[] textures;
        private float radius;
        private Vector3 point;
        [SerializeReference]
        internal Texture2D output;
        private Bounds bounds;
        private Rect[] rects;
        private float largestBound;
        public void Setup(Bounds _bounds, Vector3 offset)
        {
            Debug.Log(_bounds);
            bounds = _bounds;
            bounds.size += new Vector3(settings.extraPadding, settings.extraPadding, settings.extraPadding);
            GameObject aux = new GameObject();
            aux.name = "Temp Billboard Camera";
            point = aux.transform.position = offset + Vector3.up * _bounds.center.y;

            renderTarget = new RenderTexture(settings.squareResolution, settings.squareResolution, 0, RenderTextureFormat.ARGB32);
            renderTarget.Create();

            tempCamera = aux.AddComponent<Camera>();
            // tempCamera.
            tempCamera.aspect = 1;
            tempCamera.backgroundColor = settings.backgroundColor;
            tempCamera.cullingMask = settings.layer;
            tempCamera.fieldOfView = settings.foV;
            tempCamera.targetTexture = renderTarget;
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
            largestBound = Mathf.Max(bounds.extents.y, Mathf.Max(bounds.extents.x * 2, bounds.extents.z * 2));
            Debug.Log(largestBound);
            radius = largestBound / Mathf.Tan(Mathf.Deg2Rad * settings.foV / 2f);
            textures = new Texture2D[(int)settings.numberOfRenders];
        }

        public Texture2D Render()
        {
            for (int i = 0; i < (int)settings.numberOfRenders; i++)
            {
                float angle = 360 / (float)settings.numberOfRenders * i;
                tempCamera.gameObject.transform.position = point + new Vector3(radius * Mathf.Cos(Mathf.Deg2Rad * (angle)), 0, radius * Mathf.Sin(Mathf.Deg2Rad * (angle)));
                // tempCamera.gameObject.transform.LookAt(point, tempCamera.transform.right);
                tempCamera.transform.rotation = Quaternion.Euler(0, -90 - angle, 0);
                tempCamera.Render();
                textures[i] = new Texture2D(settings.squareResolution, settings.squareResolution);
                RenderTexture.active = renderTarget;
                textures[i].ReadPixels(new Rect(0, 0, settings.squareResolution, settings.squareResolution), 0, 0);
                textures[i].alphaIsTransparency = true;
                textures[i].Apply();
            }

            output = new Texture2D(Mathf.CeilToInt(settings.squareResolution * Mathf.Sqrt((float)settings.numberOfRenders)), Mathf.CeilToInt(settings.squareResolution * Mathf.Sqrt((float)settings.numberOfRenders)));

            rects = output.PackTextures(textures, 0, Mathf.CeilToInt(settings.squareResolution * Mathf.Sqrt((float)settings.numberOfRenders)) + 1);


            output.alphaIsTransparency = true;
            GameObject.DestroyImmediate(tempCamera.gameObject);
            return output;
        }
        public BillboardAsset CreateAsset(string name)
        {
            BillboardAsset asset = new BillboardAsset();
            asset.name = name + "_BillboardRenderer";
            asset.height = largestBound * 2;
            asset.bottom = -largestBound + bounds.center.y;
            asset.width = largestBound * 2;

            List<Vector4> UVs = new List<Vector4>();
            float side = Mathf.Sqrt((float)settings.numberOfRenders);

            for (int i = 0; i < rects.Length; i++)
            {

                UVs.Add(new Vector4(rects[i].x, rects[i].y, rects[i].width, rects[i].height));


            }
            asset.SetImageTexCoords(UVs);
            ushort[] indices = new ushort[6];
            Vector2[] vertices = new Vector2[4];

            indices[0] = 3;
            indices[1] = 2;
            indices[2] = 0;
            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 0;

            vertices[0].Set(0.037790697f, 0.020348798f);
            vertices[1].Set(0.037790697f, 0.976744f);
            vertices[2].Set(0.95930207f, 0.020348798f);
            vertices[3].Set(0.95930207f, 0.976744f);
            asset.SetIndices(indices);
            asset.SetVertices(vertices);
            return asset;
        }
    }
}
#endif