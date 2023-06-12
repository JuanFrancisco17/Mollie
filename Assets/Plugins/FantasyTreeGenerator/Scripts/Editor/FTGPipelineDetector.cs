#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FTG
{
    [InitializeOnLoad]
    public class FTGPipelineDetector
    {
        static FTGPipelineDetector()
        {
            string key = "FTGPipeline" + Application.productName;
            if (!EditorPrefs.HasKey(key))
            {
                // Debug.Log("FTG: First Load, Checking Pipeline");
                // FTGInfo info = Resources.Load<FTGInfo>("Settings/FTGInfo");
                PipelineType aux = GetPipeline();
                // Debug.Log("Current Shaders: "+ info.asset.ToString() + " Current pipeline: "+ aux.ToString());

                // Debug.Log("FTG: Setting FTG Shaders ");

                switch (aux)
                {
                    case PipelineType.UniversalPipeline:
                        ImportURP();
                        break;
                    default:
                        ImportHDRP();
                        break;
                }

                // Debug.Log("FTG: Current pipeline: " + aux.ToString());
                EditorPrefs.SetInt(key, (int)aux);
            }
            else
            {
                PipelineType storedPipeline = (PipelineType)EditorPrefs.GetInt("FTGPipeline");
                PipelineType aux = GetPipeline();
                if (storedPipeline != aux)
                {
                    // Debug.Log("FTG: Pipeline Change Detected, Reimporting FTG Shaders");
                    switch (aux)
                    {
                        case PipelineType.UniversalPipeline:
                            ImportURP();
                            break;
                        default:
                            ImportHDRP();
                            break;
                    }
                    // Debug.Log("FTG: Current pipeline: " + aux.ToString());
                    EditorPrefs.SetInt(key, (int)aux);
                }
            }
        }

        static PipelineType GetPipeline()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                // SRP
                var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (srpType.Contains("HDRenderPipelineAsset"))
                {
                    return PipelineType.HDPipeline;
                }
                else if (srpType.Contains("UniversalRenderPipelineAsset") ||
                         srpType.Contains("LightweightRenderPipelineAsset"))
                {
                    return PipelineType.UniversalPipeline;
                }
                else return PipelineType.Unsupported;
            }
#elif UNITY_2017_1_OR_NEWER
        if (GraphicsSettings.renderPipelineAsset != null) {
            // SRP not supported before 2019
            return PipelineType.Unsupported;
        }
#endif
            // no SRP
            return PipelineType.BuiltInPipeline;
        }


        static void ImportURP()
        {
            string[] paths = AssetDatabase.FindAssets("FTG URP Shaders");
            for (int index = 0; index < paths.Length; index++)
            {
                paths[index] = AssetDatabase.GUIDToAssetPath(paths[index]);
                // Debug.Log("Loading FTG URP Shaders");
                AssetDatabase.ImportPackage(paths[index], false);
            }
        }

        static void ImportHDRP()
        {
            string[] paths = AssetDatabase.FindAssets("FTG Basic HDRP-BIRP Shaders");
            for (int index = 0; index < paths.Length; index++)
            {
                paths[index] = AssetDatabase.GUIDToAssetPath(paths[index]);
                // Debug.Log("Loading FTG HDRP-BIRP Shaders");
                AssetDatabase.ImportPackage(paths[index], false);
            }
        }
    }
}
#endif