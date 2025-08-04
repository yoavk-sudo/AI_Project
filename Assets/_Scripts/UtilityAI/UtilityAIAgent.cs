using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UtilityAIAgent : MonoBehaviour
{
    public List<AIAction> actions = new List<AIAction>();
    public Context Context;
    public NavMeshAgent Agent;
    public Sensor Sensor;

    [ReadOnly]
    public List<float> readonlyEvaluations;
    public bool IsIdle { get; private set; }
    void Awake()
    {
        Context = new Context(this);

        foreach (var action in actions)
        {
            action.Initialize(Context);
            readonlyEvaluations.Add(0f); // Initialize with some default values
        }
    }

    void Update()
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
