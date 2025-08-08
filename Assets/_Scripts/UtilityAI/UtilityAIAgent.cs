using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using System;
public class UtilityAIAgent : MonoBehaviour
{
    [Header("AI Components")]
    public List<AIAction> actions = new List<AIAction>();
    public Context Context;
    public NavMeshAgent Agent;
    public Sensor Sensor;
    public Health HealthComponent;

    [ReadOnly]
    public List<float> readonlyEvaluations;
    public bool IsIdle { get; private set; }
    public Action OnAttackLandedAction;
    public Action OnAttackMissedAction;
    public Action OnEnemyKilledAction;
    public Action OnDeathAction;
    public Action OnHeal;
    protected virtual void OnEnable()
    {
        Context = new Context(this);
        readonlyEvaluations.Clear();
        foreach (var action in actions)
        {
            action.Initialize(Context);
            readonlyEvaluations.Add(0f); // Initialize with some default values
        }
    }

    protected virtual void Update()
    {
        
        AIAction bestAction = null;
        float highestUtility = float.MinValue;

        foreach (var action in actions)
        {
            float utility = action.CalculateUtility(Context);
            if (utility > highestUtility)
            {
                highestUtility = utility;
                bestAction = action;
            }
            readonlyEvaluations[actions.IndexOf(action)] = utility;
        }

        if (bestAction != null)
        {
            bestAction.Execute(Context);
            IsIdle = bestAction is IdleAIAction;
        }
    }
}
