using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NN : MonoBehaviour
{
    [SerializeField] private int[] networkShape = { 7, 32, 1 }; // Example: 7 inputs, 32 hidden, 1 output
    public Layer[] layers;

    void Awake()
    {
        InitializeNetwork(7, 32, 1);
    }

    public void InitializeNetwork(int inputs, int hidden, int outputs)
    {
        networkShape = new int[]{ inputs, hidden, outputs };
        layers = new Layer[networkShape.Length - 1];
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(networkShape[i], networkShape[i + 1]);
        }
    }

    public float[] Brain(float[] inputs)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (i == 0)
            {
                layers[i].Forward(inputs);
                layers[i].Activation(); // hidden layer
            }
            else if (i == layers.Length - 1)
            {
                layers[i].Forward(layers[i - 1].nodeArray);
                layers[i].SigmoidActivation(); // output layer → sigmoid to clamp 0–1
            }
            else
            {
                layers[i].Forward(layers[i - 1].nodeArray);
                layers[i].Activation(); // hidden layer
            }
        }

        return layers[^1].nodeArray;
    }

    public void MutateNetwork(float mutationChance, float mutationAmount)
    {
        foreach (var layer in layers)
        {
            layer.MutateLayer(mutationChance, mutationAmount);
        }
    }

    // -------------------- LAYER CLASS --------------------
    [System.Serializable]
    public class Layer
    {
        public float[,] weightsArray;
        public float[] biasesArray;
        public float[] nodeArray;

        public int n_inputs;
        public int n_neurons;

        public Layer(int n_inputs, int n_neurons)
        {
            this.n_inputs = n_inputs;
            this.n_neurons = n_neurons;

            weightsArray = new float[n_neurons, n_inputs];
            biasesArray = new float[n_neurons];
        }

        public void Forward(float[] inputsArray)
        {
            nodeArray = new float[n_neurons];
            for (int i = 0; i < n_neurons; i++)
            {
                for (int j = 0; j < n_inputs; j++)
                {
                    nodeArray[i] += weightsArray[i, j] * inputsArray[j];
                }
                nodeArray[i] += biasesArray[i];
            }
        }
        public void SigmoidActivation()
        {
            for (int i = 0; i < nodeArray.Length; i++)
            {
                nodeArray[i] = 1f / (1f + Mathf.Exp(-nodeArray[i]));
            }
        }

        public void Activation()
        {
            for (int i = 0; i < nodeArray.Length; i++)
            {
                nodeArray[i] = (float)Math.Tanh(nodeArray[i]); // or ReLU / sigmoid
            }
        }

        public void MutateLayer(float mutationChance, float mutationAmount)
        {
            for (int i = 0; i < n_neurons; i++)
            {
                for (int j = 0; j < n_inputs; j++)
                {
                    if (UnityEngine.Random.value < mutationChance)
                        weightsArray[i, j] += UnityEngine.Random.Range(-1f, 1f) * mutationAmount;
                }
                if (UnityEngine.Random.value < mutationChance)
                    biasesArray[i] += UnityEngine.Random.Range(-1f, 1f) * mutationAmount;
            }
        }
    }

    // -------------------- SERIALIZATION --------------------

    [Serializable]
    public class SerializableLayer
    {
        public int n_inputs;
        public int n_neurons;
        public List<float> biases = new();
        public List<List<float>> weights = new();
    }

    [Serializable]
    public class SerializableNN
    {
        public List<SerializableLayer> layers = new();
    }

    public SerializableNN ToSerializable()
    {
        var serialized = new SerializableNN();

        foreach (var layer in layers)
        {
            var sl = new SerializableLayer
            {
                n_inputs = layer.n_inputs,
                n_neurons = layer.n_neurons
            };

            sl.biases.AddRange(layer.biasesArray);

            for (int i = 0; i < layer.n_neurons; i++)
            {
                var row = new List<float>();
                for (int j = 0; j < layer.n_inputs; j++)
                {
                    row.Add(layer.weightsArray[i, j]);
                }
                sl.weights.Add(row);
            }

            serialized.layers.Add(sl);
        }

        return serialized;
    }

    public void LoadFromSerializable(SerializableNN serialized)
    {
        layers = new Layer[serialized.layers.Count];

        for (int i = 0; i < serialized.layers.Count; i++)
        {
            var sl = serialized.layers[i];
            var layer = new Layer(sl.n_inputs, sl.n_neurons);

            for (int b = 0; b < sl.biases.Count; b++)
            {
                layer.biasesArray[b] = sl.biases[b];
            }

            for (int n = 0; n < sl.n_neurons; n++)
            {
                for (int inp = 0; inp < sl.n_inputs; inp++)
                {
                    layer.weightsArray[n, inp] = sl.weights[n][inp];
                }
            }

            layers[i] = layer;
        }
    }

    public string SaveToJson()
    {
        var serial = ToSerializable();
        return JsonUtility.ToJson(serial, true);
    }

    public void SaveToFile(string path)
    {
        string json = SaveToJson();
        File.WriteAllText(path, json);
    }

    public void LoadFromJson(string json)
    {
        var loaded = JsonUtility.FromJson<SerializableNN>(json);
        LoadFromSerializable(loaded);
    }

    public void LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"Neural network file not found at: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        LoadFromJson(json);
    }
}
