using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class TrainingManager : MonoBehaviour
{
    [Header("Training Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float sessionCooldown = 10f;
    public float mutationChance = 0.1f;
    public float mutationAmount = 0.3f;

    private List<GameObject> currentEnemies = new List<GameObject>();
    private float sessionTimer = 0f;
    private string bestBrainPath;

    void Start()
    {
        bestBrainPath = Path.Combine(Application.persistentDataPath, "best_brain.json");
        StartCoroutine(TrainingLoop());
    }

    IEnumerator TrainingLoop()
    {
        while (true)
        {
            SpawnEnemies();
            sessionTimer = 0f;

            // Run training session
            while (sessionTimer < sessionCooldown)
            {
                sessionTimer += Time.deltaTime;
                yield return null;
            }

            EvaluateAndPrepareNextGeneration();
        }
    }

    void SpawnEnemies()
    {
        ClearEnemies();

        foreach (Transform spawnPoint in spawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            UtilityAIAgentNN enemeyUtilityAIAgentNN = enemy.GetComponent<UtilityAIAgentNN>();
            NN nn = enemy.GetComponent<NN>();

            // Load best brain if available, otherwise start fresh
            if (File.Exists(bestBrainPath))
            {
                nn.LoadFromFile(bestBrainPath);
                nn.MutateNetwork(mutationChance, mutationAmount); // Mutate slightly for diversity
            }
            else
            {
                nn.InitializeNetwork(enemeyUtilityAIAgentNN.actions.Count, 32, enemeyUtilityAIAgentNN.actions.Count); // Random initialization
            }

            currentEnemies.Add(enemy);
        }
    }

    void ClearEnemies()
    {
        foreach (var enemy in currentEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        currentEnemies.Clear();
    }

    void EvaluateAndPrepareNextGeneration()
    {
        float bestFitness = float.MinValue;
        NN bestBrain = null;

        foreach (GameObject enemy in currentEnemies)
        {
            if (enemy == null) continue;

            var ai = enemy.GetComponent<UtilityAIAgentNN>(); // or EnemyAI, or whatever script contains `Fitness`
            if (ai == null) continue;

            if (ai.Fitness > bestFitness)
            {
                bestFitness = ai.Fitness;
                bestBrain = enemy.GetComponent<NN>();
            }
        }

        if (bestBrain != null)
        {
            Debug.Log($"New Best Brain with fitness {bestFitness}");
            bestBrain.SaveToFile(bestBrainPath);
        }
        else
        {
            Debug.LogWarning("No valid brains found to save.");
        }
    }
}
