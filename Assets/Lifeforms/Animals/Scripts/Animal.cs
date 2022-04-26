/* Contains individualized animal data, movement logic, detection logic, interaction logic,
 * and many basic animal functions such as death and aging. Uses a Rigidbody2D and velocity to
 * move which is handled in the first part of the MoveAndAct method. Uses a circular Collider2D
 * trigger to detect and process other objects (looks for animals and plants) and decides what
 * to do with them within the OnTriggerStay2D method. If the animal decides to do an action it
 * moves towards its target and in the second part of the MoveAndAct method it will periodically
 * fire a small Physics2D raycast towards its target. The animal will either hit the animal and
 * initiate a direct interaction or fail and reset because it took too long. The default state
 * for an animal is AnimalAction.Idle in which it will only wander around until it find a target
 * and a reason to interact with that target.
 * 
 * Dependent on classes:
 * PermanentMonoSingleton - GameManager
 * MonoSingleton - LifeformManager */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : SimulatedLifeform
{
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
    [SerializeField] public AnimalSocial social;
    [Header("Derived Abilities")]
    [SerializeField] public float moveSpeed = 1;
    [SerializeField] public float reproductionWaitTime = 10f;
    [Header("Advantage Abilities")]
    [SerializeField] public float sight = 1f;
    [SerializeField] public float attackPower = 1f;
    [SerializeField] public float hitPointsRegenSpeed = 0f;
    [SerializeField] public float reproductionNegatives = 10f;
    [Header("Reproduction")]
    [SerializeField] public bool sexualReproduction = true;
    [SerializeField] public List<Animal> parents = new List<Animal>();

    [Header("Control Variables")]
    [SerializeField] private AnimalAction action;
    [SerializeField] public LifeformObject target; //Context depends on state
    [SerializeField] private Vector2 wanderPos; //Only used for idle wander
    [SerializeField] private List<GameObject> detectionList = new List<GameObject>(); //All objects seen since start of detection cooldown

    [Header("Timers")] //All count down to 0 or less which permits some action
    [SerializeField] private float wanderTimer; //How long an idle animal will walk in one direction
    [SerializeField] private float detectionTimer ; //Time in between checking for new attackTarget
    [SerializeField] private float timeOutTimer = 10f; //Stops action if its taking more than this time
    [SerializeField] private float actionTimer; //Time until able to perform direct interactions such as attacking or eating
    [SerializeField] private float resizeTimer; //Time until updating child and corpse scale (every update disables physics so infrequent is better)
    [SerializeField] public float reproductionTimer; //Time until able to breed
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
    [NonSerialized] public Transform sprites;

    //Constant values
    [NonSerialized] private float interactDistance;

    //Time values to reset timers to
    [Header("Timers")]
    [NonSerialized] public float wanderTimerMax = 3f;
    [NonSerialized] private float detectionTimerMax = 2f;
    [NonSerialized] private float timeOutTimerMax = 10f;
    [NonSerialized] private float actionTimerMax = 1f;
    [NonSerialized] private float resizeTimerMax = 3f;
    [NonSerialized] private float stunEffectTimerMax = 1f;
    #endregion

    #region ---=== Get/Set Variables ===---
    [NonSerialized] private LifeformValues lifeformValues;
    public LifeformValues LifeformValues
    {
        get
        {
            if (lifeformValues == null)
                lifeformValues = LifeformManager.Instance.LifeformValues;

            return lifeformValues;
        }
        private set { } //Never direcly set
    }
    public float DecomposeTimer
    {
        get { return decomposeTimer; }
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


    #region ---=== Basic Methods ===---
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        sightCollider = GetComponent<CircleCollider2D>();
        sprites = transform.Find("Sprites");

        detectionTimer = UnityEngine.Random.Range(0, 1f); //Makes sure many new spawns don't all check their detection collision at the same time
        Action = AnimalAction.Idle; //Default and resets the timeOutTimer
        slimeParticles = transform.GetComponentInChildren<ParticleSystem>().main;
    }

    void Start()
    {
        interactDistance = GameManager.Instance.GameValues.GameCellSize;
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
        if (!dead && LifeformManager.Instance != null)
            LifeformManager.Instance.AnimalPopulation--;
    }
    #endregion


    #region ---=== Movement ===---
    public void MovementAndAct()
    {
        if (stunTimer > 0)
        {
            Stunned();
            return;
        }

        actionTimer -= Time.deltaTime;
        switch (Action)
        {
            case AnimalAction.Idle:
                wanderTimer -= Time.deltaTime;
                if (wanderTimer <= 0) //Pick new wanderPos
                {
                    wanderTimer = wanderTimerMax;
                    wanderPos = new Vector2(rigidBody.position.x + UnityEngine.Random.Range(-100, 100),
                                            rigidBody.position.y + UnityEngine.Random.Range(-100, 100));
                }

                MoveTowardsPosition(wanderPos);
                break;

            case AnimalAction.Follow:
                //Target no longer valid
                if (target == null || target.obj == null || target.animal.dead)
                {
                    Action = AnimalAction.Idle;
                    return;
                }

                //Moves to position and returns direction to target which is used in interaction
                MoveTowardsPosition(new Vector2(target.obj.transform.position.x, target.obj.transform.position.y));
                break;

            case AnimalAction.Run:
                //Target no longer valid
                if (target == null || target.obj == null || target.animal.dead)
                {
                    Action = AnimalAction.Idle;
                    return;
                }

                //Moves to position and returns direction to target which is used in interaction
                MoveTowardsPosition(new Vector2(-target.obj.transform.position.x, -target.obj.transform.position.y));
                break;

            case AnimalAction.Attack:
                //Target no longer valid
                if (target == null || target.obj == null || target.animal.dead)
                {
                    Action = AnimalAction.Idle;
                    return;
                }

                //Moves to position and returns direction to target which is used in interaction
                Vector2 attackDirection = MoveTowardsPosition(new Vector2(target.obj.transform.position.x, target.obj.transform.position.y));

                if (actionTimer > 0)
                    break;
                actionTimer = 0.1f; //Buffer in between raycasts

                RaycastHit2D[] attackResults = Physics2D.RaycastAll(rigidBody.position, attackDirection, interactDistance * size);
                for (int i = 0; i < attackResults.Length; i++)
                {
                    if (!(attackResults[i].collider is CircleCollider2D) &&
                        (attackResults[i].transform.gameObject == target.obj //If it is intended target or other valid target
                        || CheckOffensiveAttack(new LifeformObject(attackResults[i].transform.gameObject, LifeformType.Animal))))
                    {
                        HitAnimal(target.animal);
                        break;
                    }
                }
                break;

            case AnimalAction.Eat:
                //Target no longer valid
                if (target == null || target.obj == null)
                {
                    Action = AnimalAction.Idle;
                    return;
                }

                //Moves to position and returns direction to target which is used in interaction
                Vector2 eatDirection = MoveTowardsPosition(new Vector2(target.obj.transform.position.x, target.obj.transform.position.y));

                if (actionTimer > 0)
                    break;
                actionTimer = 0.1f; //Buffer in between raycasts

                RaycastHit2D[] eatResults = Physics2D.RaycastAll(rigidBody.position, eatDirection, interactDistance * size);
                for (int i = 0; i < eatResults.Length; i++)
                {
                    if (eatResults[i].transform.gameObject == target.obj)
                    {
                        EatFood(target);
                        break;
                    }
                }
                break;

            case AnimalAction.Reproduce:
                //Target no longer valid
                if (target == null || target.obj == null || target.animal.dead)
                {
                    Action = AnimalAction.Idle;
                    return;
                }

                //Moves to position and returns direction to target which is used in interaction
                Vector2 reproduceDirection = MoveTowardsPosition(new Vector2(target.obj.transform.position.x, target.obj.transform.position.y));

                if (actionTimer > 0)
                    break;
                actionTimer = 0.1f; //Buffer in between raycasts

                if (sexualReproduction)
                {
                    RaycastHit2D[] reproduceResults = Physics2D.RaycastAll(rigidBody.position, reproduceDirection, interactDistance * size);
                    for (int i = 0; i < reproduceResults.Length; i++)
                    {
                        if (!(reproduceResults[i].collider is CircleCollider2D) &&
                            reproduceResults[i].transform.gameObject == target.obj)
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
                break;
        }
    }

    private void Stunned()
    {
        stunTimer -= Time.deltaTime;
        stunEffectTimer -= Time.deltaTime;

        if (stunEffectTimer <= 0)
        {
            stunEffectTimer = stunEffectTimerMax;

            GameManager.Instance.PlayStunParticle(transform.position, transform.localScale.x);
        }
    }

    private Vector2 MoveTowardsPosition(Vector2 _targetPos)
    {
        Vector2 thisPos = new(rigidBody.position.x, rigidBody.position.y);
        Vector2 direction = (_targetPos - thisPos).normalized;
        rigidBody.velocity = direction * moveSpeed * Time.deltaTime;

        return direction; //Used for interaction
    }
    #endregion


    #region ---=== Detection ===---
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
            else if (CheckReproduce(target)) { return; } //Only if full
            else { CheckFollow(target); } 
        }
        else if (Nutrition / maxNutrition > hungerHuntThreshold) //Decently full
        {
            if (CheckDefensiveAttack(target)) { return; }
            else if (CheckRun(target)) { return; }
            else if (CheckEat(target)) { return; }
            else if (CheckOffensiveAttack(target)) { return; }
            else { CheckFollow(target); }
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
    #endregion


    #region ---=== Detection Decisions ===---
    private bool CheckEat(LifeformObject _lifeform)
    {
        //Reasons to eat plant
        bool willEat = false;

        Plant targetPlant = _lifeform.plant;
        Animal targetAnimal = _lifeform.animal;

        if (targetPlant != null)
        {
            if (diet == AnimalDiet.Carnivore || targetPlant.dead)
                return false;

            if (Action == AnimalAction.Idle || Action == AnimalAction.Follow) //Unimportant actions
                willEat = true;
            else if (Action == AnimalAction.Eat && CloserNewTarget(target, _lifeform))
                willEat = true;
        }
        else if (targetAnimal != null)
        {
            if (diet == AnimalDiet.Herbivore || !targetAnimal.dead) //Animal corpses only
                return false;

            if (Action == AnimalAction.Idle || Action == AnimalAction.Follow) //Unimportant actions
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
        //1 - Violent and adult
        //2 - Protective and the animal is violent or the animal is targeting a relative
        if (nature == AnimalNature.Violent && adult
           || (social == AnimalSocial.Protective && (targetAnimal.nature == AnimalNature.Violent || (targetAnimal.target != null && targetAnimal.target.animal && IsRelated(targetAnimal.target.animal)))))
        {
            if (Action == AnimalAction.Idle || Action == AnimalAction.Follow) //Unimportant actions
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
        if (Action == AnimalAction.Idle && diet != AnimalDiet.Herbivore) //Currently searching for food and can eat animals
        {
            if (Action == AnimalAction.Idle || Action == AnimalAction.Follow) //Unimportant actions
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

        //Reasons to defensive attack (Both cases check the target's target)
        //1 - Not fearful and being targetted
        //2 - Target's target is an animal and that target has this animal as its parent, defend it
        if (targetAnimal.target != null
            && ((targetAnimal.target.obj == gameObject && nature != AnimalNature.Fearful)
            || (targetAnimal.target.animal != null && targetAnimal.target.animal.parents.Contains(this))))
        {
            if (Action == AnimalAction.Idle || Action == AnimalAction.Follow) //Unimportant actions
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
        //1 - antisocial
        //2 - fearful & being targetted
        //3 - hp too low and potential target is able to attack
        if (social == AnimalSocial.Antisocial
            || (targetAnimal.target != null && targetAnimal.target.obj == gameObject && nature == AnimalNature.Fearful)
            || (HitPoints / maxHitPoints < hpRunThreshold && (targetAnimal.diet != AnimalDiet.Herbivore || targetAnimal.nature == AnimalNature.Violent)))
        {
            if (Action != AnimalAction.Run) //Override everything else
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

    private bool CheckFollow(LifeformObject _lifeform)
    {
        //Animal only
        Animal targetAnimal = _lifeform.animal;
        if (!ValidAnimalInteraction(targetAnimal, true, false))
            return false;

        //Reasons to follow
        //1 - Is a parent
        //2 - Is a follower and they are not violent or a carnivore
        if (parents.Contains(targetAnimal)
            || (social == AnimalSocial.Follower && targetAnimal.nature != AnimalNature.Violent && targetAnimal.diet != AnimalDiet.Carnivore))
        {
            if (Action == AnimalAction.Idle)
            {
                target = _lifeform;
                Action = AnimalAction.Follow; //Resets timeOutTimer
                return true;
            }  
        }

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

            if (Action == AnimalAction.Idle || Action == AnimalAction.Follow) //Unimportant actions
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

        if (_checkRelated && IsRelated(_animal))
            return false;

        return true;
    }

    private bool IsRelated(Animal _animal)
    {
        //They are a parent or this is a parent to them
        if (parents.Contains(_animal) || _animal.parents.Contains(this))
            return true;

        //They share parents
        for (int i = 0; i < parents.Count; i++)
        {
            if (_animal.parents.Contains(parents[i]))
                return true;
        }

        return false;
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
        if (dead)
            return;

        dead = true;
        if (LifeformManager.Instance != null)
            LifeformManager.Instance.AnimalPopulation--;

        Nutrition = 0; //Needed if they die from other reasons
        DecomposeTimer = maxNutrition;

        GameManager.Instance.PlayAnimalDeathParticle(transform.position, transform.localScale.x);

        //Turn off effects and collider
        sightCollider.enabled = false;
        slimeParticles.loop = false;
        bodyMaterial.SetFloat("_WarpStrength", 0);

        //Update Sprite
        sprites.localScale = new Vector3(1, -1, 1); //Resets other changes too
        size = transform.localScale.x;
    }

    public void GrowUp()
    {
        if (dead)
            return;

        age = adultAge;//Age will create a loop
        adult = true;
        GameManager.Instance.PlayAnimalGrowUpParticle(transform.position, transform.localScale.x);
        LifeformManager.Instance.AnimalMultiplyAgeStats(this, 1 / LifeformValues.AnimalChildStatMult);
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

    public void AfterReproduce(float _nutritionLost, float _deathAgeLost)
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
    #endregion
}