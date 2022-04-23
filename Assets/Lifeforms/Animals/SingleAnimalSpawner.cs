using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleAnimalSpawner : MonoBehaviour
{
    Animal myAnimal;

    private void Update()
    {
        if (myAnimal == null)
        {
            myAnimal = LifeformManager.Instance.SpawnNewAnimal(transform.position);
            myAnimal.Randomize();
        }
    }
}