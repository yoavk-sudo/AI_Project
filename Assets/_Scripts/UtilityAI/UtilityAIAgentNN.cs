using System;
using UnityEngine;

public class UtilityAIAgentNN : UtilityAIAgent
{
    [Header("Neural Network")]
    public UtilityNNet neuralNetwork;

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
        Fitness += 10f;
    }
    public void OnAttackMissed()
    {
        Fitness -= 5f;
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
        Fitness += 10f;
    }
    public void OnIdleTooLong()
    {
        Fitness -= 5f;
    }
    private void SubscribeEvents()
    {
        HealthComponent.OnHit += OnTookDamage;
        HealthComponent.OnHeal += OnHealDamage;
        OnAttackLandedAction += OnAttackLanded;
        OnAttackMissedAction += OnAttackMissed;
        OnEnemyKilledAction += OnEnemyKilled;
        OnDeathAction += OnDeath;
    }
    private void UnsubscribeEvents()
    {
        HealthComponent.OnHit -= OnTookDamage;
        HealthComponent.OnHeal -= OnHealDamage;
        OnAttackLandedAction -= OnAttackLanded;
        OnAttackMissedAction -= OnAttackMissed;
        OnEnemyKilledAction -= OnEnemyKilled;
        OnDeathAction -= OnDeath;
    }
}
