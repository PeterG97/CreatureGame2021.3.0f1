/* A list that is used by my SingleObjectNotNull custom method that tells it
 * how to the order of parents for a newly spawned object in a scene
 * Example: [0]Lifeforms,[1]Animals would spawn an object with a parent Animals
 * object and Animals would have the parent Lifeforms.
 * 
 * Dependent on no classes */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentList : MonoBehaviour
{
    [SerializeField] public string[] parents;
}
