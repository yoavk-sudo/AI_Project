using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private Dictionary<string, int> _resourceCounts = new Dictionary<string, int>();
    public static ResourceManager Instance { get; private set; }
    public Action<string, int> UpdateResourceEvent;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _resourceCounts.Add("Wood", 0);
        _resourceCounts.Add("Crystal", 0);
        _resourceCounts.Add("Iron", 0);
        _resourceCounts.Add("Shield", 0);
        _resourceCounts.Add("Staff", 0);
    }

    public int GetResourceCount(string resourceName)
    {
        if (!_resourceCounts.ContainsKey(resourceName))
        {
            Debug.Log("<color=red>no resource exist called</color>" + resourceName);
            return 0; // Initialize if not present
        }
        return _resourceCounts[resourceName];
    }

    public void UseResourceAmount(string resourceName,int usedAmount)
    {
        _resourceCounts[resourceName] -= usedAmount;
        if (_resourceCounts[resourceName] < 0)
        {
            _resourceCounts[resourceName] = 0; // Prevent negative counts
        }
        UpdateResourceEvent?.Invoke(resourceName, _resourceCounts[resourceName]);
    }

    public void GainResourceAmount(string resourceName, int gainedAmount)
    {
        if (_resourceCounts.TryGetValue(resourceName, out int currentCount))
        {
            _resourceCounts[resourceName] = currentCount + gainedAmount;
        }
        else
        {
            _resourceCounts[resourceName] = gainedAmount; // Initialize if not present
        }
        UpdateResourceEvent?.Invoke(resourceName, _resourceCounts[resourceName]);
    }
    //dictionary to hold resource counts
}