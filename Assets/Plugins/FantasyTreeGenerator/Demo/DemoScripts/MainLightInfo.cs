using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLightInfo : MonoBehaviour
{
    private Light light;
    private static readonly int mainLightColor = Shader.PropertyToID("_MainLightColor");
    private static readonly int mainLightDirection = Shader.PropertyToID("_MainLightDirection");
    private void Awake()
    {
        light = GetComponent<Light>();
    }
    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalColor(mainLightColor,light.color);
        Shader.SetGlobalVector(mainLightDirection,light.transform.forward);
    }
}
