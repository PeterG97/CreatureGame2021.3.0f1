using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SimulatedLifeform : Lifeform
{
    #region ---=== Serialized Variables ===---
    [Header("Info")]
    [SerializeField] public int generations;
    [Header("Basic Stats")]
    [SerializeField] public float size;
    [SerializeField] public Color color;
    [SerializeField] protected float age;
    [SerializeField] public float maxAge;
    [SerializeField] public float deathAge;
    [SerializeField] public float maxNutrition;
    [SerializeField] protected float nutrition;
    [SerializeField] public bool dead = false;
    #endregion

    #region ---=== Nonserialized Variables ===---

    #endregion

    #region ---=== Get/Set Variables ===---
    public virtual float Age
    {
        get { return age; }
        set
        {
            age = value;

            if (age >= deathAge)
            {
                Die();
            }
        }
    }
    public virtual float Nutrition
    {
        get { return nutrition; }
        set
        {
            nutrition = value;

            if (nutrition <= 0)
            {
                Die();
            }
            else if (nutrition > maxNutrition)
            {
                nutrition = maxNutrition;
            }
        }
    }
    #endregion

    protected virtual void FixedUpdate()
    {
        Age += Time.deltaTime;
    }
}
