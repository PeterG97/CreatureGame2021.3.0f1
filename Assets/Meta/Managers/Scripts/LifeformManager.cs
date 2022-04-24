using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LifeformManager : MonoSingleton<LifeformManager>
{
    #region ---=== Serialized Variables ===---
    [Header("Age - Spawning")]
    [Tooltip("Second until a old age death (Default 180)")]
    [SerializeField] public float plantMaxAge = 180f;
    [Tooltip("Second until a old age death [Will be reduced in game from certain actions such as fighting or breeding] (Default 600)")]
    [SerializeField] public float animalMaxAge = 600f;
    [Tooltip("Percent random age variation (Default 0.1)")]
    [SerializeField] public float ageVariation = 0.1f;
    [Tooltip("Percent of maxAge when animal become adult (Default 0.25)")]
    [SerializeField] public float animalAdultAgePercent = 0.25f;
    [Tooltip("The chance for a child animal to appear when an animal is spawned (Default 0.25)")]
    [SerializeField] public float newSpawnChildChance = 0.25f;
    [Tooltip("What relavent child stats will be multiplied by (Default 0.5)")]
    [SerializeField] public float animalChildStatMult = 0.5f;

    [Header("Size - Spawning")]
    [SerializeField] public float plantMinSpawnSize = 0.75f;
    [SerializeField] public float plantMaxSpawnSize = 1.25f;
    [SerializeField] public float animalMinSpawnSize = 0.75f;
    [SerializeField] public float animalMaxSpawnSize = 1.25f;

    [Header("HP - Spawning")]
    [SerializeField] public float plantBaseHP = 5f;
    [SerializeField] public float animalsBaseHP = 10f;
    [Tooltip("Power to scale hp based on body size (Default 2)")]
    [SerializeField] public float hpPower = 2f;

    [Header("Movespeed - Spawning")]
    [SerializeField] public float animalsBaseMoveSpeed = 20f;
    [Tooltip("Power to inversely scale move speed based on body size (Default -1)")]
    [SerializeField] public float moveSpeedPower = -1f;

    [Header("Plant Growth - Spawning")]
    //Plant nutrition counts up to a max value
    [Tooltip("Essentially max growth (Default 3000)")]
    [SerializeField] public float plantBaseMaxNutrition = 3000f; //Min nutrition is 0
    [Tooltip("Gain of nutrition per fixedUpdate (Default 1)")]
    [SerializeField] public float plantBaseGrowRate = 1f;
    [Tooltip("Low end of new plant's nutrition (Default 0.1)")]
    [SerializeField] public float plantStartingNutritionMin = 0.1f;
    [Tooltip("High end of new plant's nutrition (Default 0.6)")]
    [SerializeField] public float plantStartingNutritionMax = 0.6f;
    [Tooltip("Baby plant's starting nutrition (Default 0.6)")]
    [SerializeField] public float babyPlantStartingNutrition = 0.01f;

    [Header("Animal Hunger - Spawning")]
    //Animal nutrition counts down to 0
    [Tooltip("Essentially max fullness, maximum fixed update frames to die if animalsBaseHungerRate is 1 (Default 10000)")]
    [SerializeField] public float animalsBaseMaxNutrition = 10000f; //Min nutrition is 0
    [Tooltip("Loss of nutrition per fixedUpdate (Default 1)")]
    [SerializeField] public float animalsBaseHungerRate = 1f;
    [Tooltip("Low end of new animal's nutrition (Default 0.1)")]
    [SerializeField] public float animalStartingNutritionMin = 0.6f;
    [Tooltip("High end of new animal's nutrition (Default 0.6)")]
    [SerializeField] public float animalStartingNutritionMax = 1f;

    [Header("Nutrition - Spawning")]
    [Tooltip("Power to scale hungerRate based on body size (Default 2)")]
    [SerializeField] public float nutritionPower = 2f;
    [Tooltip("Power to scale nutrtion change based on size (Default 2)")]
    [SerializeField] public float nutritionChangeRatePower = 2f;

    [Header("Animal Behavior - Spawning")]
    [Tooltip("Percent of random spawns that will be carnivores [remainder will be herbivores] (Default 0.05)")]
    [SerializeField] public float carnivoreChance = 0.05f;
    [Tooltip("Percent of random spawns that will be omnivores [remainder will be herbivores] (Default 0.15)")]
    [SerializeField] public float omnivoreChance = 0.15f;
    [Tooltip("Percent of random spawns that will be violent [remainder will be neutral] (Default 0.05)")]
    [SerializeField] public float violentChance = 0.05f;
    [Tooltip("Percent of random spawns that will be fearful [remainder will be fearful] (Default 0.3)")]
    [SerializeField] public float fearfulChance = 0.3f;

    [Header("Animal Abilities - Spawning")]
    [Tooltip("Min random sight (Default 0.5)")]
    [SerializeField] public float animalsMinSight = 0.5f;
    [Tooltip("Max random sight (Default 1)")]
    [SerializeField] public float animalsMaxSight = 1.5f;
    [Tooltip("Size of vision circle collider 2D (Default 5)")]
    [SerializeField] public float animalsBaseSightRadius = 5f;
    [Tooltip("Attack stat min (Default 1)")]
    [SerializeField] public float animalsAttackMin = 1f;
    [Tooltip("Attack stat max (Default 4)")]
    [SerializeField] public float animalsAttackMax = 4f;
    [Tooltip("Max HP regained every fixed update frame (Default 0.02)")]
    [SerializeField] public float animalsHPRegenMax = 0.02f;

    [Header("Reproduction - Spawning")]
    [Tooltip("Percent variation below and above the average of the parents stats (Default 0.05)")]
    [SerializeField] public float reproductionGeneticVariation = 0.1f;
    [Tooltip("The min amount of a lifeform's age that must pass before they can reporduce again (0.05)")]
    [SerializeField] public float reproductionAgePercentMin = 0.05f;
    [Tooltip("The min amount of an lifeform's age that must pass before they can reporduce again (0.2)")]
    [SerializeField] public float reproductionAgePercentMax = 0.2f;

    [Header("Plant Reproduction - Spawning")]
    [Tooltip("The age which an asexual animal will die is reduced by a percentage of the maxAge (0.1)")]
    [SerializeField] public float plantReproductionDeathAgeLost = 0.1f;

    [Header("Animal Reproduction - Spawning")]
    [Tooltip("The chance for a random animal to be asexual (Default 0.1)")]
    [SerializeField] public float chanceAsexual = 0.1f;
    [Tooltip("Chance that reproduction behavior will be changed to something random (Default 0.1)")]
    [SerializeField] public float reproductionBehaviorVariation = 0.10f;
    [Tooltip("Chance that diet behavior will be changed to something random (Default 0.03)")]
    [SerializeField] public float reproductionDietVariation = 0.03f;
    [Tooltip("Chance that reproduction type will be changed to the opposite type (0.02)")]
    [SerializeField] public float reproductionTypeVariation = 0.02f;
    [Tooltip("Nutrition percent of total lost by each parent when a baby is made (0.2)")]
    [SerializeField] public float sexualNutritionLost = 0.2f;
    [Tooltip("Nutrition percent of total lost by the parent when a baby is made (0.6)")]
    [SerializeField] public float asexualNutritionLost = 0.6f;
    [Tooltip("The age which a sexual animal will die is reduced by a percentage of the maxAge (0.2)")]
    [SerializeField] public float sexualDeathAgeLost = 0.2f;
    [Tooltip("The age which an asexual animal will die is reduced by a percentage of the maxAge (0.4)")]
    [SerializeField] public float asexualDeathAgeLost = 0.4f;

    [Header("Prefabs")]
    [SerializeField] public GameObject plantPrefab;
    [SerializeField] public GameObject animalPrefab;
    [NonSerialized] private GameObject lifefromSceneParent;
    [NonSerialized] private GameObject plantSceneParent;
    [NonSerialized] private GameObject animalSceneParent;

    [Header("Sprites")]
    [SerializeField] public Sprite[] plantSprites;
    [SerializeField] public Sprite[] bodySprites;
    [SerializeField] public Sprite[] eyeSprites;
    [SerializeField] public Sprite[] mouthSprites;
    [Tooltip("The value in HSV color format [Higher == brighter colors] (Default 0.7)")]
    [SerializeField] public float minColorValue = 0.7f;
    #endregion

    #region ---=== Nonserialized Variables ===---
    //Represented with the in game menu
    //Performance Menu
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
                    MapManager.Instance.SpawnLifeRandomCell(true);
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

    public void RandomizePlant(Plant _plant)
    {
        //Age
        _plant.maxAge = plantMaxAge * UnityEngine.Random.Range(1 - ageVariation, 1 + ageVariation);

        //Body
        _plant.size = UnityEngine.Random.Range(plantMinSpawnSize, plantMaxSpawnSize);
        _plant.color = Color.HSVToRGB(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0.7f, 1f));

        _plant.plantIndex = UnityEngine.Random.Range(0, plantSprites.Length);

        //Reproduction
        _plant.reproductionTimerMax = UnityEngine.Random.Range(reproductionAgePercentMin * _plant.maxAge, reproductionAgePercentMax * _plant.maxAge);

        //Abilities

        UpdatePlant(_plant);
    }

    public void RandomizeAnimal(Animal _animal)
    {
        //Age
        _animal.maxAge = animalMaxAge * UnityEngine.Random.Range(1 - ageVariation, 1 + ageVariation);
        _animal.deathAge = _animal.maxAge; //Even though this is a derived stat that is in UpdateAnimalAll is not set Age cannot be set without death
        if (UnityEngine.Random.value < newSpawnChildChance)
            _animal.adult = false;
        else
        {
            _animal.adult = true;
            _animal.Age = animalAdultAgePercent * _animal.maxAge;
        }

        //Body
        _animal.size = UnityEngine.Random.Range(animalMinSpawnSize, animalMaxSpawnSize);
        _animal.color = Color.HSVToRGB(UnityEngine.Random.Range(0, 1f),
                                       UnityEngine.Random.Range(0, 1f),
                                       UnityEngine.Random.Range(minColorValue, 1f));
        
        _animal.bodyIndex = UnityEngine.Random.Range(0, bodySprites.Length);
        _animal.eyeIndex = UnityEngine.Random.Range(0, eyeSprites.Length);
        _animal.mouthIndex = UnityEngine.Random.Range(0, mouthSprites.Length);

        //Behavior
        _animal.sexualReproduction = true;
        if (UnityEngine.Random.value < chanceAsexual)
            _animal.sexualReproduction = false;
        _animal.diet = RandomAnimalDiet();
        _animal.nature = RandomAnimalNature();
        _animal.moveStyle = (AnimalMoveStyle)UnityEngine.Random.Range(0, Enum.GetValues(typeof(AnimalMoveStyle)).Length);

        //Reproduction
        _animal.reproductionWaitTime = UnityEngine.Random.Range(reproductionAgePercentMin * _animal.maxAge, reproductionAgePercentMax * _animal.maxAge);

        //Abilities
        _animal.sight = UnityEngine.Random.Range(animalsMinSight, animalsMaxSight);
        _animal.attackPower = UnityEngine.Random.Range(animalsAttackMin, animalsAttackMax);
        _animal.hitPointsRegenSpeed = UnityEngine.Random.Range(0, animalsHPRegenMax);

        UpdateAnimal(_animal);
    }

    public Animal AnimalSexualReproduction(Animal _animal1, Animal _animal2)
    {
        Animal _baby = SpawnNewAnimal(_animal1.transform.position);
        float variationMult = 1 - reproductionGeneticVariation;

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

        //Color
        Color.RGBToHSV(_animal1.color, out float H1, out float S1, out float V1);
        Color.RGBToHSV(_animal2.color, out float H2, out float S2, out float V2);

        H1 = (H1 + H2) / 2;
        S1 = (S1 + S2) / 2;
        V1 = (V1 + V2) / 2;

        V1 = UnityEngine.Random.Range(V1 * variationMult, V1 / variationMult);
        if (V1 < 0.7)
            V1 = 0.7f;

        _baby.color = Color.HSVToRGB(UnityEngine.Random.Range(H1 * variationMult, H1 / variationMult),
                                     UnityEngine.Random.Range(S1 * variationMult, S1 / variationMult),
                                     V1);

        if (UnityEngine.Random.value < reproductionGeneticVariation) //Low chance for random body
            _baby.bodyIndex = UnityEngine.Random.Range(0, bodySprites.Length);
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.bodyIndex = _animal1.bodyIndex;
            else
                _baby.bodyIndex = _animal2.bodyIndex;
        }
        if (UnityEngine.Random.value < reproductionGeneticVariation) //Low chance for random eye
            _baby.eyeIndex = UnityEngine.Random.Range(0, eyeSprites.Length);
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.eyeIndex = _animal1.eyeIndex;
            else
                _baby.eyeIndex = _animal2.eyeIndex;
        }
        if (UnityEngine.Random.value < reproductionGeneticVariation) //Low chance for random mouth
            _baby.mouthIndex = UnityEngine.Random.Range(0, mouthSprites.Length);
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.mouthIndex = _animal1.mouthIndex;
            else
                _baby.mouthIndex = _animal2.mouthIndex;
        }

        //Behavior
        if (UnityEngine.Random.value < reproductionDietVariation) //Low chance for different Diet
            _baby.diet = RandomAnimalDiet();
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.diet = _animal1.diet;
            else
                _baby.diet = _animal2.diet;
        }

        if (UnityEngine.Random.value < reproductionBehaviorVariation) //Low chance for different nature
            _baby.nature = RandomAnimalNature();
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.nature = _animal1.nature;
            else
                _baby.nature = _animal2.nature;
        }

        if (UnityEngine.Random.value < reproductionBehaviorVariation) //Low chance for different nature
            _baby.moveStyle = (AnimalMoveStyle)UnityEngine.Random.Range(0, Enum.GetValues(typeof(AnimalMoveStyle)).Length);
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.moveStyle = _animal1.moveStyle;
            else
                _baby.moveStyle = _animal2.moveStyle;
        }

        if (UnityEngine.Random.value < reproductionTypeVariation) //Low chance to become asexual
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

        //Update Baby
        _baby.adult = false;
        UpdateAnimal(_baby);
        _baby.Nutrition = _baby.maxNutrition / 2;
        _baby.parents.Add(_animal1);
        _baby.parents.Add(_animal2);

        //Update Parents
        _animal1.AfterBreed(sexualNutritionLost, sexualDeathAgeLost);
        _animal2.AfterBreed(sexualNutritionLost, sexualDeathAgeLost);

        return _baby;
    }

    public Animal AnimalAsexualReproduction(Animal _animal)
    {
        Animal _baby = SpawnNewAnimal(_animal.transform.position);

        float variationMult = 1 - reproductionGeneticVariation;

        //Info
        _baby.generations = _animal.generations + 1;

        //Max Age
        _baby.maxAge = UnityEngine.Random.Range(_animal.maxAge * variationMult, _animal.maxAge / variationMult);

        //Body
        _baby.size = UnityEngine.Random.Range(_animal.size * variationMult, _animal.size / variationMult);

        //Color
        Color.RGBToHSV(_animal.color, out float H1, out float S1, out float V1);

        V1 = UnityEngine.Random.Range(V1 * variationMult, V1 / variationMult);
        if (V1 < 0.7)
            V1 = 0.7f;

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
        if (UnityEngine.Random.value < reproductionDietVariation) //Low chance for different Diet
            _baby.diet = RandomAnimalDiet();
        else
            _baby.diet = _animal.diet;

        if (UnityEngine.Random.value < reproductionBehaviorVariation) //Low chance for different nature
            _baby.nature = RandomAnimalNature();
        else
        {
            _baby.nature = _animal.nature;
        }

        if (UnityEngine.Random.value < reproductionBehaviorVariation) //Low chance for different nature
            _baby.moveStyle = (AnimalMoveStyle)UnityEngine.Random.Range(0, Enum.GetValues(typeof(AnimalMoveStyle)).Length);
        else
        {
            if (UnityEngine.Random.value > 0.5f)
                _baby.moveStyle = _animal.moveStyle;
            else
                _baby.moveStyle = _animal.moveStyle;
        }

        if (UnityEngine.Random.value < reproductionTypeVariation) //Low chance to become sexual
            _baby.sexualReproduction = true;
        else
            _baby.sexualReproduction = false;

        //Reproduction
        _baby.reproductionWaitTime = UnityEngine.Random.Range(_animal.reproductionWaitTime * variationMult, _animal.reproductionWaitTime / variationMult);

        //Abilities
        _baby.sight = UnityEngine.Random.Range(_animal.sight * variationMult, _animal.sight / variationMult);
        _baby.attackPower = UnityEngine.Random.Range(_animal.attackPower * variationMult, _animal.attackPower / variationMult);
        _baby.hitPointsRegenSpeed = UnityEngine.Random.Range(_animal.hitPointsRegenSpeed * variationMult, _animal.hitPointsRegenSpeed / variationMult);

        //Update Baby
        _baby.adult = false;
        UpdateAnimal(_baby);
        _baby.Nutrition = _baby.maxNutrition / 2;
        _baby.parents.Add(_animal);

        //Update Parent
        _animal.AfterBreed(sexualNutritionLost, asexualDeathAgeLost);

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

        float variationMult = 1 - reproductionGeneticVariation;

        //Info
        _baby.generations = _plant.generations + 1;

        //Max Age
        _baby.maxAge = UnityEngine.Random.Range(_plant.maxAge * variationMult, _plant.maxAge / variationMult);

        //Body
        _baby.size = UnityEngine.Random.Range(_plant.size * variationMult, _plant.size / variationMult);

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
        _baby.reproductionTimerMax = UnityEngine.Random.Range(_plant.reproductionTimerMax * variationMult, _plant.reproductionTimerMax / variationMult);

        //Abilities
        //_baby.sight = UnityEngine.Random.Range(_plant.sight * variationMult, _plant.sight / variationMult);

        //Update Baby
        UpdatePlant(_baby);
        _baby.Nutrition = _baby.maxNutrition * babyPlantStartingNutrition;
        _baby.Resize();

        //Update Parents
        _plant.deathAge -= _plant.maxAge * plantReproductionDeathAgeLost;
        _plant.reproductionTimer = _plant.reproductionTimerMax;

        return _baby;
    }

    public void UpdatePlant(Plant _plant)
    {
        //Age
        _plant.deathAge = _plant.maxAge;

        //Nutrition
        _plant.maxNutrition = plantBaseMaxNutrition * Mathf.Pow(_plant.size, nutritionPower);
        _plant.Nutrition = UnityEngine.Random.Range(_plant.maxNutrition * plantStartingNutritionMin,
                                        _plant.maxNutrition * plantStartingNutritionMax);

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
        _plant.maxHitPoints = Mathf.CeilToInt(plantBaseHP * Mathf.Pow(_plant.size, hpPower));
        _plant.HitPoints = _plant.maxHitPoints;

        _plant.growRate = plantBaseGrowRate * Mathf.Pow(_plant.size, nutritionChangeRatePower);

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

    public void UpdateAnimal(Animal _animal)
    {
        //Updates all animal information based on current variabled and resets to a starting state

        //Age
        _animal.deathAge = _animal.maxAge;
        _animal.adultAge = animalAdultAgePercent * _animal.maxAge;

        //Nutrition
        _animal.maxNutrition = animalsBaseMaxNutrition * Mathf.Pow(_animal.size, nutritionPower);

        //Body
        _animal.transform.localScale = new Vector3(_animal.size, _animal.size, _animal.transform.localScale.z);
        _animal.transform.position = new Vector3(_animal.transform.position.x, _animal.transform.position.y, -(_animal.transform.localScale.y - 1) / 2);
        SetAnimalBody(_animal, _animal.bodyIndex, _animal.color);
        SetAnimalEye(_animal, _animal.eyeIndex);
        SetAnimalMouth(_animal, _animal.mouthIndex);
        _animal.slimeParticles.startColor = _animal.color;

        //Derived Stats
        _animal.maxHitPoints = Mathf.CeilToInt(animalsBaseHP * Mathf.Pow(_animal.size, hpPower));
        _animal.HitPoints = _animal.maxHitPoints;

        _animal.moveSpeed = animalsBaseMoveSpeed * Mathf.Pow(_animal.size, moveSpeedPower);

        //Abilities (increase hungerRate)
        _animal.hungerRate = (animalsBaseHungerRate * Mathf.Pow(_animal.size, nutritionChangeRatePower))
                        * (0.5f * _animal.sight + 0.5f) //Sight has a scaling linear relationship 0.5x + 0.5 (1 Sight = 1 hunger)
                        * (0.1f * _animal.attackPower + 0.8f) //Attack has a scaling linear relationship 0.1x + 0.8 (2 Attack = 1 hunger)
                        * (10f * _animal.hitPointsRegenSpeed + 0.5f); //Attack has a scaling linear relationship 0.1x + 0.8 (2 Attack = 1 hunger)
        
        //Modified based on sight
        _animal.GetComponent<CircleCollider2D>().radius = animalsBaseSightRadius * _animal.sight * _animal.size;

        //Set child stats
        if (!_animal.adult)
        {
            AnimalMultiplyAgeStats(_animal, animalChildStatMult);
            _animal.ResizeChild();
        }

        //Calculated after baby check because it changes maxNutrition
        _animal.Nutrition = UnityEngine.Random.Range(_animal.maxNutrition * animalStartingNutritionMin,
                                 _animal.maxNutrition * animalStartingNutritionMax);

        //Reset control important variables
        _animal.Action = AnimalAction.Idle;
        _animal.ResetReproductionTime();
    }

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
        if (dietValue < carnivoreChance)
            return AnimalDiet.Carnivore;
        else if (dietValue < omnivoreChance + carnivoreChance)
            return AnimalDiet.Omnivore;
        else
            return AnimalDiet.Herbivore;
    }

    private AnimalNature RandomAnimalNature()
    {
        float natureValue = UnityEngine.Random.value;
        if (natureValue < violentChance)
            return AnimalNature.Violent;
        else if (natureValue < fearfulChance + violentChance)
            return AnimalNature.Fearful;
        else
            return AnimalNature.Neutral;
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
    Attack, //Moving to and attacking targetObj
    Run, //Moving away from targetObj
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

public enum AnimalMoveStyle
{
    Normal, //Direct and consistant
    Burst, //Direct but moves in short bursts, waiting inbetween
    Hop //Direct hops in cycles
}

public enum LifeformType
{
    Plant,
    Animal,
    Player
}