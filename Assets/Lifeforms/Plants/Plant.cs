using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Plant : SimulatedLifeform
{
    /* Dependent on Classes:
     * PermanentMonoSingleton - GameManager
     * PermanentMonoSingleton - LifeformManager */

    #region ---=== Auto Assigned/Constant Variables ===---

    #endregion

    #region ---=== Data Variables ===---
    [Header("Body")]
    [SerializeField] public int plantIndex = -1;
    [SerializeField] public Vector2Int gridLocation;
    [Header("Growth")]
    [SerializeField] public float growRate = 1f; //Will decrease with better abilities
    //[Header("Abilities")]
    [Header("Reproduction")]
    [SerializeField] public float reproduceThreshold = 0.75f;
    #endregion

    #region ---=== Control Variables ===---
    [NonSerialized] private float sortZOffset = 0.2f; //TODO temp fix
    [NonSerialized] private float growthUpdateTimerMax = 1f;
    [SerializeField] private float growthUpdateTimer = 0.25f;
    [NonSerialized] public float reproductionTimerMax = 10f; //Time in between breeding
    [SerializeField] public float reproductionTimer = 1f;
    #endregion


    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        Nutrition += growRate; //Will automatically decrement DecomposeTimer when dead

        growthUpdateTimer -= Time.deltaTime;
        if (growthUpdateTimer <= 0)
        {
            Resize();
        }

        reproductionTimer -= Time.deltaTime;
        if (reproductionTimer <= 0 && Nutrition / maxNutrition > reproduceThreshold)
        {
            if (LifeformManager.Instance.PlantReproduction(this) == null)
                reproductionTimer = reproductionTimerMax; //Failed wait to try again
        }
    }

    private void OnDestroy()
    {
        LifeformManager.Instance.PlantPopulation--;
        if (MapManager.Instance != null)
            MapManager.Instance.generalGrid.SetValue(gridLocation, 0);
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
        LifeformManager.Instance.RandomizePlant(this);
    }
}
