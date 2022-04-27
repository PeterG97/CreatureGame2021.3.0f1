using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineShader : MonoBehaviour
{
    [NonSerialized] public Material shaderMaterial;
    [NonSerialized] private new Camera camera;

    void Start()
    {
        shaderMaterial = GetComponent<Renderer>().material;
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        shaderMaterial.SetFloat("_CamSize", camera.orthographicSize);
    }
}
