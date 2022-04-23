using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifeform : MonoBehaviour
{
    #region ---=== Auto Assigned/Constant Variables ===---

    #endregion

    #region ---=== Data Variables ===---
    //Active Stats
    [Header("Lifeform Variables")]
    [SerializeField] private float hitPoints = 1;
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
    //Base Stats
    [SerializeField] public float maxHitPoints = 1;
    #endregion

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
