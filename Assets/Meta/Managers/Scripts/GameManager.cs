/* This class is meant to always exist and if it doesn't an instance of it will be instantiated
 * because of the behavior inherited from the PermanentMonoSingleton parent class (Found in my CustomMethods).
 * It is meant to handle fundamental functions such as saving and loading to files (not implemented).
 * Instances of this class persist through scenes. Allowing information to be transferred through
 * scenes with this class.
 * 
 * Dependent on no classes */

#region ---=== TODO ===---
/* ---Main TODO---
 * Plant abilities
 * Fix plant shader jitter
 * Try to move some of the animal specific logic to the animal class
 * 
 * ---Performance---
 * Replace FindObjectsOfType with FindObjectsWithTag performs way better
 * Implement StringBuilder performs better
 * Foreach for loops that access index of array more than once
 * Improve physics performance
 * 
 * ---Feature creep---
 * Add attack animation between fighting animals for clarity
 * Tile map grass, sand, water
 * Research best practice universal input system
 * UI Overhaul (Maybe new ui system)
 * Add sound
 * Implement better age & reproduction timer spawing (some animals spawn near death)
 * 
 * ---Small Things---
 * Camera breaks when below min orth size either restrict min grid x & y size or fix it
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PermanentMonoSingleton<GameManager>
{
    #region ---=== Serialized Variables ===---
    //Reference to scriptable object which will be copied for runtime instance
    [SerializeField] private GameValues gameValuesBase;
    #endregion

    #region ---=== Nonserialized Variables ===---
    //Central object references
    [NonSerialized] public Player player;
    //Particles
    [NonSerialized] public ParticleSystem stunParticleSystem;
    [NonSerialized] public ParticleSystem plantHitParticleSystem;
    [NonSerialized] public ParticleSystem animalHitParticleSystem;
    [NonSerialized] public ParticleSystem animalDeathParticleSystem;
    [NonSerialized] public ParticleSystem animalGrowUpParticleSystem;
    [NonSerialized] public ParticleSystem animalReproduceParticleSystem;
    #endregion

    #region ---=== Get/Set Variables ===---
    [NonSerialized] private GameValues gameValues;
    public GameValues GameValues
    {
        get
        {
            if (gameValues == null)
                gameValues = Instantiate(gameValuesBase);

            return gameValues;
        }
        private set { } //Never direcly set
    }
    #endregion


    #region ---=== Base Methods ===---
    protected override void Awake()
    {
        base.Awake();

        LoadMainParticleSystems();
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        LoadMainParticleSystems();
        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        Application.targetFrameRate = GameValues.TargetFPS;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion


    #region ---=== Particle Effect Systems ===---
    private void LoadMainParticleSystems()//GetComponent<ParticleSystem>()
    {
        GameObject parent = GameObject.Find("Particles");
        if (parent == null)
            parent = new GameObject("Particles");

        stunParticleSystem = Instantiate(Resources.Load<GameObject>("Particles/StunParticleSystem")).GetComponent<ParticleSystem>();
        stunParticleSystem.transform.parent = parent.transform;
        plantHitParticleSystem = Instantiate(Resources.Load<GameObject>("Particles/PlantHitParticleSystem")).GetComponent<ParticleSystem>();
        plantHitParticleSystem.transform.parent = parent.transform;
        animalHitParticleSystem = Instantiate(Resources.Load<GameObject>("Particles/AnimalHitParticleSystem")).GetComponent<ParticleSystem>();
        animalHitParticleSystem.transform.parent = parent.transform;
        animalDeathParticleSystem = Instantiate(Resources.Load<GameObject>("Particles/AnimalDeathParticleSystem")).GetComponent<ParticleSystem>();
        animalDeathParticleSystem.transform.parent = parent.transform;
        animalGrowUpParticleSystem = Instantiate(Resources.Load<GameObject>("Particles/AnimalGrowUpParticleSystem")).GetComponent<ParticleSystem>();
        animalGrowUpParticleSystem.transform.parent = parent.transform;
        animalReproduceParticleSystem = Instantiate(Resources.Load<GameObject>("Particles/AnimalReproduceParticleSystem")).GetComponent<ParticleSystem>();
        animalReproduceParticleSystem.transform.parent = parent.transform;
    }

    public void PlayStunParticle(Vector2 _loction, float _scale)
    {
        if (stunParticleSystem != null)
        {
            stunParticleSystem.transform.position = _loction;
            stunParticleSystem.Play();
        }
    }

    public void PlayPlantHitParticle(Vector2 _loction, float _scale)
    {
        if (plantHitParticleSystem != null)
        {
            plantHitParticleSystem.transform.position = _loction;
            plantHitParticleSystem.Play();
        }
    }

    public void PlayAnimalHitParticle(Vector2 _loction, float _scale)
    {
        if (animalHitParticleSystem != null)
        {
            animalHitParticleSystem.transform.position = _loction;
            animalHitParticleSystem.Play();
        }
    }

    public void PlayAnimalDeathParticle(Vector2 _loction, float _scale)
    {
        if (animalDeathParticleSystem != null)
        {
            animalDeathParticleSystem.transform.position = _loction;
            animalDeathParticleSystem.Play();
        }
    }

    public void PlayAnimalGrowUpParticle(Vector2 _loction, float _scale)
    {
        if (animalGrowUpParticleSystem != null)
        {
            animalGrowUpParticleSystem.transform.position = _loction;
            animalGrowUpParticleSystem.Play();
        }
    }

    public void PlayAnimalReproduceParticle(Vector2 _loction, float _scale)
    {
        if (animalReproduceParticleSystem != null)
        {
            animalReproduceParticleSystem.transform.position = _loction;
            animalReproduceParticleSystem.Play();
        }
    }
    #endregion
}
