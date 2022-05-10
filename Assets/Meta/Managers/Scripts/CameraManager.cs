/* Handles camera movement and zoom (uses orthographic size). Right now only tracks the player
 * if one exists. The game sorts depth based on the Y and Z axes.
 * 
 * Dependent on no classes */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using Cinemachine;

public class CameraManager : MonoSingleton<CameraManager>
{
    /* Dependent on no Classes: */

    #region ---=== Auto Assigned/Constant Variables ===---
    [NonSerialized] public Camera cam;
    [NonSerialized] private Transform boundsTform;
    [NonSerialized] private CinemachineConfiner2D boundsConfiner;
    [NonSerialized] private CinemachineVirtualCamera virtualCam;
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
    [NonSerialized] private float minOrthographicSize = 2f;
    [NonSerialized] private readonly float boundMaxMult = 0.999f;
    #endregion

    protected override void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        //New box collider that will adjust to map size
        boundsTform = new GameObject("CameraBounds").transform;
        var boundsCollider = boundsTform.gameObject.AddComponent<PolygonCollider2D>();
        //Setting this PolygonCollider2D to a 4 point square 
        boundsCollider.points = new Vector2[] {Vector2.zero, Vector2.up, Vector2.one, Vector2.right};
        boundsCollider.isTrigger = true;

        //Cinemachine camera reference and setup
        virtualCam = FindObjectOfType<CinemachineVirtualCamera>();
        player = FindObjectOfType<Player>().gameObject;
        virtualCam.Follow = player.transform;
        boundsConfiner = virtualCam.GetComponent<CinemachineConfiner2D>();
        boundsConfiner.m_BoundingShape2D = boundsCollider;

        //These two settings allow sorting base on mainly Y but also Z has an equal effect but
        //Z is only manually changed so this can be used for jumps or more complicated depth
        cam.transparencySortMode = TransparencySortMode.CustomAxis;
        cam.transparencySortAxis = new Vector3(0, 1, 1);
    }

    private void Update()
    {

    }

    private void OnDestroy()
    {
        if (boundsTform != null)
            Destroy(boundsTform.gameObject);
    }

    public void UpdateCameraSize(float _width, float _height)
    {
        mapWidth = _width;
        mapHeight = _height;
        screenAspectRatio = (float)Screen.width / (float)Screen.height;

        boundsTform.position = new Vector3(0, 0, 0);
        boundsTform.localScale = new Vector3(mapWidth, mapHeight, 1);
        boundsConfiner.InvalidateCache();

        //Grid aspect ratio is less than the screen aspact ratio
        if ((mapWidth / mapHeight) < screenAspectRatio) //Vertical is confining the size
        {
            print((float)Math.Round(screenAspectRatio * 100f) / 100f);

            float _newMaxSize = mapWidth / 2 / ((float)Math.Round(screenAspectRatio * 100f) / 100f);
            if (_newMaxSize >= minOrthographicSize)
            {
                maxOrthographicSize = _newMaxSize;
            }

            print("NEW Vert Size: " + maxOrthographicSize);
        }
        else //Horizontal is confining the size
        {
            float _newMaxSize = mapHeight / 2;
            if (_newMaxSize >= minOrthographicSize)
            {
                maxOrthographicSize = _newMaxSize;
            }
        }

        //Zooms out to max size when grid size updates
        CameraZoomMax();
    }

    public void CameraScrollWheelZoom(Vector2 _scrollData)
    {
        float orthSize = virtualCam.m_Lens.OrthographicSize;
        orthSize -= _scrollData[1] / 120f * zoomSpeed;
        orthSize = CameraClampOrthSize(orthSize);

        virtualCam.m_Lens.OrthographicSize = orthSize;
    }

    public void CameraZoomMax()
    {
        virtualCam.m_Lens.OrthographicSize = CameraClampOrthSize(999999f);
    }

    public void CameraZoomMin()
    {
        virtualCam.m_Lens.OrthographicSize = CameraClampOrthSize(0f);
    }

    public float CameraClampOrthSize(float _size)
    {
        //Clamps for both the min and max possible size and the actual bounds of the camera bounds
        _size = Mathf.Clamp(_size, minOrthographicSize, maxOrthographicSize);
        //Cinemachine's confiner 2D kinda sucks and if it is exactly equal to the bounding shape it breaks so boundMaxMult reduces the size slightly
        _size = Mathf.Clamp(_size, 0, boundsTform.localScale.y / 2 * boundMaxMult);
        return _size;
    }
}
