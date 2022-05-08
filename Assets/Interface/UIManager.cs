/* Contains references to relevant UI components, captures mouse events, has methods to update
 * UI information, and methods to update game data based on player submitted UI information
 * 
 * Dependent on classes:
 * PermanentMonoSingleton - GameManager
 * MonoSingleton - LifeformManager (If the UICanvas events are activated)
 * MonoSingleton - MapManager (If the UICanvas events are activated) */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoSingleton<UIManager>
{
    #region ---=== Nonserialized Variables ===---
    //State variables
    [NonSerialized] private bool isOverUI;

    //Main references
    [NonSerialized] public Canvas UICanvas;
    [NonSerialized] private InterfaceInputActions interfaceInput;
    [NonSerialized] private RectTransform optionsMenuTransform;
    [NonSerialized] private RectTransform performanceMenuTransform;
    [NonSerialized] private RectTransform animalInfoMenuTransform;
    [NonSerialized] private new Camera camera;

    //Options Menu UI
    [NonSerialized] public Toggle plantTimerToggle, animalTimerToggle;
    [NonSerialized] public TMP_InputField plantTimerInputText, animalTimerInputText,
                                          xInputText, yInputText,
                                          plantInputText, animalInputText;
    [NonSerialized] private float hideOptionsDistance;

    //Performance UI
    [NonSerialized]
    public TMP_Text hideOptionsText, fpsText, plantPopulationText, animalPopulationText,
                                           hpText, nutritionText, ageText, sizeText, hpRegenText, attackText,
                                           sightText, natureText, dietText, moveStyleText, reproductionTypeText,
                                           reproductionTimeText, actionText;

    //Animal Info UI
    [NonSerialized] private float animalInfoWidth;
    [NonSerialized] private float animalInfoHeight;

    [NonSerialized] private float fpsTime;
    [NonSerialized] private int fpsCount;
    [NonSerialized] private float fpsPollingRate = 1f;
    [NonSerialized] private bool hiddenOptions = false;
    [NonSerialized] private ContactFilter2D mouseFilter;
    [NonSerialized] private List<Collider2D> mouseRay;
    #endregion

    #region ---=== Unity Methods ===---
    protected override void Awake()
    {
        base.Awake();

        interfaceInput = new InterfaceInputActions();

        mouseFilter.useTriggers = false;
        mouseRay = new List<Collider2D>();
    }


    private void Start()
    {
        camera = Camera.main;

        UpdateUIReferences();
        UpdateAllUI();
    }

    private void Update()
    {
        UpdateFps();
    }

    private void FixedUpdate()
    {
        MouseOver();
        CheckPointerOverUI();
    }

    private void OnEnable()
    {
        if (interfaceInput != null)
            interfaceInput.Enable();

        interfaceInput.UI.Click.started += StartedClick;
        interfaceInput.UI.Click.canceled += EndedClick;
        interfaceInput.UI.ScrollWheel.performed += PerformedScrollWheel;
    }

    private void OnDisable()
    {
        if (interfaceInput != null)
            interfaceInput.Disable();

        interfaceInput.UI.Click.started -= StartedClick;
        interfaceInput.UI.Click.canceled -= EndedClick;
        interfaceInput.UI.ScrollWheel.performed -= PerformedScrollWheel;
    }
    #endregion


    #region ---=== Update Methods ===---
    private void UpdateFps()
    {
        if (fpsText != null)
        {
            fpsTime += Time.deltaTime;
            fpsCount++;
            if (fpsTime >= fpsPollingRate) //Once 1 second has passed
            {
                fpsText.text = string.Concat(Mathf.RoundToInt(fpsCount / fpsTime));
                fpsTime -= fpsPollingRate; //Subtract instead of setting to 0, leaving any extra time
                fpsCount = 0;
            }
        }
    }
    #endregion


    #region ---=== Interface Events ===---
    //Unsubscribed Events
    private void MouseOver()
    {
        Vector2 mousePosition = interfaceInput.UI.Point.ReadValue<Vector2>();
        Vector2 position = camera.ScreenToWorldPoint(mousePosition);

        int hits = Physics2D.OverlapPoint(position, mouseFilter, mouseRay);
        if (hits > 0)
        {
            for (int i = 0; i < hits; i++)
            {
                Animal animal = mouseRay[i].gameObject.GetComponent<Animal>();
                if (animal != null)
                {
                    //With just isOverUI it was possible to disable the animalInfo window when over UI but the animalInfo itself is UI
                    //so if near the end of the screen it would flash on and off
                    if (animalInfoMenuTransform == null || (!animalInfoMenuTransform.gameObject.activeSelf && isOverUI)) //If not yet enabled and currently over UI do not enable
                        break;

                    DisplayAnimalInfo(animal, mousePosition);
                    break;
                }
            }
        }
        else
            UndisplayAnimalInfo();

        mouseRay.Clear();
    }

    private void StartedClick(InputAction.CallbackContext context)
    {
        
    }

    private void EndedClick(InputAction.CallbackContext context)
    {
        
    }

    private void PerformedScrollWheel(InputAction.CallbackContext context)
    {
        if (CameraManager.Instance != null)
            CameraManager.Instance.CameraScrollWheelZoom(context.action.ReadValue<Vector2>());
    }

    public void DisplayAnimalInfo(Animal _animal, Vector2 _position)
    {
        if (animalInfoMenuTransform == null)
            return;

        animalInfoMenuTransform.gameObject.SetActive(true);

        //Position
        _position.x = Mathf.Clamp(_position.x, 0 + animalInfoWidth / 2, Screen.width - animalInfoWidth / 2);
        _position.y = Mathf.Clamp(_position.y, 0, Screen.height - animalInfoHeight);
        _position.y += 10;
        animalInfoMenuTransform.position = _position;

        //Base Stats
        hpText.text = string.Concat(Mathf.CeilToInt(_animal.HitPoints), "/", Mathf.CeilToInt(_animal.maxHitPoints));
        nutritionText.text = string.Concat(_animal.Nutrition.ToString("F0"), "/", _animal.maxNutrition.ToString("F0"));
        ageText.text = string.Concat(_animal.Age.ToString("F0"), "/", _animal.deathAge.ToString("F0"));
        sizeText.text = _animal.size.ToString("F2");

        //Abilities
        hpRegenText.text = _animal.hitPointsRegenSpeed.ToString("F3");
        attackText.text = _animal.attackPower.ToString("F0");
        sightText.text = _animal.sight.ToString("F0");

        //Behavior
        natureText.text = _animal.nature.ToString();
        dietText.text = _animal.diet.ToString();

        //Reproduction
        if (_animal.sexualReproduction)
            reproductionTypeText.text = "Sexual";
        else
            reproductionTypeText.text = "Asexual";
        reproductionTimeText.text = _animal.reproductionTimer.ToString("N1");

        //Action
        actionText.text = _animal.Action.ToString();
    }

    public void UndisplayAnimalInfo()
    {
        if (animalInfoMenuTransform != null)
        {
            animalInfoMenuTransform.gameObject.SetActive(false);
        }  
    }

    public bool PointerOverUI()
    {
        return isOverUI;
    }

    private void CheckPointerOverUI()
    {
        Vector2 mousePosition = interfaceInput.UI.Point.ReadValue<Vector2>();
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(mousePosition.x, mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        isOverUI = results.Count > 0;
    }
    #endregion


    #region ---=== Setup ===---
    public void UpdateUIReferences()
    {
        SetUpUI();
        Invoke("DelayedMeasurements", 0.1f);
    }

    private void SetUpUI()
    {
        if (!CheckNull.SingleInstanceNotNull(ref UICanvas, false))
            return;

        if (UICanvas.transform.Find("OptionsMenu") == null)
            return;

        //Options Menu
        optionsMenuTransform = UICanvas.transform.Find("OptionsMenu").GetComponent<RectTransform>();

        Transform AutoSpawn = optionsMenuTransform.Find("AutoSpawn");
        plantTimerToggle = AutoSpawn.Find("PlantToggle").GetComponent<Toggle>();
        plantTimerToggle.onValueChanged.AddListener((Event) => PlantAutoToggleClicked(Event));
        plantTimerInputText = AutoSpawn.Find("PlantInput").GetComponent<TMP_InputField>(); //Assumes only one TextMesh
        plantTimerInputText.onEndEdit.AddListener((Event) => UpdateAutoSpawnData());
        animalTimerToggle = AutoSpawn.Find("AnimalToggle").GetComponent<Toggle>();
        animalTimerToggle.onValueChanged.AddListener((Event) => AnimalAutoToggleClicked(Event));
        animalTimerInputText = AutoSpawn.Find("AnimalInput").GetComponent<TMP_InputField>();
        animalTimerInputText.onEndEdit.AddListener((Event) => UpdateAutoSpawnData());

        //Grid Size Row
        Transform gridSize = optionsMenuTransform.Find("GridSize");
        xInputText = gridSize.Find("XInput").GetComponent<TMP_InputField>(); //Assumes only one TextMesh
        yInputText = gridSize.Find("YInput").GetComponent<TMP_InputField>();
        gridSize.Find("UpdateButton").GetComponent<Button>().onClick.AddListener(()
                                                                      => UpdateGridButtonClicked());
        //Spawn Percent Row
        Transform SpawnPercent = optionsMenuTransform.Find("SpawnPercent");
        plantInputText = SpawnPercent.Find("PlantInput").GetComponent<TMP_InputField>();
        animalInputText = SpawnPercent.Find("AnimalInput").GetComponent<TMP_InputField>();
        //Respawn All Row
        optionsMenuTransform.Find("RespawnAll").Find("RespawnAllButton").GetComponent<Button>()
                                               .onClick.AddListener(() => RespawnButtonClicked());
        //Single Spawn Row
        Transform SingleSpawnTransform = optionsMenuTransform.Find("SingleSpawn");
        SingleSpawnTransform.Find("SpawnPlant").GetComponent<Button>()
                                             .onClick.AddListener(() => SpawnPlantButtonClicked());
        SingleSpawnTransform.Find("SpawnAnimal").GetComponent<Button>()
                                             .onClick.AddListener(() => SpawnAnimalButtonClicked());
        //Hide Menu Row
        Transform hideRow = optionsMenuTransform.Find("HideMenu").GetComponent<RectTransform>();
        Transform hideButton = hideRow.Find("HideMenuButton");
        hideButton.GetComponent<Button>().onClick.AddListener(() => HideMenuButtonClicked());
        hideOptionsText = hideButton.Find("HideMenuText").GetComponent<TMP_Text>();


        //Performance Menu
        performanceMenuTransform = UICanvas.transform.Find("PerformanceMenu").GetComponent<RectTransform>();
        fpsText = performanceMenuTransform.Find("FPS").Find("FPSText").GetComponent<TMP_Text>();
        Transform population = performanceMenuTransform.Find("Population");
        plantPopulationText = population.Find("PlantPoplationText").GetComponent<TMP_Text>();
        animalPopulationText = population.Find("AnimalPoplationText").GetComponent<TMP_Text>();


        //Animal Info
        animalInfoMenuTransform = UICanvas.transform.Find("AnimalInfo").GetComponent<RectTransform>();
        animalInfoWidth = animalInfoMenuTransform.sizeDelta.x;
        animalInfoHeight = animalInfoMenuTransform.sizeDelta.y;

        //Base Stats
        Transform hpNutritionRow = animalInfoMenuTransform.Find("HPNutritionRow");
        hpText = hpNutritionRow.Find("HPText").GetComponent<TMP_Text>();
        nutritionText = hpNutritionRow.Find("NutritionText").GetComponent<TMP_Text>();
        Transform ageSizeRow = animalInfoMenuTransform.Find("AgeSizeRow");
        ageText = ageSizeRow.Find("AgeText").GetComponent<TMP_Text>();
        sizeText = ageSizeRow.Find("SizeText").GetComponent<TMP_Text>();

        //Abilities
        Transform abilitiesRow1 = animalInfoMenuTransform.Find("AbilitiesRow1");
        hpRegenText = abilitiesRow1.Find("HPRegenText").GetComponent<TMP_Text>();
        attackText = abilitiesRow1.Find("AttackText").GetComponent<TMP_Text>();
        Transform abilitiesRow2 = animalInfoMenuTransform.Find("AbilitiesRow2");
        sightText = abilitiesRow2.Find("SightText").GetComponent<TMP_Text>();

        //Behavior
        Transform behaviorRow1 = animalInfoMenuTransform.Find("BehaviorRow1");
        natureText = behaviorRow1.Find("NatureText").GetComponent<TMP_Text>();
        dietText = behaviorRow1.Find("DietText").GetComponent<TMP_Text>();
        Transform behaviorRow2 = animalInfoMenuTransform.Find("BehaviorRow2");
        moveStyleText = behaviorRow2.Find("MoveStyleText").GetComponent<TMP_Text>();

        //Reproduction
        Transform reproductionRow = animalInfoMenuTransform.Find("ReproductionRow");
        reproductionTypeText = reproductionRow.Find("TypeText").GetComponent<TMP_Text>();
        reproductionTimeText = reproductionRow.Find("TimeText").GetComponent<TMP_Text>();

        //Action
        Transform actionRow = animalInfoMenuTransform.Find("ActionRow");
        actionText = actionRow.Find("ActionText").GetComponent<TMP_Text>(); ;
    }

    private void DelayedMeasurements()
    {
        //Has to be delayed because auto sized UI elements are stupid
        if (!CheckNull.SingleInstanceNotNull(ref UICanvas, false) || optionsMenuTransform == null)
            return;

        hideOptionsDistance = optionsMenuTransform.sizeDelta.y
                      - optionsMenuTransform.Find("HideMenu").GetComponent<RectTransform>().sizeDelta.y
                      - optionsMenuTransform.GetComponent<LayoutGroup>().padding.bottom;
    }
    #endregion


    #region ---=== Button/Toggle Events ===---
    private void PlantAutoToggleClicked(bool _toggled)
    {
        if (_toggled)
            LifeformManager.Instance.spawnNewPlantByTimer = true;
        else
            LifeformManager.Instance.spawnNewPlantByTimer = false;

        UpdateAutoSpawnData();
    }

    private void AnimalAutoToggleClicked(bool _toggled)
    {
        if (_toggled)
            LifeformManager.Instance.spawnNewAnimalByTimer = true;
        else
            LifeformManager.Instance.spawnNewAnimalByTimer = false;

        UpdateAutoSpawnData();
    }

    private void UpdateGridButtonClicked()
    {
        xInputText.text = Mathf.Clamp(Convert.ToInt32(xInputText.text), 1, 64).ToString();
        yInputText.text = Mathf.Clamp(Convert.ToInt32(yInputText.text), 1, 36).ToString();
        MapManager.Instance.cellWidth = Convert.ToInt32(xInputText.text);
        MapManager.Instance.cellHeight = Convert.ToInt32(yInputText.text);
        MapManager.Instance.UpdateGeneralGridSize();
    }

    private void RespawnButtonClicked()
    {
        animalInputText.text = Mathf.Clamp(float.Parse(animalInputText.text), 0, 100f).ToString();
        plantInputText.text = Mathf.Clamp(float.Parse(plantInputText.text), 0, 100f).ToString();
        MapManager.Instance.animalSpawnPercent = float.Parse(animalInputText.text);
        MapManager.Instance.plantSpawnPercent = float.Parse(plantInputText.text);
        MapManager.Instance.respawnAll = true;
    }

    private void SpawnPlantButtonClicked()
    {
        MapManager.Instance.SpawnLifeRandomCell(true);
    }

    private void SpawnAnimalButtonClicked()
    {
        MapManager.Instance.SpawnLifeRandomCell(false);
    }

    private void HideMenuButtonClicked()
    {
        hiddenOptions = !hiddenOptions;

        if (hiddenOptions)
        {
            hideOptionsText.text = "v";
            optionsMenuTransform.anchoredPosition = new Vector3(0, hideOptionsDistance, 0);
        }
        else
        {
            hideOptionsText.text = "^";
            optionsMenuTransform.anchoredPosition = Vector3.zero;
        }
    }
    #endregion


    #region ---=== Update Data/UI ===---
    //Setting the data based on the UI
    private void UpdateAutoSpawnData()
    {
        plantTimerInputText.text = Mathf.Clamp(float.Parse(plantTimerInputText.text), 1f, 999f).ToString();
        animalTimerInputText.text = Mathf.Clamp(float.Parse(animalTimerInputText.text), 1f, 999f).ToString();
        LifeformManager.Instance.plantSpawnTimeMax = float.Parse(plantTimerInputText.text);
        LifeformManager.Instance.animalSpawnTimeMax = float.Parse(animalTimerInputText.text);
        LifeformManager.Instance.plantSpawnTime = LifeformManager.Instance.plantSpawnTimeMax;
        LifeformManager.Instance.animalSpawnTime = LifeformManager.Instance.animalSpawnTimeMax;
    }

    private void UpdateAllUI()
    {
        UpdateAutoSpawnUI();
        UpdateGridUI();
        UpdatePopulationUI();
    }

    //Setting the UI based on the data
    public void UpdateAutoSpawnUI()
    {
        if (plantTimerToggle != null)
        {
            plantTimerToggle.isOn.Equals(LifeformManager.Instance.spawnNewPlantByTimer);
            animalTimerToggle.isOn.Equals(LifeformManager.Instance.spawnNewAnimalByTimer);
            plantTimerInputText.text = string.Concat(LifeformManager.Instance.plantSpawnTimeMax);
            animalTimerInputText.text = string.Concat(LifeformManager.Instance.animalSpawnTimeMax);
        }
    }

    public void UpdateGridUI()
    {
        if (xInputText != null)
        {
            xInputText.text = string.Concat(MapManager.Instance.cellWidth);
            yInputText.text = string.Concat(MapManager.Instance.cellHeight);
        }
    }

    public void UpdatePopulationUI()
    {
        if (animalInputText != null)
        {
            animalInputText.text = string.Concat(MapManager.Instance.animalSpawnPercent);
            plantInputText.text = string.Concat(MapManager.Instance.plantSpawnPercent);
        }
    }

    public void UpdatePlantPopulationText(int _newPopulation)
    {
        if (plantPopulationText != null)
            plantPopulationText.text = string.Concat(_newPopulation);
    }

    public void UpdateAnimalPopulationText(int _newPopulation)
    {
        if (animalPopulationText != null)
            animalPopulationText.text = string.Concat(_newPopulation);
    }
    #endregion
}
