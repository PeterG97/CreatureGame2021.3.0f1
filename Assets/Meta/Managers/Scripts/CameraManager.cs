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
    [NonSerialized] protected CinemachineBrain cinemachineBrain;
    [NonSerialized] private GameObject player;
    #endregion

    #region ---=== Inspector Variables ===---
    [SerializeField] private float zoomStepAmount = 0.5f;
    [SerializeField] private float zoomSpeed = 0.05f;
    [SerializeField] private float zoomFinishThreshold = 0.005f;
    #endregion

    #region ---=== Data Variables ===---
    [NonSerialized] public float mapWidth;
    [NonSerialized] public float mapHeight;
    [NonSerialized] public float screenAspectRatio;
    [NonSerialized] private float minOrthographicSize = 2f;
    [NonSerialized] private float maxOrthographicSize = 18f;
    [NonSerialized] private float maxMapOrthographicSize;
    [NonSerialized] private float targetZoom;
    [NonSerialized] private bool noTarget; //If the camera know it has no target (incase an object is destroyed)
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
        cinemachineBrain = GetComponent<CinemachineBrain>();
        CamSetTarget(FindObjectOfType<Player>().gameObject);
        boundsConfiner = virtualCam.GetComponent<CinemachineConfiner2D>();
        boundsConfiner.m_BoundingShape2D = boundsCollider;

        //These two settings allow sorting base on mainly Y but also Z has an equal effect but
        //Z is only manually changed so this can be used for jumps or more complicated depth
        cam.transparencySortMode = TransparencySortMode.CustomAxis;
        cam.transparencySortAxis = new Vector3(0, 1, 1);
    }

    private void Update()
    {
        CamZoomStep();

        //If the target is gone but the noTarget does not reflect that (Object was probably destroyed)
        if (!noTarget && virtualCam.Follow == null)
            CamSetNoTarget();
    }

    private void OnDestroy()
    {
        if (boundsTform != null)
            Destroy(boundsTform.gameObject);
    }

    public void CamUpdateSize(float _width, float _height)
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
            float _newMaxSize = mapWidth / 2 / ((float)Math.Round(screenAspectRatio * 100f) / 100f);
            if (_newMaxSize >= minOrthographicSize)
            {
                maxMapOrthographicSize = _newMaxSize;
            }
        }
        else //Horizontal is confining the size
        {
            float _newMaxSize = mapHeight / 2;
            if (_newMaxSize >= minOrthographicSize)
            {
                maxMapOrthographicSize = _newMaxSize;
            }
        }

        //Zooms out to max size when grid size updates
        CamSetZoomInstantMax();
    }


    #region ---=== Public Camera Functions ===---
    public void CamSetTargetZoom(float _size)
    {
        _size = CamClampOrthSize(_size);

        targetZoom = _size;
    }

    public void CamSetZoomInstantMax()
    {
        CamZoomInstant(999999f); //Set current zoom
        CamSetTargetZoom(999999f); //Set target zoom
    }

    public void CamSetZoomInstantMin()
    {
        CamZoomInstant(0f); //Set current zoom
        CamSetTargetZoom(0f); //Set target zoom
    }

    public void CamSetZoomStepMax()
    {
        CamSetTargetZoom(999999f);
    }

    public void CamSetZoomStepMin()
    {
        CamSetTargetZoom(0f);
    }

    public void CamSetTarget(GameObject _obj)
    {
        noTarget = false;
        virtualCam.Follow = _obj.transform;
    }


    public void CamSetNoTarget()
    {
        noTarget = true;
        virtualCam.Follow = null;
        virtualCam.ForceCameraPosition(new Vector3(mapWidth / 2, mapHeight / 2, virtualCam.transform.position.z),
                                       virtualCam.transform.rotation);
    }

    public void CamEnableUpdate()
    {
        cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
    }

    public void CamDisableUpdate()
    {
        cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;
        cinemachineBrain.ManualUpdate(); //Runs it once in case it hasn't be run yet
    }

    public void CamScrollWheelZoom(Vector2 _scrollData)
    {
        CamSetTargetZoom(CamClampOrthSize(targetZoom - _scrollData[1] / 120f * zoomStepAmount));
    }
    #endregion


    #region ---=== Private Camera Functions ===---
    private void CamZoomStep()
    {
        if (virtualCam.m_Lens.OrthographicSize == targetZoom)
            return;

        //Lerp to desired size
        float zoom = Mathf.Lerp(virtualCam.m_Lens.OrthographicSize, targetZoom, zoomSpeed);

        //If under this threshold set to target size to prevent tiny decimal issues
        if (Mathf.Abs(zoom - targetZoom) < zoomFinishThreshold)
            zoom = targetZoom;

        CamZoomInstant(zoom);
    }

    private void CamZoomInstant(float _size)
    {
        virtualCam.m_Lens.OrthographicSize = CamClampOrthSize(_size);

        //In Cinemachine if the orthographic size is equal to the bounding box then it will stop being restrained
        //So when that would be true disable follow and set to center of map
        if (virtualCam.m_Lens.OrthographicSize >= maxMapOrthographicSize && virtualCam.m_Lens.OrthographicSize > minOrthographicSize)
        {
            CamDisableUpdate();
            transform.position = new Vector3(mapWidth / 2, mapHeight / 2, virtualCam.transform.position.z);
        }
        else
        {
            CamEnableUpdate();
        }
    }

    private float CamClampOrthSize(float _size)
    {
        //Clamp for the game's min and max possible size
        _size = Mathf.Clamp(_size, minOrthographicSize, maxOrthographicSize);
        //Clamps for map's current min and max possible size
        _size = Mathf.Clamp(_size, 0, maxMapOrthographicSize);

        return _size;
    }
    #endregion ---=== Camera Functions ===---
}
