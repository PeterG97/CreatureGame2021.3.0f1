using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SimulatedLifeform : Lifeform
{
    [Header("Info")]
    [SerializeField] public float generations = 0f;
    [Header("Basic Stats")]
    [SerializeField] public float size = 1;
    [SerializeField] public Color color = Color.white;
    [SerializeField] public float maxNutrition = 10000f;
    [SerializeField] protected float nutrition = 10000f;
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
    [Tooltip("Second until a old age death (Default 600)")]
    [SerializeField] public float maxAge = 5f;
    [SerializeField] public float deathAge = 5f;
    [SerializeField] protected float age = 0;
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

    protected virtual void FixedUpdate()
    {
        Age += Time.deltaTime;
    }
}
