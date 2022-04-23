using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlantSpawner : MonoBehaviour
{
    Plant myPlant;

    private void Update()
    {
        if (myPlant == null)
        {
            myPlant = LifeformManager.Instance.SpawnNewPlant(transform.position);
            myPlant.Randomize();
            myPlant.Nutrition = myPlant.maxNutrition * 0.75f;
            myPlant.Resize();
        }
    }
}