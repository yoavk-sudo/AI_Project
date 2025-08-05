using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;

public class UtilityAIAgentNN : UtilityAIAgent
{
    [Header("Neural Network")]
    public NN neuralNetwork;

    [ReadOnly]
    public List<float> readonlyEvaluations;

    public bool IsIdle { get; private set; }
    public float Fitness { get; private set; }
    public bool MutateMutations { get; private set; } = true;
    public float MutationAmount { get; private set; } = 0.8f;
    public float MutationChance { get; private set; } = 0.3f;
    private bool isMutated = false;
    private void OnEnable()
    {
        Context = new Context(this);
        readonlyEvaluations.Clear();

        foreach (var action in actions)
        {
            action.Initialize(Context);
            readonlyEvaluations.Add(0f); // placeholder for utility scores
        }

        if (neuralNetwork == null)
        {
            neuralNetwork = GetComponent<NN>();
        }
    }

    private void Update()
    {
        //only do this once
        if (!isMutated)
        {
            //call mutate function to mutate the neural network
            MutateCreature();
            isMutated = true;
        }
        float[] inputs = GatherNNInputs();

        // Get outputs from neural net
        float[] outputs = neuralNetwork.Brain(inputs);
        int bestActionIndex = 0;
        //outputs count cant be 0.
        float maxEvaluation = outputs[0];

        for (int i = 1; i < outputs.Length; i++)
        {
            if (outputs[i] > maxEvaluation)
            {
                maxEvaluation = outputs[i];
                bestActionIndex = i;
            }
        }

        // Interpret first output as index
        AIAction bestAction = actions[bestActionIndex];

        // Execute the chosen action
        bestAction.Execute(Context);
        IsIdle = bestAction is IdleAIAction;

        // For debugging and visualization
        for (int i = 0; i < actions.Count; i++)
        {
            readonlyEvaluations[i] = (i == bestActionIndex) ? 1f : 0f;
        }
    }

    private float[] GatherNNInputs()
    {
        return readonlyEvaluations.ToArray();
    }
    private void MutateCreature()
    {
        if (MutateMutations)
        {
            MutationAmount += Random.Range(-1.0f, 1.0f) / 100;
            MutationChance += Random.Range(-1.0f, 1.0f) / 100;
        }

        //make sure mutation amount and chance are positive using max function
        MutationAmount = Mathf.Max(MutationAmount, 0);
        MutationChance = Mathf.Max(MutationChance, 0);

        neuralNetwork.MutateNetwork(MutationAmount, MutationChance);
    }

    public void OnAttackLanded()
    {
        Fitness += 10f;
    }
    public void OnAttackMIssed()
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
    public void OnIdleTooLong()
    {
        Fitness -= 5f;
    }
}
