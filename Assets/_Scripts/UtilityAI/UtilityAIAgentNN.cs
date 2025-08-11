using System;
using UnityEngine;

public class UtilityAIAgentNN : UtilityAIAgent
{
    [Header("Neural Network")]
    [SerializeField] TextAsset _nnBrainToLoad;
    [SerializeField] bool _loadNNOnStart = false;
    public UtilityNNet neuralNetwork;

    int missingAttacks;
    public bool IsIdle { get; private set; }
    [ReadOnly] public float Fitness;

    private float _idleTimer = 0f;

    [SerializeField] private float _idleThreshold = 3f; // seconds before penalty
    [SerializeField] private float _idleMoveThreshold = 0.05f; // min movement speed to count as "active"

    private Vector3 _lastPosition;

    [ReadOnly]
    public float[] readonlyOutputs;

    public Func<UtilityNNet> OnNNDeath;
    protected override void OnEnable()
    {
        base.OnEnable();
        readonlyOutputs = new float[actions.Count];
        SubscribeEvents();
        if(_loadNNOnStart && _nnBrainToLoad == null)
            Debug.LogError("No neural network brain to load. Please assign a valid TextAsset.");
        else
        {
            if (_loadNNOnStart)
            {
                neuralNetwork = UtilityNNet.FromJson(_nnBrainToLoad.text);
                if (neuralNetwork == null)
                    Debug.LogError("Failed to load neural network from the provided TextAsset.");
            }
        }
    }
    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    protected override void Update()
    {
        foreach (var action in actions)
        {
            float utility = action.CalculateUtility(Context);
            readonlyEvaluations[actions.IndexOf(action)] = utility;
        }
        float[] inputs = GatherNNInputs();

        // Get outputs from neural net
        readonlyOutputs = neuralNetwork.Run(inputs);
        int bestActionIndex = 0;
        //outputs count cant be 0.
        float maxEvaluation = readonlyOutputs[0];

        for (int i = 1; i < readonlyOutputs.Length; i++)
        {
            if (readonlyOutputs[i] > maxEvaluation)
            {
                maxEvaluation = readonlyOutputs[i];
                bestActionIndex = i;
            }
        }

        // Interpret first output as index
        AIAction bestAction = actions[bestActionIndex];

        // Execute the chosen action
        bestAction.Execute(Context);
        IsIdle = bestAction is IdleAIAction;
        CheckIdlePenalty();
    }

    private float[] GatherNNInputs()
    {
        return readonlyEvaluations.ToArray();
    }
    
    private void CheckIdlePenalty()
    {
        // 1) Movement-based idle detection
        float movedDistance = Vector3.Distance(transform.position, _lastPosition);
        bool isIdle = movedDistance < _idleMoveThreshold;

        // 2) Track idle time
        if (isIdle)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _idleThreshold)
            {
                Fitness -=3f;
                _idleTimer = 0f; // reset so it doesn't spam every frame
            }
        }
        else
        {
            _idleTimer = 0f; // reset if moving
        }

        _lastPosition = transform.position;
    }
    public void OnAttackLanded()
    {
        Fitness += 50;
    }
    public void OnAttackMissed()
    {
        //reward for trying
        Fitness += 2f;
        missingAttacks++;
        if(missingAttacks>=3)
        {
            Fitness -= 10f; // penalty for missing too many attacks
            missingAttacks = 0; // reset counter after penalty
        }
    }

    public void OnEnemyKilled()
    {
        Fitness += 50f;
    }
    public void OnDeath()
    {
        Fitness -= 100f;
    }
    public void OnTookDamage()
    {
        Fitness -= 5f;
    }
    public void OnHealDamage()
    {
        Fitness += 100f;
    }
    public void OnHealDamageOnHighHP()
    {
        Fitness -= 50;
    }
    public void OnGotPotion()
    {
        Fitness += 20;
    }
    public void OnIdleTooLong()
    {
        Fitness -= 5f;
    }
    private void SubscribeEvents()
    {
        HealthComponent.OnHit += OnTookDamage;
        HealthComponent.OnHeal += OnHealDamage;
        HealthComponent.OnHealOnHighHP += OnHealDamageOnHighHP;
        HealthComponent.OnGotPotion += OnGotPotion;
        OnAttackLandedAction += OnAttackLanded;
        OnAttackMissedAction += OnAttackMissed;
        OnEnemyKilledAction += OnEnemyKilled;
        OnDeathAction += OnDeath;
    }
    private void UnsubscribeEvents()
    {
        HealthComponent.OnHit -= OnTookDamage;
        HealthComponent.OnHeal -= OnHealDamage;
        HealthComponent.OnHealOnHighHP -= OnHealDamageOnHighHP;
        HealthComponent.OnGotPotion -= OnGotPotion;
        OnAttackLandedAction -= OnAttackLanded;
        OnAttackMissedAction -= OnAttackMissed;
        OnEnemyKilledAction -= OnEnemyKilled;
        OnDeathAction -= OnDeath;
    }
}
