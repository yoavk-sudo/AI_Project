using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [Header("Training Settings")]
    [SerializeField] private UtilityAIAgentNN _agentPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private int _populationSize = 10;
    [SerializeField] private float _sessionTime = 30f;
    [SerializeField] private float _mutationChance = 0.05f;
    [SerializeField] private float _mutationStrength = 0.3f;

    private float _timer;
    [ReadOnly , SerializeField] private int _generation;
    [ReadOnly , SerializeField] private List<UtilityAIAgentNN> _agents = new List<UtilityAIAgentNN>();

    private void Start()
    {
        CreateInitialPopulation();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _sessionTime)
        {
            EvolvePopulation();
            _timer = 0f;
        }
    }

    private void CreateInitialPopulation()
    {
        _agents.Clear();
        _generation = 1;

        for (int i = 0; i < _populationSize; i++)
        {
            SpawnAgent(i);
        }
    }

    private void SpawnAgent(int index, UtilityNNet brainToCopy = null)
    {
        Transform spawn = _spawnPoints[index % _spawnPoints.Length];
        UtilityAIAgentNN agent = Instantiate(_agentPrefab, spawn.position, spawn.rotation);

        agent.neuralNetwork = new UtilityNNet();
        if (brainToCopy == null)
        {
            agent.neuralNetwork.Initialize(agent.actions.Count, agent.actions.Count, 2, 8);
        }
        else
        {
            agent.neuralNetwork.InitializeCopy(brainToCopy);
        }
        _agents.Add(agent);
    }

    private void EvolvePopulation()
    {
        // Sort by fitness (best first)
        var ordered = _agents.OrderByDescending(a => a.Fitness).ToList();

        Debug.Log($"Generation {_generation} complete. Best fitness: {ordered[0].Fitness}");

        // Save best 3 brains
        UtilityNNet[] bestBrains = new UtilityNNet[3];
        for (int i = 0; i < 3 && i < ordered.Count; i++)
        {
            bestBrains[i] = new UtilityNNet();
            bestBrains[i].InitializeCopy(ordered[i].neuralNetwork);
        }

        // Destroy current population
        foreach (var agent in _agents)
            Destroy(agent.gameObject);
        _agents.Clear();

        _generation++;

        // How many elites to preserve
        int eliteCount = Mathf.Min(3, _populationSize);

        for (int i = 0; i < _populationSize; i++)
        {
            // Choose which elite to base this brain on
            int eliteIdx = i % eliteCount;

            // Make a fresh brain and copy from the elite
            var parent = new UtilityNNet();
            parent.InitializeCopy(bestBrains[eliteIdx]);

            // Mutate only non-elites
            if (i >= eliteCount)
                parent.Mutate(_mutationChance, _mutationStrength);

            SpawnAgent(i, parent);
        }
    }
}
