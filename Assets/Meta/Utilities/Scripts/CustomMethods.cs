using UnityEngine;

public static class CheckNull
{
    public static bool SingleInstanceNotNull<T>(ref T _argument, bool _spawnIfNull) where T : MonoBehaviour
    {
        if (_argument == null)
        {
            _argument = MonoBehaviour.FindObjectOfType<T>();

            //Still Null
            if (_argument == null)
            {
                return false;
            }
        }

        return true;
    }

    public static bool SingleInstanceNotNull(ref Canvas _canvas, bool _spawnIfNull)
    {
        if (_canvas == null)
        {
            _canvas = MonoBehaviour.FindObjectOfType<Canvas>();

            //Still Null
            if (_canvas == null)
            {
                return false;
            }
        }

        return true;
    }

    public static bool SingleObjectNotNull(ref GameObject _argument, string _objName, bool _spawnIfNull)
    {
        //Checks if game object is null
        //if not do nothing
        //if null then try to find if an object of the given name exists or create a new one and give it the correct parents
        if (_argument == null)
        {
            _argument = GameObject.Find(_objName);

            //Still Null
            if (_argument == null)
            {
                if (!_spawnIfNull)
                    return false;

                GameObject resouce = Resources.Load<GameObject>("Objects/" + _objName);
                if (resouce == null)
                    return false;

                _argument = MonoBehaviour.Instantiate(resouce);

                ParentList parentsListClass = _argument.GetComponent<ParentList>();
                if (parentsListClass == null)
                    return true; //Done and no need to set parents

                string[] parents = parentsListClass.parents;

                GameObject lastParent = null;
                foreach (var parent in parents)
                {
                    GameObject thisParent = GameObject.Find(parent);
                    if (thisParent == null)
                        thisParent = new GameObject(parent);

                    if (lastParent != null)
                        thisParent.transform.SetParent(lastParent.transform);

                    lastParent = thisParent;

                    if (parent == parents[^1]) //End of loop
                    {
                        _argument.transform.SetParent(lastParent.transform);
                    }
                }

                return true;
            }
        }

        return true;
    }
}

public class Movement : MonoBehaviour
{
    public static Quaternion QuaternionAngle2D(Vector3 _pos, Vector3 _targetPos)
    {
        Vector2 direction = Camera.main.ScreenToWorldPoint(_targetPos) - _pos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 45;
        return Quaternion.Euler(new Vector3(0, 0, angle));
    }
}

public class PermanentMonoSingleton<T> : MonoBehaviour where T : Component
{
    //Will generate if referenced and not yet existing
    //Will also delete other copies if multiple exist (Logs error as this should not happen)
    //Loads from a prefab in the resouces folder so all references and varaibles are consistent

    public static bool shutdown = false;

    private static T _instance;
    public static T Instance
    {
        get
        {
            //When this Instance in this class is first called check for other same type instances
            if (_instance == null && !shutdown)
            {
                T[] sameTypeObjects = FindObjectsOfType(typeof(T)) as T[];

                if (sameTypeObjects.Length > 0)
                    _instance = sameTypeObjects[0];

                if (sameTypeObjects.Length > 1)
                {
                    //Potentially auto delete if an issue
                    Debug.LogError(string.Concat("More than 1 singleton instance of type: ",
                                   typeof(T).Name, " : ", sameTypeObjects.Length, " in total."));

                    for (int i = 1; i < sameTypeObjects.Length; i++)
                    {
                        Destroy(sameTypeObjects[i].gameObject);
                    }
                }
                else if (_instance == null)
                {
                    GameObject resource = Resources.Load<GameObject>("Managers/" + typeof(T).Name);

                    if (resource == null)
                    {
                        Debug.LogError(string.Concat("No instance of referenced singleton exists: ",
                                   typeof(T).Name, " : ", " and no prefab exists within resouces to be loaded."));
                        return null;
                    }

                    _instance = Instantiate(resource).GetComponent<T>();
                }
            }

            return _instance;
        }
        private set { _instance = value; }
    }

    protected virtual void Awake()
    {
        //Deletes other singleton copies that may spawn durring the game
        if (_instance != null)
        {
            //If gameObject has the same name as the class then it is assumed the object exists for the class and should be destroyed
            //Cuts off the end of the name because unity adds text for clones and copies
            string className = GetType().Name;
            int nameLength = className.Length;

            if (className.Equals(gameObject.name.Substring(0, nameLength)))
                Destroy(gameObject); //Destroy dedicated gameObject
            else
                Destroy(this); //Only destroy script
        }
        else
            DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnApplicationQuit()
    {
        shutdown = true;
    }
}

public class MonoSingleton<T> : MonoBehaviour where T : Component
{
    //Will delete all other copies

    private static T _instance;
    public static T Instance
    {
        get
        {
            //When this Instance in this class is first called check for other same type instances
            if (_instance == null)
            {
                T[] sameTypeObjects = FindObjectsOfType(typeof(T)) as T[];

                if (sameTypeObjects.Length > 0)
                    _instance = sameTypeObjects[0];

                if (sameTypeObjects.Length > 1)
                {
                    //Potentially auto delete if an issue
                    Debug.LogError(string.Concat("More than 1 singleton instance of type: ",
                                   typeof(T).Name, " : ", sameTypeObjects.Length, " in total."));

                    for (int i = 1; i < sameTypeObjects.Length; i++)
                    {
                        Destroy(sameTypeObjects[i].gameObject);
                    }
                }
            }

            return _instance;
        }
        private set { _instance = value; }
    }

    protected virtual void Awake()
    {
        //Deletes other singleton copies that may spawn durring the game
        if (_instance != null)
        {
            //If gameObject has the same name as the class then it is assumed the object exists for the class and should be destroyed
            //Cuts off the end of the name because unity addes text for clones and copies
            string className = GetType().Name;
            int nameLength = className.Length;

            if (className.Equals(gameObject.name.Substring(0, nameLength)))
                Destroy(gameObject); //Destroy dedicated gameObject
            else
                Destroy(this); //Only destroy script
        }
    }
}
