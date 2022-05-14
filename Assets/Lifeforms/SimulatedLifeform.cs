/* A class that is inherited by both plants and animals and contains variables and function
 * that both use such as age and size. This class is meant for all life forms that have a life
 * and death cycle based on age as well as customizable traits such size and color.
 * 
 * Dependent on no classes */

using System;
using UnityEngine;

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
