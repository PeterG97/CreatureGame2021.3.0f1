using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem.UI;

public class CameraManager : MonoSingleton<CameraManager>
{
    /* Dependent on no Classes: */

    #region ---=== Auto Assigned/Constant Variables ===---
    [NonSerialized] public Camera cam;
    [NonSerialized] private GameObject player;
    #endregion

    #region ---=== Inspector Variables ===---
    [SerializeField] private float zoomSpeed = 0.5f;
    #endregion

    #region ---=== Data Variables ===---
    [NonSerialized] public float mapWidth;
    [NonSerialized] public float mapHeight;
    [NonSerialized] public float screenAspectRatio;
    [NonSerialized] private float maxOrthographicSize = 5f;
    [NonSerialized] private float minOrthographicSize = 3f;
    #endregion

    public void Start()
    {
        cam = GetComponent<Camera>();

        //These two settings allow sorting base on mainly Y but also Z has an equal effect but
        //Z is only manually changed so this can be used for jumps or more complicated depth
        cam.transparencySortMode = TransparencySortMode.CustomAxis;
        cam.transparencySortAxis = new Vector3(0, 1, 1);

        player = FindObjectOfType<Player>().gameObject;
    }

    public void Update()
    {
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (player != null)
        {
            transform.position = new Vector3(player.transform.position.x,
                                             player.transform.position.y,
                                             transform.position.z);
        }

        float zoom = maxOrthographicSize - cam.orthographicSize;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, mapWidth / 2 - zoom * screenAspectRatio, mapWidth / 2 + zoom * screenAspectRatio),
                                 Mathf.Clamp(transform.position.y, mapHeight / 2 - zoom, mapHeight / 2 + zoom),
                                 transform.position.z);
    }

    public void UpdateCameraSize(float _width, float _height)
    {
        mapWidth = _width;
        mapHeight = _height;

        screenAspectRatio = (float)Screen.width / (float)Screen.height;
        float gridAspectRatio = _width / _height;

        if (gridAspectRatio < screenAspectRatio)
        {
            float _newMaxSize = _height / 2;
            if (_newMaxSize >= minOrthographicSize)
            {
                maxOrthographicSize = _newMaxSize;

                //Zooms out to max size when grid size updates
                cam.orthographicSize = maxOrthographicSize;

                transform.position = new Vector3(screenAspectRatio * maxOrthographicSize, _height / 2, transform.position.z);
            }
        }
        else
        {
            float _newMaxSize = (gridAspectRatio) / (screenAspectRatio) * _height / 2;
            if (_newMaxSize >= minOrthographicSize)
            {
                maxOrthographicSize = _newMaxSize;

                //Zooms out to max size when grid size updates
                cam.orthographicSize = maxOrthographicSize;

                transform.position = new Vector3(_width / 2, maxOrthographicSize, transform.position.z);
            }
        }
    }

    public void CameraScrollWheelZoom(Vector2 _scrollData)
    {
        cam.orthographicSize -= _scrollData[1] / 120f * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthographicSize, maxOrthographicSize);
    }
}
