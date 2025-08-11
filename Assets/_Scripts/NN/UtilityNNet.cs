using System;
using System.IO;
using UnityEngine;

public class UtilityNNet
{
    private int _inputCount;
    private int _outputCount;
    private int _hiddenLayerCount;
    private int _hiddenNeuronCount;

    // Activations
    private float[] _input;                  // [inputCount]
    private float[] _hidden;                 // [_hiddenLayerCount * _hiddenNeuronCount]
    private float[] _output;                 // [outputCount]
    private int[] _hiddenOffsets;            // [hiddenLayerCount], base index per hidden layer

    // Parameters
    private float[][,] _W;                   // weights[layer][from, to]  (keep [,] for clarity/perf)
    private float[] _B;                      // flattened biases of all layers
    private int[] _biasOffsets;              // [hiddenLayerCount + 1], base index per bias layer

    public bool UseSoftmax = false;
    public float InitRange = 1f;

    public void Initialize(int inputCount, int outputCount, int hiddenLayerCount, int hiddenNeuronCount)
    {
        if (inputCount <= 0 || outputCount <= 0) throw new ArgumentException("input/output must be > 0");
        if (hiddenLayerCount <= 0 || hiddenNeuronCount <= 0) throw new ArgumentException("hidden dims must be > 0");

        _inputCount = inputCount;
        _outputCount = outputCount;
        _hiddenLayerCount = hiddenLayerCount;
        _hiddenNeuronCount = hiddenNeuronCount;

        // Activations
        _input = new float[_inputCount];
        _hidden = new float[_hiddenLayerCount * _hiddenNeuronCount];
        _output = new float[_outputCount];

        // Offsets
        _hiddenOffsets = new int[_hiddenLayerCount];
        for (int l = 0; l < _hiddenLayerCount; l++)
            _hiddenOffsets[l] = l * _hiddenNeuronCount;

        // Weights
        _W = new float[_hiddenLayerCount + 1][,];
        _W[0] = new float[_inputCount, _hiddenNeuronCount];                    // Input -> H0
        for (int l = 1; l < _hiddenLayerCount; l++)
            _W[l] = new float[_hiddenNeuronCount, _hiddenNeuronCount];         // Hi-1 -> Hi
        _W[_hiddenLayerCount] = new float[_hiddenNeuronCount, _outputCount];   // Hlast -> Out

        // Biases (flattened): hidden layers + output layer
        _biasOffsets = new int[_hiddenLayerCount + 1];
        int totalBias = 0;
        for (int l = 0; l < _hiddenLayerCount; l++)
        {
            _biasOffsets[l] = totalBias;
            totalBias += _hiddenNeuronCount;
        }
        _biasOffsets[_hiddenLayerCount] = totalBias;
        totalBias += _outputCount;

        _B = new float[totalBias];

        RandomizeParameters();
    }

    public void InitializeCopy(UtilityNNet other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        Initialize(other._inputCount, other._outputCount, other._hiddenLayerCount, other._hiddenNeuronCount);
        UseSoftmax = other.UseSoftmax;
        InitRange = other.InitRange;

        // Deep copy weights
        for (int l = 0; l < _W.Length; l++)
        {
            int r0 = _W[l].GetLength(0);
            int c0 = _W[l].GetLength(1);
            for (int r = 0; r < r0; r++)
                for (int c = 0; c < c0; c++)
                    _W[l][r, c] = other._W[l][r, c];
        }
        // Copy biases
        Array.Copy(other._B, _B, _B.Length);
    }

    public void RandomizeParameters()
    {
        // Weights
        for (int l = 0; l < _W.Length; l++)
        {
            int rows = _W[l].GetLength(0);
            int cols = _W[l].GetLength(1);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    _W[l][r, c] = UnityEngine.Random.Range(-InitRange, InitRange);
        }
        // Biases
        for (int i = 0; i < _B.Length; i++)
            _B[i] = UnityEngine.Random.Range(-InitRange, InitRange);
    }

    public float[] Run(float[] inputs)
    {
        if (inputs == null || inputs.Length != _inputCount)
            throw new ArgumentException($"Expected {_inputCount} inputs, got {(inputs == null ? -1 : inputs.Length)}");

        Array.Copy(inputs, _input, _inputCount);

        // Input -> Hidden[0]
        int h0 = _hiddenOffsets[0];
        int b0 = _biasOffsets[0];
        for (int j = 0; j < _hiddenNeuronCount; j++)
        {
            float sum = _B[b0 + j];
            for (int i = 0; i < _inputCount; i++)
                sum += _input[i] * _W[0][i, j];
            _hidden[h0 + j] = Tanh(sum);
        }

        // Hidden -> Hidden
        for (int l = 1; l < _hiddenLayerCount; l++)
        {
            int hPrev = _hiddenOffsets[l - 1];
            int hCur = _hiddenOffsets[l];
            int bCur = _biasOffsets[l];

            for (int j = 0; j < _hiddenNeuronCount; j++)
            {
                float sum = _B[bCur + j];
                for (int i = 0; i < _hiddenNeuronCount; i++)
                    sum += _hidden[hPrev + i] * _W[l][i, j];
                _hidden[hCur + j] = Tanh(sum);
            }
        }

        // Last hidden -> Output
        int outLayer = _hiddenLayerCount;
        int hLast = _hiddenOffsets[_hiddenLayerCount - 1];
        int bOut = _biasOffsets[outLayer];

        for (int j = 0; j < _outputCount; j++)
        {
            float sum = _B[bOut + j];
            for (int i = 0; i < _hiddenNeuronCount; i++)
                sum += _hidden[hLast + i] * _W[outLayer][i, j];
            _output[j] = sum; // raw logits
        }

        if (UseSoftmax)
            Softmax(_output);
        else
            for (int j = 0; j < _outputCount; j++)
                _output[j] = Sigmoid(_output[j]);

        return _output;
    }

    public void Mutate(float mutationChance, float mutationStrength)
    {
        // Weights
        for (int l = 0; l < _W.Length; l++)
        {
            int rows = _W[l].GetLength(0);
            int cols = _W[l].GetLength(1);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (UnityEngine.Random.value < mutationChance)
                        _W[l][r, c] += UnityEngine.Random.Range(-mutationStrength, mutationStrength);
        }
        // Biases
        for (int i = 0; i < _B.Length; i++)
            if (UnityEngine.Random.value < mutationChance)
                _B[i] += UnityEngine.Random.Range(-mutationStrength, mutationStrength);
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
            // Bias meta (sizes per layer) + flat data
            biasSizes = BuildBiasSizes(),
            B = (float[])_B.Clone()
        };

        // Weights -> DTO (flat)
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
            int k = 0;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    wDto.data[k++] = _W[l][r, c];
            dto.W[l] = wDto;
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

        // Validate bias sizes and copy
        int expectedTotalBias = 0;
        var expectedSizes = BuildBiasSizes();
        if (dto.biasSizes == null || dto.biasSizes.Length != expectedSizes.Length)
            throw new Exception("biasSizes missing or wrong length in JSON.");
        for (int i = 0; i < expectedSizes.Length; i++)
        {
            if (dto.biasSizes[i] != expectedSizes[i])
                throw new Exception($"biasSizes mismatch at layer {i}: expected {expectedSizes[i]}, got {dto.biasSizes[i]}");
            expectedTotalBias += expectedSizes[i];
        }
        if (dto.B == null || dto.B.Length != expectedTotalBias)
            throw new Exception($"Bias array length mismatch: expected {expectedTotalBias}, got {(dto.B == null ? -1 : dto.B.Length)}");

        Array.Copy(dto.B, _B, _B.Length);

        // Copy weights
        for (int l = 0; l < dto.W.Length; l++)
        {
            var w = dto.W[l];
            if (_W[l].GetLength(0) != w.rows || _W[l].GetLength(1) != w.cols)
                throw new Exception(
                    $"Weight shape mismatch at layer {l}: expected [{_W[l].GetLength(0)},{_W[l].GetLength(1)}], got [{w.rows},{w.cols}]"
                );
            int k = 0;
            for (int r = 0; r < w.rows; r++)
                for (int c = 0; c < w.cols; c++)
                    _W[l][r, c] = w.data[k++];
        }
    }

    public static UtilityNNet FromJson(string json)
    {
        var net = new UtilityNNet();
        net.LoadFromJson(json);
        return net;
    }

    public static UtilityNNet LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return FromJson(json);
    }

    // ---- Helpers ----
    private int[] BuildBiasSizes()
    {
        var sizes = new int[_hiddenLayerCount + 1];
        for (int l = 0; l < _hiddenLayerCount; l++)
            sizes[l] = _hiddenNeuronCount;
        sizes[_hiddenLayerCount] = _outputCount;
        return sizes;
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

    public WeightLayerDTO[] W;   // weights as flat per layer
    public int[] biasSizes;      // sizes per bias layer (hidden... + output)
    public float[] B;            // all biases flattened
}

[Serializable]
public class WeightLayerDTO
{
    public int rows;
    public int cols;
    public float[] data; // flat array length = rows * cols
}
