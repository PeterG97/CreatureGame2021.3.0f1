/* Contains individualized plant data and basic functions such as growth, decaying, and death.
 * Plants increase their nutrition every FixedUpdate based on their growth rate and only die if
 * an animal eats all of their nutrition at once, their HP drops to <= 0, or from old age. They
 * also attempt to reproduce on a timer but if no open spaces are around them they fail.
 * 
 * Dependent on classes:
 * PermanentMonoSingleton - GameManager
 * MonoSingleton - LifeformManager */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : SimulatedLifeform
{
    #region ---=== Serialized Variables ===---
    [Header("Body")]
    [SerializeField] public int plantIndex = -1;
    [SerializeField] public Vector2Int gridLocation;
    [Header("Growth")]
    [SerializeField] public float growRate = 1f; //Will decrease with better abilities
    [Header("Derived Abilities")]
    [NonSerialized] public float reproductionWaitTime = 10f; //Time in between breeding
    [Header("Abilities")]
    [Header("Reproduction")]
    [SerializeField] public float reproduceThreshold = 0.75f;

    [Header("Timers")]
    [SerializeField] private float growthUpdateTimer = 0.25f;
    [SerializeField] public float reproductionTimer = 1f;
    #endregion

    #region ---=== Nonserialized Variables ===---
    [NonSerialized] public SpriteRenderer sprite;

    [NonSerialized] private float sortZOffset = 0.2f; //TODO temp fix

    [Header("Timers")]
    [NonSerialized] private float growthUpdateTimerMax = 1f;
    #endregion

    #region ---=== Get/Set Variables ===---

    #endregion

    private void Awake()
    {
        sprite = transform.GetChild(0).GetComponentInChildren<SpriteRenderer>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (dead)
        {
            Decaying();
            return;
        }

        Nutrition += growRate;

        growthUpdateTimer -= Time.deltaTime;
        if (growthUpdateTimer <= 0)
        {
            Resize();
        }

        reproductionTimer -= Time.deltaTime;
        if (reproductionTimer <= 0 && Nutrition / maxNutrition > reproduceThreshold)
        {
            if (LifeformManager.Instance.PlantReproduction(this) == null)
                reproductionTimer = reproductionWaitTime; //Failed wait to try again
        }
    }

    private void OnDestroy()
    {
        //Normally plants decrement this counter when they enter the dead state so this is for when they are destroyed before death
        if (!dead)
        {
            if (LifeformManager.Instance != null)
                LifeformManager.Instance.PlantPopulation--;
            if (MapManager.Instance != null)
                MapManager.Instance.generalGrid.SetValue(gridLocation, 0);
        }
    }

    public void Resize()
    {
        growthUpdateTimer = growthUpdateTimerMax;

        float remainingSize = Nutrition / maxNutrition;
        if (remainingSize < 0.15f)
            remainingSize = 0.15f;
        transform.localScale = new Vector3(size * remainingSize, size * remainingSize, transform.localScale.z);

        //Fixes YZ depth sorting for scaled objects
        transform.position = new Vector3(transform.position.x, transform.position.y, -(transform.localScale.y - 1)/2 + sortZOffset);
    }

    public void Randomize()
    {
        if (!dead)
            LifeformManager.Instance.RandomizePlant(this);
    }

    public void AfterReproduce(float _deathAgeLost)
    {
        //Stats lost
        deathAge -= maxAge * _deathAgeLost;

        //Timer
        reproductionTimer = reproductionWaitTime;
    }

    public override void Die()
    {
        if (dead)
            return;

        dead = true;
        Resize();

        if (LifeformManager.Instance != null)
            LifeformManager.Instance.PlantPopulation--;
        if (MapManager.Instance != null)
            MapManager.Instance.generalGrid.SetValue(gridLocation, 0);
    }

    public void Decaying()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.01f);
        sprite.color = Color.Lerp(sprite.color, Color.black, 0.01f);
        nutrition -= maxNutrition * 0.01f;
        nutrition = Mathf.Clamp(nutrition, 1, maxNutrition);

        if (transform.localScale.x < 0.05f)
        {
            Destroy(gameObject);
        }
    }
}
