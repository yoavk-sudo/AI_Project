using System;
using System.IO;
using UnityEngine;

public class UtilityNNet
{
    private int _inputCount;
    private int _outputCount;
    private int _hiddenLayerCount;
    private int _hiddenNeuronCount;

    private float[] _input;
    private float[][] _hidden;   // [layer][neuron]
    private float[] _output;

    private float[][,] _W;       // weights[layer][from, to]
    private float[][] _B;        // biases[layer][to]

    public bool UseSoftmax = false;
    public float InitRange = 1f;

    public void Initialize(int inputCount, int outputCount, int hiddenLayerCount, int hiddenNeuronCount)
    {
        _inputCount = inputCount;
        _outputCount = outputCount;
        _hiddenLayerCount = hiddenLayerCount;
        _hiddenNeuronCount = hiddenNeuronCount;

        _input = new float[_inputCount];
        _hidden = new float[_hiddenLayerCount][];
        for (int i = 0; i < _hiddenLayerCount; i++)
            _hidden[i] = new float[_hiddenNeuronCount];
        _output = new float[_outputCount];

        // Allocate weights
        _W = new float[_hiddenLayerCount + 1][,]; // +1 for output layer
        // Input -> H1
        _W[0] = new float[_inputCount, _hiddenNeuronCount];
        // Hidden -> Hidden
        for (int i = 1; i < _hiddenLayerCount; i++)
            _W[i] = new float[_hiddenNeuronCount, _hiddenNeuronCount];
        // Last hidden -> Output
        _W[_hiddenLayerCount] = new float[_hiddenNeuronCount, _outputCount];

        // Allocate biases
        _B = new float[_hiddenLayerCount + 1][];
        for (int i = 0; i < _hiddenLayerCount; i++)
            _B[i] = new float[_hiddenNeuronCount];
        _B[_hiddenLayerCount] = new float[_outputCount];

        RandomizeParameters();
    }
    public void InitializeCopy(UtilityNNet other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        _inputCount = other._inputCount;
        _outputCount = other._outputCount;
        _hiddenLayerCount = other._hiddenLayerCount;
        _hiddenNeuronCount = other._hiddenNeuronCount;
        UseSoftmax = other.UseSoftmax;
        InitRange = other.InitRange;

        _input = new float[_inputCount];
        _hidden = new float[_hiddenLayerCount][];
        for (int i = 0; i < _hiddenLayerCount; i++)
            _hidden[i] = new float[_hiddenNeuronCount];
        _output = new float[_outputCount];

        // Allocate new arrays for weights
        _W = new float[_hiddenLayerCount + 1][,];
        _W[0] = new float[_inputCount, _hiddenNeuronCount];
        for (int i = 1; i < _hiddenLayerCount; i++)
            _W[i] = new float[_hiddenNeuronCount, _hiddenNeuronCount];
        _W[_hiddenLayerCount] = new float[_hiddenNeuronCount, _outputCount];

        // Allocate new arrays for biases
        _B = new float[_hiddenLayerCount + 1][];
        for (int i = 0; i < _hiddenLayerCount; i++)
            _B[i] = new float[_hiddenNeuronCount];
        _B[_hiddenLayerCount] = new float[_outputCount];

        // Copy values from other network
        for (int l = 0; l < _W.Length; l++)
        {
            for (int i = 0; i < _W[l].GetLength(0); i++)
            {
                for (int j = 0; j < _W[l].GetLength(1); j++)
                {
                    _W[l][i, j] = other._W[l][i, j];
                }
            }
        }

        for (int l = 0; l < _B.Length; l++)
        {
            for (int j = 0; j < _B[l].Length; j++)
            {
                _B[l][j] = other._B[l][j];
            }
        }
    }

    public void RandomizeParameters()
    {
        for (int l = 0; l < _W.Length; l++)
        {
            for (int i = 0; i < _W[l].GetLength(0); i++)
            {
                for (int j = 0; j < _W[l].GetLength(1); j++)
                {
                    _W[l][i, j] = UnityEngine.Random.Range(-InitRange, InitRange);
                }
            }
        }
        for (int l = 0; l < _B.Length; l++)
        {
            for (int j = 0; j < _B[l].Length; j++)
            {
                _B[l][j] = UnityEngine.Random.Range(-InitRange, InitRange);
            }
        }
    }

    public float[] Run(float[] inputs)
    {
        if (inputs.Length != _inputCount)
            throw new ArgumentException($"Expected {_inputCount} inputs, got {inputs.Length}");

        Array.Copy(inputs, _input, _inputCount);

        // Input -> Hidden[0]
        for (int j = 0; j < _hiddenNeuronCount; j++)
        {
            float sum = _B[0][j];
            for (int i = 0; i < _inputCount; i++)
                sum += _input[i] * _W[0][i, j];
            _hidden[0][j] = Tanh(sum);
        }

        // Hidden -> Hidden
        for (int l = 1; l < _hiddenLayerCount; l++)
        {
            for (int j = 0; j < _hiddenNeuronCount; j++)
            {
                float sum = _B[l][j];
                for (int i = 0; i < _hiddenNeuronCount; i++)
                    sum += _hidden[l - 1][i] * _W[l][i, j];
                _hidden[l][j] = Tanh(sum);
            }
        }

        // Last hidden -> Output
        int outLayer = _hiddenLayerCount;
        for (int j = 0; j < _outputCount; j++)
        {
            float sum = _B[outLayer][j];
            for (int i = 0; i < _hiddenNeuronCount; i++)
                sum += _hidden[_hiddenLayerCount - 1][i] * _W[outLayer][i, j];
            _output[j] = sum; // raw
        }

        if (UseSoftmax)
            Softmax(_output);
        else
        {
            for (int j = 0; j < _outputCount; j++)
                _output[j] = Sigmoid(_output[j]);
        }

        return _output;
    }

    private static float Tanh(float x) => (float)Math.Tanh(x);
    private static float Sigmoid(float x) => 1f / (1f + Mathf.Exp(-x));

    private static void Softmax(float[] values)
    {
        float max = float.NegativeInfinity;
        for (int i = 0; i < values.Length; i++)
            if (values[i] > max) max = values[i];
        float sum = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = Mathf.Exp(values[i] - max);
            sum += values[i];
        }
        if (sum <= Mathf.Epsilon) sum = Mathf.Epsilon;
        for (int i = 0; i < values.Length; i++)
            values[i] /= sum;
    }
    public void Mutate(float mutationChance, float mutationStrength)
    {
        // Mutate weights
        for (int l = 0; l < _W.Length; l++)
        {
            for (int i = 0; i < _W[l].GetLength(0); i++)
            {
                for (int j = 0; j < _W[l].GetLength(1); j++)
                {
                    if (UnityEngine.Random.value < mutationChance)
                    {
                        _W[l][i, j] += UnityEngine.Random.Range(-mutationStrength, mutationStrength);
                    }
                }
            }
        }

        // Mutate biases
        for (int l = 0; l < _B.Length; l++)
        {
            for (int j = 0; j < _B[l].Length; j++)
            {
                if (UnityEngine.Random.value < mutationChance)
                {
                    _B[l][j] += UnityEngine.Random.Range(-mutationStrength, mutationStrength);
                }
            }
        }
    }

    // --- JSON SAVE ---
    public string ToJson(bool prettyPrint = false)
    {
        var dto = new UtilityNNetDTO
        {
            inputCount = _inputCount,
            outputCount = _outputCount,
            hiddenLayerCount = _hiddenLayerCount,
            hiddenNeuronCount = _hiddenNeuronCount,
            useSoftmax = UseSoftmax,
            initRange = InitRange,

            W = new WeightLayerDTO[_W.Length],
            B = new float[_B.Length][]
        };

        // Weights -> DTO (flat array)
        for (int l = 0; l < _W.Length; l++)
        {
            int rows = _W[l].GetLength(0);
            int cols = _W[l].GetLength(1);

            var wDto = new WeightLayerDTO
            {
                rows = rows,
                cols = cols,
                data = new float[rows * cols]
            };

            int index = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    wDto.data[index++] = _W[l][r, c];
                }
            }

            dto.W[l] = wDto;
        }

        // Biases -> DTO
        for (int l = 0; l < _B.Length; l++)
        {
            int n = _B[l].Length;
            var arr = new float[n];
            Array.Copy(_B[l], arr, n);
            dto.B[l] = arr;
        }

        return JsonUtility.ToJson(dto, prettyPrint);
    }


    public void SaveToFile(string path, bool prettyPrint = false)
    {
        var json = ToJson(prettyPrint);
        File.WriteAllText(path, json);
    }

    // --- JSON LOAD (overwrite this instance) ---
    public void LoadFromJson(string json)
    {
        var dto = JsonUtility.FromJson<UtilityNNetDTO>(json);
        Initialize(dto.inputCount, dto.outputCount, dto.hiddenLayerCount, dto.hiddenNeuronCount);
        UseSoftmax = dto.useSoftmax;
        InitRange = dto.initRange;

        // Copy weights from flat array
        for (int l = 0; l < dto.W.Length; l++)
        {
            var w = dto.W[l];
            if (_W[l].GetLength(0) != w.rows || _W[l].GetLength(1) != w.cols)
                throw new Exception(
                    $"Weight shape mismatch at layer {l}: expected [{_W[l].GetLength(0)},{_W[l].GetLength(1)}], got [{w.rows},{w.cols}]"
                );

            int index = 0;
            for (int r = 0; r < w.rows; r++)
            {
                for (int c = 0; c < w.cols; c++)
                {
                    _W[l][r, c] = w.data[index++];
                }
            }
        }

        // Copy biases from DTO
        for (int l = 0; l < dto.B.Length; l++)
        {
            if (_B[l].Length != dto.B[l].Length)
                throw new Exception(
                    $"Bias length mismatch at layer {l}: expected {_B[l].Length}, got {dto.B[l].Length}"
                );

            Array.Copy(dto.B[l], _B[l], _B[l].Length);
        }
    }


    // Convenience factory
    public static UtilityNNet FromJson(string json)
{
    var net = new UtilityNNet();
    net.LoadFromJson(json);
    return net;
}

// File load
public static UtilityNNet LoadFromFile(string path)
{
    var json = File.ReadAllText(path);
    return FromJson(json);
}



}
[Serializable]
public class UtilityNNetDTO
{
    public int inputCount;
    public int outputCount;
    public int hiddenLayerCount;
    public int hiddenNeuronCount;
    public bool useSoftmax;
    public float initRange;

    public WeightLayerDTO[] W;   // each weight layer as rows
    public float[][] B;          // biases per layer
}

[Serializable]
public class WeightLayerDTO
{
    public int rows;
    public int cols;
    public float[] data; // flat array length = rows * cols
}

