using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Animal : SimulatedLifeform
{
    /* Dependent on Classes:
     * PermanentMonoSingleton - GameManager
     * PermanentMonoSingleton - LifeformManager */

    #region ---=== Serialized Variables ===---
    [Header("Animal Attributes")]
    [Header("Body")]
    [SerializeField] public int bodyIndex = 0;
    [SerializeField] public int eyeIndex = 0;
    [SerializeField] public int mouthIndex = 0;
    [Header("Age")]
    [SerializeField] public bool adult = true;
    [SerializeField] public float adultAge = 0f;
    [Header("Nutrition")]
    [SerializeField] public float hungerRate = 1f;
    [SerializeField] public float hungerHuntThreshold = 0.5f;
    [SerializeField] public float hungerEatThreshold = 0.75f;
    [SerializeField] public float hpRunThreshold = 0.5f;
    [Header("Behavior")]
    [SerializeField] public AnimalDiet diet;
    [SerializeField] public AnimalNature nature;
    [SerializeField] public AnimalMoveStyle moveStyle;
    [Header("Derived Abilities")]
    [SerializeField] public float moveSpeed = 1;
    [Header("Advantage Abilities")]
    [SerializeField] public float sight = 1f;
    [SerializeField] public float attackPower = 1f;
    [SerializeField] public float hitPointsRegenSpeed = 0f;
    [SerializeField] public float reproductionWaitTime = 10f; //TODO not fully impemented as an ability
    [Header("Reproduction")]
    [SerializeField] public bool sexualReproduction = true;
    [SerializeField] public List<Animal> parents = new List<Animal>();

    [Header("Control Variables")]
    [SerializeField] public bool dead = false;
    [SerializeField] private AnimalAction action;
    [SerializeField] public LifeformObject target; //Context depends on state
    [SerializeField] public Vector2 wanderPos; //Only used for idle wander
    [SerializeField] private List<GameObject> detectionList = new List<GameObject>(); //All objects seen since start of detection cooldown

    [Header("Timers")] //All count down to 0 or less which permits some action
    [SerializeField] private float wanderTimer; //How long an idle animal will walk in one direction
    [SerializeField] private float detectionTimer ; //Time in between checking for new attackTarget
    [SerializeField] private float timeOutTimer = 10f; //Stops action if its taking more than this time
    [SerializeField] private float actionTimer; //Time until able to perform direct interactions such as attacking or eating
    [SerializeField] private float resizeTimer; //Time until updating child and corpse scale (every update disables physics so infrequent is better)
    [SerializeField] private float reproductionTimer; //Time until able to breed
    [SerializeField] private float stunTimer; //Time until able to move again
    [SerializeField] private float stunEffectTimer; //Time until a new particle is spawned
    [SerializeField] private float decomposeTimer = 1f; //Time until an animal corpse disappears
    #endregion

    #region ---=== Nonserialized Variables ===---
    //Own Components
    [NonSerialized] public Rigidbody2D rigidBody;
    [NonSerialized] public Collider2D sightCollider;
    [NonSerialized] public ParticleSystem.MainModule slimeParticles;
    [NonSerialized] private Material bodyMaterial;

    //Constant values
    [NonSerialized] private float interactDistance;

    //Time values to reset timers to
    [Header("Timers")]
    [NonSerialized] private float wanderTimerMax = 3f;
    [NonSerialized] private float detectionTimerMax = 2f;
    [NonSerialized] private float timeOutTimerMax = 10f;
    [NonSerialized] private float actionTimerMax = 1f;
    [NonSerialized] private float resizeTimerMax = 3f;
    [NonSerialized] private float stunEffectTimerMax = 1f;
    #endregion

    #region ---=== Get/Set Variables ===---
    public float DecomposeTimer
    {
        get
        {
            return decomposeTimer;
        }
        set
        {
            decomposeTimer = value;

            if (decomposeTimer <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
    public override float Age
    {
        get { return age; }
        set
        {
            age = value;

            if (age >= deathAge && !dead)
            {
                Die();
            }
            else if (!adult && age >= adultAge)
            {
                GrowUp();
            }
        }
    }
    public override float Nutrition
    {
        get { return nutrition; }
        set
        {
            nutrition = value;

            if (nutrition <= 0)
            {
                if (!dead)
                {
                    Die();
                }
                else
                {
                    DecomposeTimer -= hungerRate;
                }
            }
        }
    }
    public AnimalAction Action
    {
        get { return action; }
        set
        {
            timeOutTimer = timeOutTimerMax;
            if (value == AnimalAction.Idle)
            {
                //Just set to idle from other action
                if (action != AnimalAction.Idle)
                    wanderTimer = 0; //To make sure that a new position is set
                target = null;
            }

            action = value;
        }
    }
    #endregion


    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        sightCollider = GetComponent<CircleCollider2D>();

        detectionTimer = UnityEngine.Random.Range(0, 1f); //Makes sure many new spawns don't all check their detection collision at the same time
        Action = AnimalAction.Idle; //Default and resets the timeOutTimer
        slimeParticles = transform.GetComponentInChildren<ParticleSystem>().main;
    }

    void Start()
    {
        interactDistance = GameManager.Instance.gameCellSize;
        bodyMaterial = transform.Find("Sprites").Find("BodySprite").GetComponent<Renderer>().material;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        Nutrition -= hungerRate; //Will automatically decrement DecomposeTimer when dead
        resizeTimer -= Time.deltaTime; //Used to grow child and shrink corpse

        if (!dead)
        {
            //Changing stats
            HitPoints += hitPointsRegenSpeed;
            if (HitPoints > maxHitPoints)
                HitPoints = maxHitPoints;
            //Timers
            detectionTimer -= Time.deltaTime;
            timeOutTimer -= Time.deltaTime;
            if (timeOutTimer <= 0)
            {
                timeOutTimer = timeOutTimerMax;

                Action = AnimalAction.Idle;
            }

            if (adult)
            {
                reproductionTimer -= Time.deltaTime;
            }
            else
            {
                if (resizeTimer <= 0)
                {
                    ResizeChild();
                }
            }

            MovementAndAct();
        }
        else
        {
            if (resizeTimer <= 0)
            {
                ResizeCorpse();
            }
        }
    }

    private void OnDestroy()
    {
        //Normally animals decrement this counter when they enter the dead state so this is for when they are destroyed before death
        if (!dead && LifeformManager.Instance)
            LifeformManager.Instance.AnimalPopulation--;
    }

    public void MovementAndAct()
    {
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            stunEffectTimer -= Time.deltaTime;
            
            if (stunEffectTimer <= 0)
            {
                stunEffectTimer = stunEffectTimerMax;

                GameManager.Instance.PlayStunParticle(transform.position, transform.localScale.x);
            }

            return;
        }

        actionTimer -= Time.deltaTime;
        //Idle wander
        if (Action == AnimalAction.Idle)
        {
            wanderTimer -= Time.deltaTime;

            Vector2 thisPosition = new Vector2(rigidBody.position.x, rigidBody.position.y);

            if (wanderTimer <= 0) //Pick new wanderPos
            {
                wanderPos = new Vector2(thisPosition.x + UnityEngine.Random.Range(-100, 100),
                                        thisPosition.y + UnityEngine.Random.Range(-100, 100));

                wanderTimer = wanderTimerMax;
            }

            rigidBody.velocity = (wanderPos - thisPosition).normalized * moveSpeed * Time.deltaTime;
            return;
        }

        //If the obj was destroyed
        if (target == null || target.obj == null)
        {
            Action = AnimalAction.Idle;
            return;
        }

        //If the obj exists but may have died
        if (Action == AnimalAction.Attack || Action == AnimalAction.Run || Action == AnimalAction.Reproduce)
        {
            if (target.animal.dead)
            {
                Action = AnimalAction.Idle;
                return;
            }
        }

        //Otherwise Move to targetObj
        Vector2 targetPos = new (target.obj.transform.position.x, target.obj.transform.position.y);
        Vector2 thisPos = new (rigidBody.position.x, rigidBody.position.y);
        Vector2 direction;
        if (Action == AnimalAction.Run)
            direction = (thisPos - targetPos).normalized;
        else
            direction = (targetPos - thisPos).normalized;
        rigidBody.velocity = direction * moveSpeed * Time.deltaTime;

        //Interact if close enough (Attack, eat)
        if (Action == AnimalAction.Attack && actionTimer <= 0)
        {
            RaycastHit2D[] hitResults = Physics2D.RaycastAll(rigidBody.position, direction, interactDistance * size);
            Debug.DrawRay(rigidBody.position, direction * interactDistance * size, Color.red, 1f, false);
            for (int i = 0; i < hitResults.Length; i++)
            {
                if (!(hitResults[i].collider is CircleCollider2D) &&
                    (hitResults[i].transform.gameObject == target.obj //If it is intended target or other valid target
                    || CheckOffensiveAttack(new LifeformObject(hitResults[i].transform.gameObject, LifeformType.Animal))))
                {
                    HitAnimal(target.animal);
                    break;
                }
            }
        }
        else if (Action == AnimalAction.Eat && actionTimer <= 0)
        {
            RaycastHit2D[] hitResults = Physics2D.RaycastAll(rigidBody.position, direction, interactDistance * size);
            Debug.DrawRay(rigidBody.position, direction * interactDistance * size, Color.green, 1f, false);
            for (int i = 0; i < hitResults.Length; i++)
            {
                if (hitResults[i].transform.gameObject == target.obj)
                {
                    EatFood(target);
                    break;
                }
            }
        }
        else if (Action == AnimalAction.Reproduce && actionTimer <= 0)
        {
            if (sexualReproduction)
            {
                RaycastHit2D[] hitResults = Physics2D.RaycastAll(rigidBody.position, direction, interactDistance * size);
                Debug.DrawRay(rigidBody.position, direction * interactDistance * size, Color.cyan, 1f, false);
                for (int i = 0; i < hitResults.Length; i++)
                {
                    if (!(hitResults[i].collider is CircleCollider2D) &&
                        hitResults[i].transform.gameObject == target.obj)
                    {
                        Reproduce(target.animal);
                        break;
                    }
                }
            }
            else //asexual
            {
                Reproduce(null);
            }
        }

        //Not yet used
        if (moveStyle == AnimalMoveStyle.Normal)
        {

        }
        else if (moveStyle == AnimalMoveStyle.Burst)
        {

        }
        else if (moveStyle == AnimalMoveStyle.Hop)
        {

        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (detectionTimer <= 0)
        {
            detectionList.Clear();
            detectionTimer = detectionTimerMax;
        }

        //circleCollider Prevents seeing trigger colliders which is used for sight which prevents seeing others through their sight circle
        //The list check makes sure the same object isn't checked again until the timer resets
        if (other is CircleCollider2D || detectionList.Contains(other.gameObject))
            return;
        else
            detectionList.Add(other.gameObject);

        if (other.gameObject == null || other.gameObject == gameObject)
            return;

        LifeformObject target = new (other.gameObject);

        if (Nutrition / maxNutrition > hungerEatThreshold) //Full
        {
            //Order of Priority
            if (CheckDefensiveAttack(target)) { return; }
            else if (CheckRun(target)) { return; }
            else if (CheckOffensiveAttack(target)) { return; }
            else { CheckReproduce(target); } //Only if full
        }
        else if (Nutrition / maxNutrition > hungerHuntThreshold) //Decently full
        {
            if (CheckDefensiveAttack(target)) { return; }
            else if (CheckRun(target)) { return; }
            else if (CheckEat(target)) { return; }
            else { CheckOffensiveAttack(target); }
        }
        else //Seriously hungry
        {
            if (CheckEat(target)) { return; }
            else if (CheckHuntAttack(target)) { return; }
            else if (CheckDefensiveAttack(target)) { return; }
            else if (CheckOffensiveAttack(target)) { return; }
            else { CheckRun(target); }
        }
    }


    #region ---=== Detection Decisions ===---
    private bool CheckEat(LifeformObject _lifeform)
    {
        //Reasons to eat plant
        bool willEat = false;

        Plant targetPlant = _lifeform.plant;
        Animal targetAnimal = _lifeform.animal;

        if (targetPlant != null)
        {
            if (diet == AnimalDiet.Carnivore)
                return false;

            if (Action == AnimalAction.Idle)
                willEat = true;
            else if (Action == AnimalAction.Eat && CloserNewTarget(target, _lifeform))
                willEat = true;
        }
        else if (targetAnimal != null)
        {
            if (diet == AnimalDiet.Herbivore || !targetAnimal.dead) //Animal corpses only
                return false;

            if (Action == AnimalAction.Idle)
                willEat = true;
            else if (Action == AnimalAction.Eat && CloserNewTarget(target, _lifeform))
                willEat = true;
        }

        if (willEat)
        {
            target = _lifeform;
            Action = AnimalAction.Eat; //Resets timeOutTimer
            return true;
        }
        else
            return false;
    }

    private bool CheckOffensiveAttack(LifeformObject _lifeform)
    {
        bool willAttack = false;

        //Animal only
        Animal targetAnimal = _lifeform.animal;
        if (!ValidAnimalInteraction(targetAnimal, true, true))
            return false;

        //Reasons to attack
        if (nature == AnimalNature.Violent && adult
           || (targetAnimal.target != null && targetAnimal.target.obj == gameObject && nature == AnimalNature.Neutral)) //Neutral and being targetted
        {
            if (Action == AnimalAction.Idle)
                willAttack = true;
            else if (Action == AnimalAction.Attack && CloserNewTarget(target, _lifeform))
                willAttack = true;
        }

        if (willAttack)
        {
            target = _lifeform;
            Action = AnimalAction.Attack; //Resets timeOutTimer
            return true;
        }
        else
            return false;
    }

    private bool CheckHuntAttack(LifeformObject _lifeform)
    {
        bool willAttack = false;

        //Animal only
        Animal targetAnimal = _lifeform.animal;
        if (!ValidAnimalInteraction(targetAnimal, true, true))
            return false;

        //Reasons to attack
        if (Action == AnimalAction.Idle && diet != AnimalDiet.Herbivore) //Currently searching for food and can ear animals
        {
            if (Action == AnimalAction.Idle)
                willAttack = true;
            else if (Action == AnimalAction.Attack && CloserNewTarget(target, _lifeform))
                willAttack = true;
        }

        if (willAttack)
        {
            target = _lifeform;
            Action = AnimalAction.Attack; //Resets timeOutTimer
            return true;
        }
        else
            return false;
    }

    private bool CheckDefensiveAttack(LifeformObject _lifeform)
    {
        bool willAttack = false;

        //Animal only
        Animal targetAnimal = _lifeform.animal;
        if (!ValidAnimalInteraction(targetAnimal, true, false))
            return false;

        //Reasons to defensive attack
        //First condition - Neutral and being targetted
        if (targetAnimal.target != null && targetAnimal.target.obj == gameObject && nature != AnimalNature.Fearful)
        {
            if (Action == AnimalAction.Idle)
                willAttack = true;
            else if (Action == AnimalAction.Attack && CloserNewTarget(target, _lifeform))
                willAttack = true;
        }

        if (willAttack)
        {
            target = _lifeform;
            Action = AnimalAction.Attack; //Resets timeOutTimer
            return true;
        }
        else
            return false;
    }

    private bool CheckRun(LifeformObject _lifeform)
    {
        bool willRun = false;

        //Animal only
        Animal targetAnimal = _lifeform.animal;
        if (!ValidAnimalInteraction(targetAnimal, true, false))
            return false;

        //Reasons to run
        //First condition - fearful & being targetted
        //Second condition - hp too low and potential target is able to attack
        if ((targetAnimal.target != null && targetAnimal.target.obj == gameObject && nature == AnimalNature.Fearful)
            || (HitPoints / maxHitPoints < hpRunThreshold
            && (targetAnimal.diet != AnimalDiet.Herbivore || targetAnimal.nature == AnimalNature.Violent))) //Being attacked
        {
            if (Action != AnimalAction.Run)
                willRun = true;
            else if (CloserNewTarget(target, _lifeform)) //Prioritize closer threat if already running
                willRun = true;
        }

        if (willRun)
        {
            target = _lifeform;
            Action = AnimalAction.Run; //Resets timeOutTimer
            return true;
        }
        else
            return false;
    }

    private bool CheckReproduce(LifeformObject _lifeform)
    {
        if (sexualReproduction && CanReproduce(sexualReproduction))
        {
            //Animal only
            Animal targetAnimal = _lifeform.animal;
            if (!ValidAnimalInteraction(targetAnimal, true, true)
                || !targetAnimal.CanReproduce(sexualReproduction))
                return false;

            if (Action == AnimalAction.Idle)
            {
                target = _lifeform;
                Action = AnimalAction.Reproduce; //Resets timeOutTimer
                return true;
            }
            else if (Action == AnimalAction.Reproduce && CloserNewTarget(target, _lifeform))
            {
                target = _lifeform;
                Action = AnimalAction.Reproduce; //Resets timeOutTimer
                return true;
            }
        }
        else if (!sexualReproduction && CanReproduce(sexualReproduction))
        {
            target = new LifeformObject(gameObject, LifeformType.Animal);
            Action = AnimalAction.Reproduce; //Resets timeOutTimer
            return true;
        }

        return false;
    }
    #endregion
    

    #region ---=== Direct Interactions With Other Lifeform ===---
    private void HitAnimal(Animal _animal)
    {
        actionTimer = actionTimerMax;

        GameManager.Instance.PlayAnimalHitParticle(_animal.transform.position, _animal.transform.localScale.x);

        _animal.deathAge -= attackPower;
        _animal.HitPoints -= attackPower;
    }

    private void EatFood(LifeformObject _lifeform)
    {
        //Reset variables
        actionTimer = actionTimerMax;
        Action = AnimalAction.Idle;

        Plant plant = _lifeform.plant;
        Animal animal = _lifeform.animal;

        //Only plant and animal (only corpses) right now
        if (plant != null)
        {
            GameManager.Instance.PlayPlantHitParticle(plant.transform.position, plant.transform.localScale.x);

            float nutritionMult = LifeformManager.Instance.CalcColorSimilarity(color, plant.color);
            float wantedNutrition = (maxNutrition - nutrition) / nutritionMult;
            float targetNutrition = plant.Nutrition * nutritionMult;

            //So the eater cannot eat more than is there
            if (plant.Nutrition - wantedNutrition >= 0)
            {
                Nutrition += wantedNutrition * nutritionMult;
                plant.Nutrition -= wantedNutrition;
                plant.Resize();
            }
            else //Take all
            {
                Nutrition += targetNutrition;
                plant.Nutrition = 0;
            }
        }
        else if (animal != null)
        {
            GameManager.Instance.PlayAnimalHitParticle(animal.transform.position, animal.transform.localScale.x);

            float nutritionMult = LifeformManager.Instance.CalcColorSimilarity(color, animal.color);
            float wantedNutrition = (maxNutrition - nutrition) / nutritionMult;
            float targetNutrition = animal.DecomposeTimer * nutritionMult;

            //So the eater cannot eat more than is there
            if (animal.DecomposeTimer - wantedNutrition >= 0)
            {
                Nutrition += wantedNutrition * nutritionMult;
                animal.DecomposeTimer -= wantedNutrition;
                animal.ResizeCorpse();
            }
            else //Take all
            {
                Nutrition += targetNutrition;
                animal.DecomposeTimer = 0;
            }
        }
    }
    
    private bool Reproduce(Animal _animal)
    {
        if (_animal == null && CanReproduce(sexualReproduction))
        {
            GameManager.Instance.PlayAnimalReproduceParticle(transform.position, transform.localScale.x);
            LifeformManager.Instance.AnimalAsexualReproduction(this);
            return true;
        }
        else if (CanReproduce(sexualReproduction) && _animal.CanReproduce(sexualReproduction))
        {
            GameManager.Instance.PlayAnimalReproduceParticle(transform.position, transform.localScale.x);
            LifeformManager.Instance.AnimalSexualReproduction(this, _animal);
            return true;
        }

        //Can no longer reproduce
        Action = AnimalAction.Idle;
        return false;
    }
    #endregion


    #region ---=== Decision Conditions ===---
    private bool ValidAnimalInteraction(Animal _animal, bool _checkDead, bool _checkRelated)
    {
        //Not animal/animal class is missing or is same as self or is checking for dead and is dead
        if (_animal == null || _animal == this || (_checkDead && _animal.dead))
            return false;

        if (_checkRelated)
        {
            //They are a parent or this is a parent to them
            if (parents.Contains(_animal) || _animal.parents.Contains(this))
                return false;

            //They share parents
            for (int i = 0; i < parents.Count; i++)
            {
                if (_animal.parents.Contains(parents[i]))
                    return false;
            }
        }

        return true;
    }

    private bool CloserNewTarget(LifeformObject _old, LifeformObject _new)
    {
        //Still have a pointer to a target but that target object has been destroyed
        if (_old.obj == null)
        {
            target = null;
            return true;
        }

        //New target was found this frame so it is assumed to be fine but old target can be destroyed in between
        if (Vector2.Distance(rigidBody.position, _new.obj.transform.position) <
            Vector2.Distance(rigidBody.position, _old.obj.transform.position))
            return true;
        else
            return false;
    }

    private bool CanReproduce(bool _withSexual)
    {
        //If with sexual but they are asexual
        if (_withSexual && !sexualReproduction)
            return false;
        //If with asexual but they are sexual
        else if (!_withSexual && sexualReproduction)
            return false;

        //Can breed if their timer is out & they have enough food & they are adult
        if (reproductionTimer <= 0 && Nutrition / maxNutrition > hungerEatThreshold && adult)
            return true;
        else
            return false;
    }
    #endregion


    #region ---=== Basic Animal Functions ===---
    public override void Die()
    {
        LifeformManager.Instance.AnimalPopulation--;
        dead = true;
        Nutrition = 0; //Needed if they die from other reasons
        DecomposeTimer = maxNutrition;

        slimeParticles.loop = false;
        GameManager.Instance.PlayAnimalDeathParticle(transform.position, transform.localScale.x);

        //Turn off for performance
        sightCollider.enabled = false;

        //Update Sprite
        transform.Find("Sprites").localScale = new Vector3(1, -1, 1); //Resets other changes too
        size = transform.localScale.x;
        bodyMaterial.SetFloat("_WarpStrength", 0);
    }

    public void GrowUp()
    {
        if (dead)
            return;

        age = adultAge;//Age will create a loop
        adult = true;
        GameManager.Instance.PlayAnimalGrowUpParticle(transform.position, transform.localScale.x);
        LifeformManager.Instance.AnimalMultiplyAgeStats(this, 1 / LifeformManager.Instance.animalChildStatMult);
    }

    public void ResizeChild()
    {
        resizeTimer = resizeTimerMax;

        float newSize = 0.5f/(adultAge / maxAge) * (Age / maxAge) + 0.5f;
        transform.localScale = new Vector3(size * newSize, size * newSize, transform.localScale.z);

        //Fixes YZ depth sorting for scaled objects
        transform.position = new Vector3(transform.position.x, transform.position.y, -(transform.localScale.y - 1) / 2);
    }

    public void ResizeCorpse()
    {
        resizeTimer = resizeTimerMax;

        float remainingSize = decomposeTimer / maxNutrition;
        if (remainingSize < 0.15f)
            remainingSize = 0.15f;
        transform.localScale = new Vector3(size * remainingSize, size * remainingSize, transform.localScale.z);

        //Fixes YZ depth sorting for scaled objects
        transform.position = new Vector3(transform.position.x, transform.position.y, -(transform.localScale.y - 1) / 2);
    }

    public void AfterBreed(float _nutritionLost, float _deathAgeLost)
    {
        //Stats lost
        Nutrition -= maxNutrition * _nutritionLost;
        deathAge -= maxAge * _deathAgeLost;

        //Timer Resets
        actionTimer = actionTimerMax;
        detectionTimer = detectionTimerMax; //Prevents immediatly doing something else
        reproductionTimer = reproductionWaitTime;
        Action = AnimalAction.Idle;
    }

    //Accessors for editor buttons
    public void Randomize()
    {
        if (dead)
            return;

        LifeformManager.Instance.RandomizeAnimal(this);
    }

    public void UpdateProperties()
    {
        if (dead)
            return;
        
        LifeformManager.Instance.UpdateAnimal(this);
    }

    public void SetStun(float _time)
    {
        stunTimer = _time;
        stunEffectTimer = 0;
    }

    public void ResetReproductionTime()
    {
        reproductionTimer = reproductionWaitTime;
    }

    public float GetReproductionTime()
    {
        return reproductionTimer;
    }

    #endregion
}