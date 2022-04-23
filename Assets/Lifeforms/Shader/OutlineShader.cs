using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineShader : MonoBehaviour
{
    [System.NonSerialized] public Material shaderMaterial;
    void Start()
    {
        shaderMaterial = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        shaderMaterial.SetFloat("_CamSize", Camera.main.orthographicSize);
    }
}
