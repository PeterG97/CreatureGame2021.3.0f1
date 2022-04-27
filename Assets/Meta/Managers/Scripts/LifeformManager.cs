/* Contains the functions to create and initialize plants and animals and stores references to
 * various assets used for setting up lifeforms. Lifeforms can be spawned using the
 * SpawnNewPlant/Animal methods. After spawning, lifeforms need to either be sent to the Randomize
 * methods or have their base values directly set. RandomizeLifeform chooses all of their
 * values based on the potential starting ranges defined by the Lifeform Values scriptable object
 * which acts as a central definition of lifeform properties. After a lifeform has its base traits
 * setup or changed it needs to be updated in the UpdatePlant/Animal methods which updates the
 * visuals and its derived values based on its base values as well as resetting its timers and
 * hunger/hp. The LifeformManager class also includes reproduction methods for both plants and
 * animals which chooses traits for their child based on the parents with some random variation
 * added on top. This file also contains the LifeformObjects class which is used to refer to
 * objects that could be either a plant or animal while linking their lifeform class to their
 * game object. And there are some enum values that plants and animals use at the bottom of the file.
 * 
 * Dependent on classes:
 * PermanentMonoSingleton - GameManager */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeformManager : MonoSingleton<LifeformManager>
{
    #region ---=== Serialized Variables ===---
    //Reference to scriptable object which will be copied for runtime instance
    [SerializeField] private LifeformValues lifeformValuesBase;

    [Header("Prefabs")]
    [SerializeField] public GameObject plantPrefab;
    [SerializeField] public GameObject animalPrefab;

    [Header("Sprites")]
    [SerializeField] public Sprite[] plantSprites;
    [SerializeField] public Sprite[] bodySprites;
    [SerializeField] public Sprite[] eyeSprites;
    [SerializeField] public Sprite[] mouthSprites;
    #endregion

    #region ---=== Nonserialized Variables ===---
    //Object references
    [NonSerialized] private GameObject plantSceneParent;
    [NonSerialized] private GameObject animalSceneParent;

    //Population tracker for performance menu
    [NonSerialized] private int plantPopulation;
    [NonSerialized] private int animalPopulation;

    //Options Menu
    [NonSerialized] public bool spawnNewPlantByTimer = false; //Assumed false based on options UI
    [NonSerialized] public float plantSpawnTimeMax = 20f;
    [NonSerialized] public float plantSpawnTime = 1f;
    [NonSerialized] public bool spawnNewAnimalByTimer = false; //Assumed false based on options UI
    [NonSerialized] public float animalSpawnTimeMax = 60f;
    [NonSerialized] public float animalSpawnTime = 1f;
    [NonSerialized] public float spawnNewTimeVariation = 0.1f;
    #endregion

    #region ---=== Get/Set Variables ===---
    [NonSerialized] private LifeformValues lifeformValues;
    public LifeformValues LifeformValues
    {
        get
        {
            if (lifeformValues == null)
                lifeformValues = Instantiate(lifeformValuesBase);

            return lifeformValues;
        }
        private set { } //Never direcly set
    }
    public int PlantPopulation
    {
        get { return plantPopulation; }
        set
        {
            plantPopulation = value;

            if (UIManager.Instance != null)
                UIManager.Instance.UpdatePlantPopulationText(plantPopulation);
        }
    }
    public int AnimalPopulation
    {
        get { return animalPopulation; }
        set
        {
            animalPopulation = value;

            if (UIManager.Instance != null)
                UIManager.Instance.UpdateAnimalPopulationText(animalPopulation);
        }
    }
    #endregion


    #region ---=== Main Methods ===---
    protected override void Awake()
    {
        base.Awake();

        plantSpawnTime = plantSpawnTimeMax;
        animalSpawnTime = animalSpawnTimeMax;
    }

    private void FixedUpdate()
    {

        if (MapManager.Instance != null)
        {
            if (spawnNewPlantByTimer)
            {
                plantSpawnTime -= Time.deltaTime;

                if (plantSpawnTime <= 0)
                {
                    MapManager.Instance.SpawnLifeRandomCell(true);
                    plantSpawnTime = UnityEngine.Random.Range(plantSpawnTimeMax - (plantSpawnTimeMax * spawnNewTimeVariation),
                                                              plantSpawnTimeMax + (plantSpawnTimeMax * spawnNewTimeVariation));
                }
            }

            if (spawnNewAnimalByTimer)
            {
                animalSpawnTime -= Time.deltaTime;

                if (animalSpawnTime <= 0)
                {
                    MapManager.Instance.SpawnLifeRandomCell(false);
                    animalSpawnTime = UnityEngine.Random.Range(animalSpawnTimeMax - (animalSpawnTimeMax * spawnNewTimeVariation),
                                                               animalSpawnTimeMax + (animalSpawnTimeMax * spawnNewTimeVariation));
                }
            }
        }
    }
    #endregion


    #region ---=== General Use Methods ===---
    public Plant SpawnNewPlant(Vector2 _pos)
    {
        GameObject newPlant = Instantiate(plantPrefab, new Vector3(_pos.x, _pos.y,
                                          transform.position.z), Quaternion.identity);
        PlantPopulation++;

        newPlant.name = string.Concat("Plant_", newPlant.gameObject.GetInstanceID().ToString().Substring(1));
        
        if (CheckNull.SingleObjectNotNull(ref plantSceneParent, "Plants", true))
            newPlant.transform.SetParent(plantSceneParent.transform);

        if (MapManager.Instance != null)
            MapManager.Instance.generalGrid.SetValue(new Vector2(_pos.x, _pos.y), newPlant.GetInstanceID());

        return newPlant.GetComponent<Plant>();
    }

    public Animal SpawnNewAnimal(Vector2 _pos)
    {
        GameObject newAnimal = Instantiate(animalPrefab, new Vector3(_pos.x, _pos.y,
                                           transform.position.z), Quaternion.identity);
        AnimalPopulation++;

        newAnimal.name = string.Concat("Animal_", newAnimal.gameObject.GetInstanceID().ToString().Substring(1));
        
        if (CheckNull.SingleObjectNotNull(ref animalSceneParent, "Animals", true))
            newAnimal.transform.SetParent(animalSceneParent.transform);

        return newAnimal.GetComponent<Animal>();
    }


    #region ---=== Randomize Lifeforms ===---
    public void RandomizeLifeform(Plant _plant)
    {
        //Age
        _plant.maxAge = LifeformValues.PlantMaxAge * UnityEngine.Random.Range(1 - LifeformValues.AgeVariation, 1 + LifeformValues.AgeVariation);

        //Body
        _plant.size = UnityEngine.Random.Range(LifeformValues.PlantMinSpawnSize, LifeformValues.PlantMaxSpawnSize);
        _plant.color = Color.HSVToRGB(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0.7f, 1f));

        _plant.plantIndex = UnityEngine.Random.Range(0, plantSprites.Length);

        //Turn into ability TODO
        _plant.reproductionWaitTime = UnityEngine.Random.Range(LifeformValues.PlantReproductionTimePercentMin * _plant.maxAge,
                                                               LifeformValues.PlantReproductionTimePercentMax * _plant.maxAge);

        //Abilities

        UpdateLifeform(_plant);
    }

    public void RandomizeLifeform(Animal _animal)
    {
        //Age
        _animal.maxAge = LifeformValues.AnimalMaxAge * UnityEngine.Random.Range(1 - LifeformValues.AgeVariation, 1 + LifeformValues.AgeVariation);
        _animal.deathAge = _animal.maxAge; //Even though this is a derived stat that is in UpdateAnimalAll is not set Age cannot be set without death
        if (UnityEngine.Random.value < LifeformValues.NewSpawnChildChance)
            _animal.adult = false;
        else
        {
            _animal.adult = true;
            _animal.Age = LifeformValues.AnimalAdultAgePercent * _animal.maxAge;
        }

        //Body
        _animal.size = UnityEngine.Random.Range(LifeformValues.AnimalMinSpawnSize, LifeformValues.AnimalMaxSpawnSize);
        _animal.color = Color.HSVToRGB(UnityEngine.Random.Range(0, 1f),
                                       UnityEngine.Random.Range(0, 1f),
                                       UnityEngine.Random.Range(LifeformValues.MinColorValue, 1f));
        
        _animal.bodyIndex = UnityEngine.Random.Range(0, bodySprites.Length);
        _animal.eyeIndex = UnityEngine.Random.Range(0, eyeSprites.Length);
        _animal.mouthIndex = UnityEngine.Random.Range(0, mouthSprites.Length);

        //Behavior
        _animal.sexualReproduction = true;
        if (UnityEngine.Random.value < LifeformValues.ChanceAsexual)
            _animal.sexualReproduction = false;
        _animal.diet = RandomAnimalDiet();
        _animal.nature = RandomAnimalNature();
        _animal.social = RandomAnimalSocial();

        //Abilities
        _animal.sight = UnityEngine.Random.Range(LifeformValues.AnimalsMinSight, LifeformValues.AnimalsMaxSight);
        _animal.attackPower = UnityEngine.Random.Range(LifeformValues.AnimalsAttackMin, LifeformValues.AnimalsAttackMax);
        _animal.hitPointsRegenSpeed = UnityEngine.Random.Range(0, LifeformValues.AnimalsHPRegenMax);
        _animal.reproductionNegatives = UnityEngine.Random.Range(LifeformValues.AnimalReproductionNegativesMin, LifeformValues.AnimalReproductionNegativesMax);
        _animal.reproductionWaitTime = LifeformValues.AnimalReproductionTimePercent * _animal.maxAge * _animal.reproductionNegatives;

        UpdateLifeform(_animal);
    }
    #endregion


    #region ---=== Update Lifeforms ===---
    public void UpdateLifeform(Plant _plant)
    {
        //Age
        _plant.deathAge = _plant.maxAge;

        //Nutrition
        _plant.maxNutrition = LifeformValues.PlantBaseMaxNutrition * Mathf.Pow(_plant.size, LifeformValues.NutritionPower);
        _plant.Nutrition = UnityEngine.Random.Range(_plant.maxNutrition * LifeformValues.PlantStartingNutritionMin,
                                        _plant.maxNutrition * LifeformValues.PlantStartingNutritionMax);

        //Body
        _plant.transform.localScale = new Vector3(_plant.size, _plant.size, _plant.transform.localScale.z);
        SpriteRenderer spriteRenderer = _plant.transform.Find("Sprites").Find("MainSprite").GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = plantSprites[_plant.plantIndex];
        spriteRenderer.color = _plant.color;
        if (UnityEngine.Random.value < 0.5f) //50% chance to flip sprite
            spriteRenderer.transform.localScale = new Vector3(spriteRenderer.transform.localScale.x * -1,
                                                              spriteRenderer.transform.localScale.y,
                                                              spriteRenderer.transform.localScale.z);

        //Derived Stats
        _plant.maxHitPoints = Mathf.CeilToInt(LifeformValues.PlantBaseHP * Mathf.Pow(_plant.size, LifeformValues.HpPower));
        _plant.HitPoints = _plant.maxHitPoints;

        _plant.growRate = LifeformValues.PlantBaseGrowRate * Mathf.Pow(_plant.size, LifeformValues.NutritionChangeRatePower);

        //Abilities
        //TODO
        //Depends on abilities
        /*
        _plant.growRate = (plantBaseGrowRate * Mathf.Pow(_plant.size, nutritionChangeRatePower))
                                * (0.5f * _animal.sight + 0.5f) //Sight has a scaling linear relationship 0.5x + 0.5 (1 Sight = 1 hunger)
                                * (0.1f * _animal.attackPower + 0.8f); //Attack has a scaling linear relationship 0.1x + 0.8 (2 Attack = 1 hunger)
        */

        _plant.reproductionTimer = 1f;
        _plant.Resize();
    }

    public void UpdateLifeform(Animal _animal)
    {
        //Updates all animal information based on current variabled and resets to a starting state

        //Age
        _animal.deathAge = _animal.maxAge;
        _animal.adultAge = LifeformValues.AnimalAdultAgePercent * _animal.maxAge;

        //Nutrition
        _animal.maxNutrition = LifeformValues.AnimalsBaseMaxNutrition * Mathf.Pow(_animal.size, LifeformValues.NutritionPower);

        //Body
        _animal.transform.localScale = new Vector3(_animal.size, _animal.size, _animal.transform.localScale.z);
        _animal.transform.position = new Vector3(_animal.transform.position.x, _animal.transform.position.y, -(_animal.transform.localScale.y - 1) / 2);
        SetAnimalBody(_animal, _animal.bodyIndex, _animal.color);
        SetAnimalEye(_animal, _animal.eyeIndex);
        SetAnimalMouth(_animal, _animal.mouthIndex);
        _animal.slimeParticles.startColor = _animal.color;

        //Derived Stats
        _animal.maxHitPoints = Mathf.CeilToInt(LifeformValues.AnimalsBaseHP * Mathf.Pow(_animal.size, LifeformValues.HpPower));
        _animal.HitPoints = _animal.maxHitPoints;

        _animal.moveSpeed = LifeformValues.AnimalsBaseMoveSpeed * Mathf.Pow(_animal.size, LifeformValues.MoveSpeedPower);
        _animal.wanderTimerMax = _animal.size * 3f;
        _animal.reproductionWaitTime = LifeformValues.AnimalReproductionTimePercent * _animal.maxAge * _animal.reproductionNegatives;

        //Abilities (increase hungerRate)
        _animal.hungerRate = (LifeformValues.AnimalsBaseHungerRate * Mathf.Pow(_animal.size, LifeformValues.NutritionChangeRatePower))
                        * (0.5f * MathF.Pow(_animal.sight, 3) + 0.5f) //Sight has an exponential relationship 0.5x^3 + 0.5 (1 = 1 hunger)
                        * (0.1f * MathF.Pow(_animal.attackPower, 2) + 0.8f) //Attack has an exponential relationship 0.1x^2 + 0.8 (2 = 1 hunger)
                        * (1000f * MathF.Pow(_animal.hitPointsRegenSpeed, 2) + 1f) //HP Regen has an exponential relationship 1000x^2 + 1 (0 = 1 hunger, 0.02 = 1.4 hunger)
                        * (-MathF.Pow(_animal.reproductionNegatives, 3) / 10f + 1f); //Reproduction Negatives has an exponential relationship -x^3/10 + 1 (0 = 1 hunger, 1 = 0.9, 2.15 = 0)

        //Modified based on sight
        _animal.GetComponent<CircleCollider2D>().radius = LifeformValues.AnimalsBaseSightRadius * _animal.sight * _animal.size;


        _animal.sprites.localPosition = Vector3.zero;

        //Set child stats
        if (!_animal.adult)
        {
            AnimalMultiplyAgeStats(_animal, LifeformValues.AnimalChildStatMult);
            _animal.ResizeChild();
        }

        //Calculated after baby check because it changes maxNutrition
        _animal.Nutrition = UnityEngine.Random.Range(_animal.maxNutrition * LifeformValues.AnimalStartingNutritionMin,
                                                     _animal.maxNutrition * LifeformValues.AnimalStartingNutritionMax);

        //Reset control important variables
        _animal.Action = AnimalAction.Idle;
        _animal.reproductionTimer = _animal.reproductionWaitTime;
    }
    #endregion


    #region ---=== Reproduction ===---
    public Animal AnimalSexualReproduction(Animal _animal1, Animal _animal2)
    {
        Animal _baby = SpawnNewAnimal(_animal1.transform.position);
        float variationMult = 1 - LifeformValues.ReproductionGeneticVariation;

        //Info
        if (_animal1.generations >= _animal2.generations)
            _baby.generations = _animal1.generations + 1;
        else
            _baby.generations = _animal2.generations + 1;

        //Max Age
        float parentsMaxAge = (_animal1.maxAge + _animal2.maxAge) / 2;
        _baby.maxAge = UnityEngine.Random.Range(parentsMaxAge * variationMult, parentsMaxAge / variationMult);

        //Body
        float parentsSize = (_animal1.size + _animal2.size) / 2;
        _baby.size = UnityEngine.Random.Range(parentsSize * variationMult, parentsSize / variationMult);
        _baby.size = Mathf.Clamp(_baby.size, LifeformValues.AnimalMinSize, LifeformValues.AnimalMaxSize);

        //Color
        Color.RGBToHSV(_animal1.color, out float H1, out float S1, out float V1);
        Color.RGBToHSV(_animal2.color, out float H2, out float S2, out float V2);

        H1 = (H1 + H2) / 2;
        S1 = (S1 + S2) / 2;
        V1 = (V1 + V2) / 2;

        V1 = UnityEngine.Random.Range(V1 * variationMult, V1 / variationMult);
        if (V1 < LifeformValues.MinColorValue)
            V1 = LifeformValues.MinColorValue;

        _baby.color = Color.HSVToRGB(UnityEngine.Random.Range(H1 * variationMult, H1 / variationMult),
                                     UnityEngine.Random.Range(S1 * variationMult, S1 / variationMult),
                                     V1);

        if (UnityEngine.Random.value < LifeformValues.ReproductionGeneticVariation) //Low chance for random body
            _baby.bodyIndex = UnityEngine.Random.Range(0, bodySprites.Length);
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.bodyIndex = _animal1.bodyIndex;
            else
                _baby.bodyIndex = _animal2.bodyIndex;
        }
        if (UnityEngine.Random.value < LifeformValues.ReproductionGeneticVariation) //Low chance for random eye
            _baby.eyeIndex = UnityEngine.Random.Range(0, eyeSprites.Length);
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.eyeIndex = _animal1.eyeIndex;
            else
                _baby.eyeIndex = _animal2.eyeIndex;
        }
        if (UnityEngine.Random.value < LifeformValues.ReproductionGeneticVariation) //Low chance for random mouth
            _baby.mouthIndex = UnityEngine.Random.Range(0, mouthSprites.Length);
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.mouthIndex = _animal1.mouthIndex;
            else
                _baby.mouthIndex = _animal2.mouthIndex;
        }

        //Behavior
        if (UnityEngine.Random.value < LifeformValues.ReproductionDietVariation) //Low chance for different Diet
            _baby.diet = RandomAnimalDiet();
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.diet = _animal1.diet;
            else
                _baby.diet = _animal2.diet;
        }

        if (UnityEngine.Random.value < LifeformValues.ReproductionBehaviorVariation) //Low chance for different nature
            _baby.nature = RandomAnimalNature();
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.nature = _animal1.nature;
            else
                _baby.nature = _animal2.nature;
        }

        if (UnityEngine.Random.value < LifeformValues.ReproductionBehaviorVariation) //Low chance for different social
            _baby.social = RandomAnimalSocial();
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.social = _animal1.social;
            else
                _baby.social = _animal2.social;
        }

        if (UnityEngine.Random.value < LifeformValues.ReproductionTypeVariation) //Low chance to become asexual
            _baby.sexualReproduction = false;
        else
            _baby.sexualReproduction = true;

        //Reproduction
        float parentsReproductionTimerMax = (_animal1.reproductionWaitTime + _animal2.reproductionWaitTime) / 2;
        _baby.reproductionWaitTime = UnityEngine.Random.Range(parentsReproductionTimerMax * variationMult, parentsReproductionTimerMax / variationMult);

        //Abilities
        float parentsSight = (_animal1.sight + _animal2.sight) / 2;
        _baby.sight = UnityEngine.Random.Range(parentsSight * variationMult, parentsSight / variationMult);

        float  parentsAttackPower = (_animal1.attackPower + _animal2.attackPower) / 2;
        _baby.attackPower = UnityEngine.Random.Range(parentsAttackPower * variationMult, parentsAttackPower / variationMult);

        float parentsRegenSpeed = (_animal1.hitPointsRegenSpeed + _animal2.hitPointsRegenSpeed) / 2;
        _baby.hitPointsRegenSpeed = UnityEngine.Random.Range(parentsRegenSpeed * variationMult, parentsRegenSpeed / variationMult);

        float parentsReproductionNegatives = (_animal1.reproductionNegatives + _animal2.reproductionNegatives) / 2;
        _baby.reproductionNegatives = UnityEngine.Random.Range(parentsReproductionNegatives * variationMult, parentsReproductionNegatives / variationMult);
        _baby.reproductionNegatives = Mathf.Clamp(_baby.reproductionNegatives, 0.1f, 2f);

        //Update Baby
        _baby.adult = false;
        UpdateLifeform(_baby);
        _baby.Nutrition = _baby.maxNutrition / 2;
        _baby.parents.Add(_animal1);
        _baby.parents.Add(_animal2);

        //Update Parents
        _animal1.AfterReproduce(LifeformValues.SexualNutritionLost, LifeformValues.SexualDeathAgeLost);
        _animal2.AfterReproduce(LifeformValues.SexualNutritionLost, LifeformValues.SexualDeathAgeLost);

        return _baby;
    }

    public Animal AnimalAsexualReproduction(Animal _animal)
    {
        Animal _baby = SpawnNewAnimal(_animal.transform.position);

        float variationMult = 1 - LifeformValues.ReproductionGeneticVariation;

        //Info
        _baby.generations = _animal.generations + 1;

        //Max Age
        _baby.maxAge = UnityEngine.Random.Range(_animal.maxAge * variationMult, _animal.maxAge / variationMult);

        //Body
        _baby.size = UnityEngine.Random.Range(_animal.size * variationMult, _animal.size / variationMult);
        _baby.size = Mathf.Clamp(_baby.size, LifeformValues.AnimalMinSize, LifeformValues.AnimalMaxSize);

        //Color
        Color.RGBToHSV(_animal.color, out float H1, out float S1, out float V1);

        V1 = UnityEngine.Random.Range(V1 * variationMult, V1 / variationMult);
        if (V1 < LifeformValues.MinColorValue)
            V1 = LifeformValues.MinColorValue;

        _baby.color = Color.HSVToRGB(UnityEngine.Random.Range(H1 * variationMult, H1 / variationMult),
                                     UnityEngine.Random.Range(S1 * variationMult, S1 / variationMult),
                                     V1);

        if (UnityEngine.Random.value > variationMult) //Low chance for random body
            _baby.bodyIndex = UnityEngine.Random.Range(0, bodySprites.Length);
        else
        {
            _baby.bodyIndex = _animal.bodyIndex;
        }
        if (UnityEngine.Random.value > variationMult) //Low chance for random eye
            _baby.eyeIndex = UnityEngine.Random.Range(0, eyeSprites.Length);
        else
        {
            _baby.eyeIndex = _animal.eyeIndex;
        }
        if (UnityEngine.Random.value > variationMult) //Low chance for random mouth
            _baby.mouthIndex = UnityEngine.Random.Range(0, mouthSprites.Length);
        else
        {
            _baby.mouthIndex = _animal.mouthIndex;
        }

        //Behavior
        if (UnityEngine.Random.value < LifeformValues.ReproductionDietVariation) //Low chance for different Diet
            _baby.diet = RandomAnimalDiet();
        else
            _baby.diet = _animal.diet;

        if (UnityEngine.Random.value < LifeformValues.ReproductionBehaviorVariation) //Low chance for different nature
            _baby.nature = RandomAnimalNature();
        else
        {
            _baby.nature = _animal.nature;
        }
        if (UnityEngine.Random.value < LifeformValues.ReproductionBehaviorVariation) //Low chance for different social
            _baby.social = RandomAnimalSocial();
        else
        {
            _baby.social = _animal.social;
        }

        if (UnityEngine.Random.value < LifeformValues.ReproductionTypeVariation) //Low chance to become sexual
            _baby.sexualReproduction = true;
        else
            _baby.sexualReproduction = false;

        //Reproduction
        _baby.reproductionWaitTime = UnityEngine.Random.Range(_animal.reproductionWaitTime * variationMult, _animal.reproductionWaitTime / variationMult);

        //Abilities
        _baby.sight = UnityEngine.Random.Range(_animal.sight * variationMult, _animal.sight / variationMult);
        _baby.attackPower = UnityEngine.Random.Range(_animal.attackPower * variationMult, _animal.attackPower / variationMult);
        _baby.hitPointsRegenSpeed = UnityEngine.Random.Range(_animal.hitPointsRegenSpeed * variationMult, _animal.hitPointsRegenSpeed / variationMult);
        _baby.reproductionNegatives = UnityEngine.Random.Range(_animal.reproductionNegatives * variationMult, _animal.reproductionNegatives / variationMult);
        _baby.reproductionNegatives = Mathf.Clamp(_baby.reproductionNegatives, 0.1f, 2f);

        //Update Baby
        _baby.adult = false;
        UpdateLifeform(_baby);
        _baby.Nutrition = _baby.maxNutrition / 2;
        _baby.parents.Add(_animal);

        //Update Parent
        _animal.AfterReproduce(LifeformValues.SexualNutritionLost, LifeformValues.AsexualDeathAgeLost);

        return _baby;
    }

    public Plant PlantReproduction(Plant _plant)
    {
        //Can only reproduce if there is a MapManager
        if (MapManager.Instance == null)
            return null;

        Plant _baby = MapManager.Instance.SpawnPlantOnCell(MapManager.Instance.generalGrid.GetAdjacentRandomCell(_plant.gridLocation));
        
        if (_baby == null)
            return _baby;

        float variationMult = 1 - LifeformValues.ReproductionGeneticVariation;

        //Info
        _baby.generations = _plant.generations + 1;

        //Max Age
        _baby.maxAge = UnityEngine.Random.Range(_plant.maxAge * variationMult, _plant.maxAge / variationMult);

        //Body
        _baby.size = UnityEngine.Random.Range(_plant.size * variationMult, _plant.size / variationMult);
        _baby.size = Mathf.Clamp(_baby.size, LifeformValues.PlantMinSize, LifeformValues.PlantMaxAge);

        //Color
        Color.RGBToHSV(_plant.color, out float H1, out float S1, out float V1);

        V1 = UnityEngine.Random.Range(V1 * variationMult, V1 / variationMult);
        if (V1 < 0.7)
            V1 = 0.7f;

        _baby.color = Color.HSVToRGB(UnityEngine.Random.Range(H1 * variationMult, H1 / variationMult),
                                     UnityEngine.Random.Range(S1 * variationMult, S1 / variationMult),
                                     V1);

        if (UnityEngine.Random.value > variationMult) //Low chance for random body
            _baby.plantIndex = UnityEngine.Random.Range(0, plantSprites.Length);
        else
        {
            _baby.plantIndex = _plant.plantIndex;
        }

        //Reproduction
        _baby.reproductionWaitTime = UnityEngine.Random.Range(_plant.reproductionWaitTime * variationMult, _plant.reproductionWaitTime / variationMult);

        //Abilities
        //_baby.sight = UnityEngine.Random.Range(_plant.sight * variationMult, _plant.sight / variationMult);

        //Update Baby
        UpdateLifeform(_baby);
        _baby.Nutrition = _baby.maxNutrition * LifeformValues.BabyPlantStartingNutrition;
        _baby.Resize();

        //Update Parents
        _plant.AfterReproduce(LifeformValues.PlantReproductionDeathAgeLost);

        return _baby;
    }
    #endregion


    public float CalcColorSimilarity(Color _firstColor, Color _secondColor)
    {
        //Returns 0-1 which will function as a multiplier

        float colorSimilarity = 1 - (Mathf.Abs((_firstColor.r - _secondColor.r) / 3)
                                + (Mathf.Abs(_firstColor.g - _secondColor.g) / 3)
                                + (Mathf.Abs(_firstColor.b - _secondColor.b) / 3));

        return colorSimilarity;
    }

    public void AnimalMultiplyAgeStats(Animal _animal, float _mult)
    {
        //Stats
        _animal.maxHitPoints *= _mult;
        _animal.moveSpeed *= _mult;
        _animal.maxNutrition *= _mult;
        _animal.hungerRate *= _mult;

        //Abilities
        _animal.attackPower *= _mult;
        _animal.hitPointsRegenSpeed *= _mult;
    }
    #endregion


    #region ---=== Local Use Methods ===---
    private void SetAnimalBody(Animal _animal, int _index, Color _color)
    {
        SpriteRenderer spriteRenderer = _animal.transform.Find("Sprites").Find("BodySprite").GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = bodySprites[_index];
        _animal.color = _color;
        spriteRenderer.color = _color;

        _animal.bodyIndex = _index;
    }

    private void SetAnimalEye(Animal _animal, int _index)
    {
        SpriteRenderer spriteRenderer = _animal.transform.Find("Sprites").Find("EyeSprite").GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = eyeSprites[_index];
        spriteRenderer.transform.localScale = new Vector3(_animal.sight, _animal.sight,
                                                            spriteRenderer.transform.localScale.z);

        _animal.eyeIndex = _index;
    }

    private void SetAnimalMouth(Animal _animal, int _index)
    {
        SpriteRenderer spriteRenderer = _animal.transform.Find("Sprites").Find("MouthSprite").GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = mouthSprites[_index];

        _animal.mouthIndex = _index;
    }

    private AnimalDiet RandomAnimalDiet()
    {
        float dietValue = UnityEngine.Random.value;
        if (dietValue < LifeformValues.CarnivoreChance)
            return AnimalDiet.Carnivore;
        else if (dietValue < LifeformValues.OmnivoreChance + LifeformValues.CarnivoreChance)
            return AnimalDiet.Omnivore;
        else
            return AnimalDiet.Herbivore;
    }

    private AnimalNature RandomAnimalNature()
    {
        float natureValue = UnityEngine.Random.value;
        if (natureValue < LifeformValues.ViolentChance)
            return AnimalNature.Violent;
        else if (natureValue < LifeformValues.FearfulChance + LifeformValues.ViolentChance)
            return AnimalNature.Fearful;
        else
            return AnimalNature.Neutral;
    }

    private AnimalSocial RandomAnimalSocial()
    {
        float socialValue = UnityEngine.Random.value;
        if (socialValue < LifeformValues.FollowerChance)
            return AnimalSocial.Follower;
        else if (socialValue < LifeformValues.ProtectiveChance + LifeformValues.FollowerChance)
            return AnimalSocial.Protective;
        else if (socialValue < LifeformValues.AntisocialChance + LifeformValues.ProtectiveChance + LifeformValues.FollowerChance)
            return AnimalSocial.Antisocial;
        else
            return AnimalSocial.Neutral;
    }
    #endregion
}

[SerializeField]
public class LifeformObject
{
    public GameObject obj;
    public LifeformType type;
    public Plant plant;
    public Animal animal;

    public LifeformObject()
    {
        obj = null;
        type = LifeformType.Plant;
    }

    public LifeformObject(GameObject _obj)
    {
        obj = _obj;
        plant = _obj.GetComponent<Plant>();
        animal = _obj.GetComponent<Animal>();
        //TODO add player defining script

        if (plant != null)
            type = LifeformType.Plant;
        else if (animal != null)
            type = LifeformType.Animal;
        else
            type = LifeformType.Player;
    }

    public LifeformObject(GameObject _obj, LifeformType _type)
    {
        obj = _obj;
        type = _type;

        if (_type == LifeformType.Plant)
            plant = _obj.GetComponent<Plant>();
        else if (_type == LifeformType.Animal)
            animal = _obj.GetComponent<Animal>();
        else
        { //TODO
        }
    }
}

public enum AnimalAction
{
    //Basically their short-term goal which entails moving relative to their target and maybe attempting to interact
    Idle, //Wander random direction, if they find a target this is overruled
    Follow, //Move to friendly targetObj (similar to idle as there is no real goal except for grouping similar animals)
    Run, //Moving away from targetObj
    Attack, //Moving to and attacking targetObj
    Eat, //Moving to and eating targetObj
    Reproduce //Moveing to and attempting to reproduce with target
}

public enum AnimalDiet
{
    Herbivore,
    Omnivore,
    Carnivore
}

public enum AnimalNature
{
    Neutral, //Attacks if attacked or if hungry & carnivore
    Fearful, //Runs if attacked only fights if hungry & carnivore
    Violent //Attack every visable animal
}

public enum AnimalSocial
{
    Neutral, //No extra behavior
    Follower, //Tries to follow children if not then follow friendly
    Protective, //Attacks violent animals
    Antisocial //Runs from all animals until it needs them
}

public enum LifeformType
{
    Plant,
    Animal,
    Player
}