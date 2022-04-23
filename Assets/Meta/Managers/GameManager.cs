using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

#region ---=== TODO ===---
/*
 * Adjust depth sorting for outline (either add basic offset to help a little or track line width in script)
 * Add movement behaviors
 * Add social behavior (follower, protective, antisocial)
 * Hover Animal Menu
 * Tile map grass, sand, water
 * Get PermanentMonSingletons to load from prefab
 * Plant wind shader stutter
 * Research best practice universal input system
 * UI Overhaul (Maybe new ui system)
 * --- Quality Programming ---
 * Seperate Variables into serialized and nonserialized
 */
#endregion

public class GameManager : PermanentMonoSingleton<GameManager>
{
    /* Dependent on no Classes */

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

    #region ---=== Serialized Variables ===---
    [SerializeField] public int targetFPS = 120;
    [SerializeField] public float gameCellSize = 1;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        LoadMainParticleSystems();
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        LoadMainParticleSystems();
        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        Application.targetFrameRate = targetFPS;
    }

    private void OnEnable()
    {
        
    }

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
