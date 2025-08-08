using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [Header("prefabs")]
    [SerializeField] private UtilityAIAgentNN _agentPrefab;
    [SerializeField] private GameObject _potionPrefab;
    [Header("holders")]
    [SerializeField] private Transform _potionsHolder;
    [SerializeField] private Transform _enemiesHolder;
    [Header("Spawn Points")]
    [SerializeField] private Transform[] _enemySpawnPoints;
    [SerializeField] private Transform[] _potionsSpawnPoints;
    [Header("Training Settings")]
    [SerializeField] private float _sessionTime = 30f;
    [SerializeField] private float _mutationChance = 0.05f;
    [SerializeField] private float _mutationStrength = 0.3f;

    private float _timer;
    [Header("Readonly for monitoring")]
    [ReadOnly , SerializeField] private int _generation;
    [ReadOnly , SerializeField] private List<UtilityAIAgentNN> _agents = new List<UtilityAIAgentNN>();
    private List<GameObject> _potions = new List<GameObject>();

    private void Start()
    {
        SpawnNewPotions();
        CreateInitialPopulation();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _sessionTime)
        {
            SpawnNewPotions();
            EvolvePopulation();
            _timer = 0f;
        }
    }
    private void SpawnNewPotions()
    {
        foreach (var potion in _potions)
            Destroy(potion.gameObject);
        foreach (var potionSpawnPoint in _potionsSpawnPoints)
        {
            GameObject potion = Instantiate(_potionPrefab, potionSpawnPoint.position, Quaternion.identity, _potionsHolder);
            _potions.Add(potion);
        }
    }

    private void CreateInitialPopulation()
    {
        _agents.Clear();
        _generation = 1;

        for (int i = 0; i < _enemySpawnPoints.Length; i++)
        {
            SpawnAgent(i);
        }
    }

    private void SpawnAgent(int index, UtilityNNet brainToCopy = null)
    {
        Transform spawn = _enemySpawnPoints[index % _enemySpawnPoints.Length];
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
        int eliteCount = Mathf.Min(3, _enemySpawnPoints.Length);

        for (int i = 0; i < _enemySpawnPoints.Length; i++)
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
        if(_generation % 15 == 0)
        {
            #if UNITY_EDITOR
            string resourcesPath = Path.Combine(Application.dataPath, "Resources/Brains");
            if (!Directory.Exists(resourcesPath))
                Directory.CreateDirectory(resourcesPath);

            bestBrains[0].SaveToFile(Path.Combine(resourcesPath,$"Brain_{_generation}.json")); // Save the best brain to a JSON file
            // Every 20 generations, increase population size
            Debug.Log("save brain to json file");
            #endif

        }
    }
    List<UtilityNNet> LoadBestBrains(int numberOfBestBrains)
    {
        var brains = new List<UtilityNNet>();

        // Load all brain files from Resources/Brains
        TextAsset[] brainFiles = Resources.LoadAll<TextAsset>($"Brains");
        if (brainFiles.Length == 0) return brains;

        // Parse numeric suffixes
        var sorted = brainFiles
            .Select(b =>
            {
                string[] parts = b.name.Split('_');
                int num = 0;
                if (parts.Length > 1) int.TryParse(parts.Last(), out num);
                return new { asset = b, index = num };
            })
            .OrderByDescending(x => x.index) // highest first
            .ToList();

        // Take top N
        foreach (var entry in sorted.Take(numberOfBestBrains))
        {
            brains.Add(UtilityNNet.FromJson(entry.asset.text));
        }

        return brains;
    }
    

}
