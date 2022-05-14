using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Lifeform Values", menuName = "Global Values/Lifeform Values")]
public class LifeformValues : ScriptableObject
{
    #region ---=== Variables ===---
    [Header("Timer")]
    [Tooltip("If the simulation should start with plants spawning automatically (Default true)")]
    [SerializeField] private bool spawnNewPlantByTimer = true;
    public bool SpawnNewPlantByTimer { get => spawnNewPlantByTimer; set => spawnNewPlantByTimer = value; }
    [Tooltip("Time to spawn new random plant (Default 10)")]
    [SerializeField] private float plantSpawnTime = 10f;
    public float PlantSpawnTime { get => plantSpawnTime; set => plantSpawnTime = value; }
    [Tooltip("If the simulation should start with animals spawning automatically (Default true)")]
    [SerializeField] private bool spawnNewAnimalByTimer = true;
    public bool SpawnNewAnimalByTimer { get => spawnNewAnimalByTimer; set => spawnNewAnimalByTimer = value; }
    [Tooltip("Time to spawn new random plant (Default 60)")]
    [SerializeField] private float animalSpawnTime = 60f;
    public float AnimalSpawnTime { get => animalSpawnTime; set => animalSpawnTime = value; }

    [Header("Color")]
    [Tooltip("The value in HSV color format [Higher == brighter colors] (Default 0.7)")]
    [SerializeField] private float minColorValue = 0.7f;
    public float MinColorValue { get => minColorValue; set => minColorValue = value; }

    [Header("Age")]
    [Tooltip("Second until a old age death (Default 180)")]
    [SerializeField] private float plantMaxAge = 180f;
    public float PlantMaxAge { get => plantMaxAge; set => plantMaxAge = value; }
    [Tooltip("Second until a old age death [Will be reduced in game from certain actions such as fighting or breeding] (Default 600)")]
    [SerializeField] private float animalMaxAge = 600f;
    public float AnimalMaxAge { get => animalMaxAge; set => animalMaxAge = value; }
    [Tooltip("Percent random age variation (Default 0.1)")]
    [SerializeField] private float ageVariation = 0.1f;
    public float AgeVariation { get => ageVariation; set => ageVariation = value; }
    [Tooltip("Percent of maxAge when animal become adult (Default 0.25)")]
    [SerializeField] private float animalAdultAgePercent = 0.25f;
    public float AnimalAdultAgePercent { get => animalAdultAgePercent; set => animalAdultAgePercent = value; }
    [Tooltip("The chance for a child animal to appear when an animal is spawned (Default 0.25)")]
    [SerializeField] private float newSpawnChildChance = 0.25f;
    public float NewSpawnChildChance { get => newSpawnChildChance; set => newSpawnChildChance = value; }
    [Tooltip("What relavent child stats will be multiplied by (Default 0.5)")]
    [SerializeField] private float animalChildStatMult = 0.5f;
    public float AnimalChildStatMult { get => animalChildStatMult; set => animalChildStatMult = value; }


    [Header("Size")]
    [Tooltip("Minimum plant spawn scale (Default 0.75)")]
    [SerializeField] private float plantMinSpawnSize = 0.75f;
    public float PlantMinSpawnSize { get => plantMinSpawnSize; set => plantMinSpawnSize = value; }
    [Tooltip("Maximum plant spawn scale (Default 1.25)")]
    [SerializeField] private float plantMaxSpawnSize = 1.25f;
    public float PlantMaxSpawnSize { get => plantMaxSpawnSize; set => plantMaxSpawnSize = value; }
    [Tooltip("Minimum animal spawn scale (Default 0.75)")]
    [SerializeField] private float animalMinSpawnSize = 0.75f;
    public float AnimalMinSpawnSize { get => animalMinSpawnSize; set => animalMinSpawnSize = value; }
    [Tooltip("Maximum animal spawn scale (Default 1.25)")]
    [SerializeField] private float animalMaxSpawnSize = 1.25f;
    public float AnimalMaxSpawnSize { get => animalMaxSpawnSize; set => animalMaxSpawnSize = value; }
    [Tooltip("Minimum plant scale (Default 0.5)")]
    [SerializeField] private float plantMinSize = 0.5f;
    public float PlantMinSize { get => plantMinSize; set => plantMinSize = value; }
    [Tooltip("Maximum plant scale (Default 1.5)")]
    [SerializeField] private float plantMaxSize = 1.5f;
    public float PlantMaxSize { get => plantMaxSize; set => plantMaxSize = value; }
    [Tooltip("Minimum animal scale (Default 0.25)")]
    [SerializeField] private float animalMinSize = 0.25f;
    public float AnimalMinSize { get => animalMinSize; set => animalMinSize = value; }
    [Tooltip("Maximum animal scale (Default 2)")]
    [SerializeField] private float animalMaxSize = 3f;
    public float AnimalMaxSize { get => animalMaxSize; set => animalMaxSize = value; }


    [Header("HP")]
    [SerializeField] private float plantBaseHP = 5f;
    public float PlantBaseHP { get => plantBaseHP; set => plantBaseHP = value; }
    [SerializeField] private float animalsBaseHP = 10f;
    public float AnimalsBaseHP { get => animalsBaseHP; set => animalsBaseHP = value; }
    [Tooltip("Power to scale hp based on body size (Default 2)")]
    [SerializeField] private float hpPower = 2f;
    public float HpPower { get => hpPower; set => hpPower = value; }


    [Header("Movespeed")]
    [SerializeField] private float animalsBaseMoveSpeed = 40f;
    public float AnimalsBaseMoveSpeed { get => animalsBaseMoveSpeed; set => animalsBaseMoveSpeed = value; }
    [Tooltip("Power to inversely scale move speed based on body size (Default -1)")]
    [SerializeField] private float moveSpeedPower = -1f;
    public float MoveSpeedPower { get => moveSpeedPower; set => moveSpeedPower = value; }


    [Header("Plant Growth")]
    //Plant nutrition counts up to a max value
    [Tooltip("Essentially max growth (Default 6000)")]
    [SerializeField] private float plantBaseMaxNutrition = 6000f; //Min nutrition is 0
    public float PlantBaseMaxNutrition { get => plantBaseMaxNutrition; set => plantBaseMaxNutrition = value; }
    [Tooltip("Gain of nutrition per fixedUpdate (Default 2)")]
    [SerializeField] private float plantBaseGrowRate = 2f;
    public float PlantBaseGrowRate { get => plantBaseGrowRate; set => plantBaseGrowRate = value; }
    [Tooltip("Low end of new plant's nutrition (Default 0.1)")]
    [SerializeField] private float plantStartingNutritionMin = 0.2f;
    public float PlantStartingNutritionMin { get => plantStartingNutritionMin; set => plantStartingNutritionMin = value; }
    [Tooltip("High end of new plant's nutrition (Default 0.6)")]
    [SerializeField] private float plantStartingNutritionMax = 1f;
    public float PlantStartingNutritionMax { get => plantStartingNutritionMax; set => plantStartingNutritionMax = value; }
    [Tooltip("Baby plant's starting nutrition (Default 0.6)")]
    [SerializeField] private float babyPlantStartingNutrition = 0.01f;
    public float BabyPlantStartingNutrition { get => babyPlantStartingNutrition; set => babyPlantStartingNutrition = value; }


    [Header("Animal Hunger")]
    //Animal nutrition counts down to 0
    [Tooltip("Essentially max fullness, maximum fixed update frames to die if animalsBaseHungerRate is 1 (Default 10000)")]
    [SerializeField] private float animalsBaseMaxNutrition = 10000f; //Min nutrition is 0
    public float AnimalsBaseMaxNutrition { get => animalsBaseMaxNutrition; set => animalsBaseMaxNutrition = value; }
    [Tooltip("Loss of nutrition per fixedUpdate (Default 1)")]
    [SerializeField] private float animalsBaseHungerRate = 1f;
    public float AnimalsBaseHungerRate { get => animalsBaseHungerRate; set => animalsBaseHungerRate = value; }
    [Tooltip("Low end of new animal's nutrition (Default 0.1)")]
    [SerializeField] private float animalStartingNutritionMin = 0.6f;
    public float AnimalStartingNutritionMin { get => animalStartingNutritionMin; set => animalStartingNutritionMin = value; }
    [Tooltip("High end of new animal's nutrition (Default 0.6)")]
    [SerializeField] private float animalStartingNutritionMax = 1f;
    public float AnimalStartingNutritionMax { get => animalStartingNutritionMax; set => animalStartingNutritionMax = value; }


    [Header("Nutrition")]
    [Tooltip("Power to scale hungerRate based on body size (Default 2)")]
    [SerializeField] private float nutritionPower = 2f;
    public float NutritionPower { get => nutritionPower; set => nutritionPower = value; }
    [Tooltip("Power to scale nutrtion change based on size (Default 2)")]
    [SerializeField] private float nutritionChangeRatePower = 2f;
    public float NutritionChangeRatePower { get => nutritionChangeRatePower; set => nutritionChangeRatePower = value; }


    [Header("Animal Behavior")]
    [Tooltip("Percent of random spawns that will be carnivores [remainder will be herbivores] (Default 0.05)")]
    [SerializeField] private float carnivoreChance = 0.05f;
    public float CarnivoreChance { get => carnivoreChance; set => carnivoreChance = value; }
    [Tooltip("Percent of random spawns that will be omnivores [remainder will be herbivores] (Default 0.15)")]
    [SerializeField] private float omnivoreChance = 0.15f;
    public float OmnivoreChance { get => omnivoreChance; set => omnivoreChance = value; }
    [Tooltip("Percent of random spawns that will be nature violent [remainder will be neutral] (Default 0.05)")]
    [SerializeField] private float violentChance = 0.05f;
    public float ViolentChance { get => violentChance; set => violentChance = value; }
    [Tooltip("Percent of random spawns that will be nature fearful [remainder will be fearful] (Default 0.3)")]
    [SerializeField] private float fearfulChance = 0.3f;
    public float FearfulChance { get => fearfulChance; set => fearfulChance = value; }
    [Tooltip("Percent of random spawns that will be a social follower [remainder will be neutral] (Default 0.2)")]
    [SerializeField] private float followerChance = 0.2f;
    public float FollowerChance { get => followerChance; set => followerChance = value; }
    [Tooltip("Percent of random spawns that will be a social protective [remainder will be neutral] (Default 0.2)")]
    [SerializeField] private float protectiveChance = 0.2f;
    public float ProtectiveChance { get => protectiveChance; set => protectiveChance = value; }
    [Tooltip("Percent of random spawns that will be a social antisocial [remainder will be neutral] (Default 0.05)")]
    [SerializeField] private float antisocialChance = 0.05f;
    public float AntisocialChance { get => antisocialChance; set => antisocialChance = value; }


    [Header("Animal Abilities")]
    [Tooltip("Min random sight (Default 0.5)")]
    [SerializeField] private float animalsMinSight = 0.5f;
    public float AnimalsMinSight { get => animalsMinSight; set => animalsMinSight = value; }
    [Tooltip("Max random sight (Default 1)")]
    [SerializeField] private float animalsMaxSight = 1.5f;
    public float AnimalsMaxSight { get => animalsMaxSight; set => animalsMaxSight = value; }
    [Tooltip("Size of vision circle collider 2D (Default 3)")]
    [SerializeField] private float animalsBaseSightRadius = 3f;
    public float AnimalsBaseSightRadius { get => animalsBaseSightRadius; set => animalsBaseSightRadius = value; }
    [Tooltip("Attack stat min (Default 1)")]
    [SerializeField] private float animalsAttackMin = 1f;
    public float AnimalsAttackMin { get => animalsAttackMin; set => animalsAttackMin = value; }
    [Tooltip("Attack stat max (Default 4)")]
    [SerializeField] private float animalsAttackMax = 4f;
    public float AnimalsAttackMax { get => animalsAttackMax; set => animalsAttackMax = value; }
    [Tooltip("Max HP regained every fixed update frame (Default 0.02)")]
    [SerializeField] private float animalsHPRegenMax = 0.02f;
    public float AnimalsHPRegenMax { get => animalsHPRegenMax; set => animalsHPRegenMax = value; }


    [Header("Reproduction")]
    [Tooltip("Percent variation below and above the average of the parents stats (Default 0.05)")]
    [SerializeField] private float reproductionGeneticVariation = 0.1f;
    public float ReproductionGeneticVariation { get => reproductionGeneticVariation; set => reproductionGeneticVariation = value; }


    [Header("Plant Reproduction")]
    [Tooltip("The age which an asexual animal will die is reduced by a percentage of the maxAge (0.05)")]
    [SerializeField] private float plantReproductionDeathAgeLost = 0.05f;
    public float PlantReproductionDeathAgeLost { get => plantReproductionDeathAgeLost; set => plantReproductionDeathAgeLost = value; }
    [Tooltip("The min amount of a lifeform's age that must pass before they can reporduce again (0.1)")]
    [SerializeField] private float plantReproductionTimePercentMin = 0.1f;
    public float PlantReproductionTimePercentMin { get => plantReproductionTimePercentMin; set => plantReproductionTimePercentMin = value; }
    [Tooltip("The min amount of an lifeform's age that must pass before they can reporduce again (0.2)")]
    [SerializeField] private float plantReproductionTimePercentMax = 0.2f;
    public float PlantReproductionTimePercentMax { get => plantReproductionTimePercentMax; set => plantReproductionTimePercentMax = value; }


    [Header("Animal Reproduction")]
    [Tooltip("The chance for a random animal to be asexual (Default 0.1)")]
    [SerializeField] private float chanceAsexual = 0.1f;
    public float ChanceAsexual { get => chanceAsexual; set => chanceAsexual = value; }
    [Tooltip("Chance that reproduction behavior will be changed to something random (Default 0.1)")]
    [SerializeField] private float reproductionBehaviorVariation = 0.10f;
    public float ReproductionBehaviorVariation { get => reproductionBehaviorVariation; set => reproductionBehaviorVariation = value; }
    [Tooltip("Chance that diet behavior will be changed to something random (Default 0.03)")]
    [SerializeField] private float reproductionDietVariation = 0.03f;
    public float ReproductionDietVariation { get => reproductionDietVariation; set => reproductionDietVariation = value; }
    [Tooltip("Chance that reproduction type will be changed to the opposite type (0.02)")]
    [SerializeField] private float reproductionTypeVariation = 0.02f;
    public float ReproductionTypeVariation { get => reproductionTypeVariation; set => reproductionTypeVariation = value; }
    [Tooltip("Nutrition percent of total lost by each parent when a baby is made (0.2)")]
    [SerializeField] private float sexualNutritionLost = 0.2f;
    public float SexualNutritionLost { get => sexualNutritionLost; set => sexualNutritionLost = value; }
    [Tooltip("Nutrition percent of total lost by the parent when a baby is made (0.6)")]
    [SerializeField] private float asexualNutritionLost = 0.6f;
    public float AsexualNutritionLost { get => asexualNutritionLost; set => asexualNutritionLost = value; }
    [Tooltip("The age which a sexual animal will die is reduced by a percentage of the maxAge (0.2)")]
    [SerializeField] private float sexualDeathAgeLost = 0.2f;
    public float SexualDeathAgeLost { get => sexualDeathAgeLost; set => sexualDeathAgeLost = value; }
    [Tooltip("The age which an asexual animal will die is reduced by a percentage of the maxAge (0.4)")]
    [SerializeField] private float asexualDeathAgeLost = 0.4f;
    public float AsexualDeathAgeLost { get => asexualDeathAgeLost; set => asexualDeathAgeLost = value; }
    [Tooltip("The base amount of a lifeform's age that must pass before they can reporduce again (0.05)")]
    [SerializeField] private float animalReproductionTimePercent = 0.1f;
    public float AnimalReproductionTimePercent { get => animalReproductionTimePercent; set => animalReproductionTimePercent = value; }
    [Tooltip("The min drawbacks to reproduction [Time to reproduce, nutrition loss, deathAge loss] (0.75)")]
    [SerializeField] private float animalReproductionNegativesMin = 0.5f;
    public float AnimalReproductionNegativesMin { get => animalReproductionNegativesMin; set => animalReproductionNegativesMin = value; }
    [Tooltip("The min drawbacks to reproduction [Time to reproduce, nutrition loss, deathAge loss] (0.75)")]
    [SerializeField] private float animalReproductionNegativesMax = 1.5f;
    public float AnimalReproductionNegativesMax { get => animalReproductionNegativesMax; set => animalReproductionNegativesMax = value; }
    #endregion
}
