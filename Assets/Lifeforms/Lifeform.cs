using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifeform : MonoBehaviour
{
    #region ---=== Serialized Variables ===---
    [Header("Lifeform Variables")]
    [SerializeField] private float hitPoints = 1;
    [SerializeField] public float maxHitPoints = 1;
    #endregion

    #region ---=== Nonserialized Variables ===---

    #endregion

    #region ---=== Get/Set Variables ===---
    public float HitPoints
    {
        get
        {
            return hitPoints;
        }
        set
        {
            hitPoints = value;

            if (hitPoints <= 0)
            {
                Die();
            }
        }
    }
    #endregion

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
