using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private Dictionary<string, int> _resourceCounts = new Dictionary<string, int>();
    public static ResourceManager Instance { get; private set; }
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
    }
    public int GetResourceCount(string resourceName)
    {
        if (!_resourceCounts.ContainsKey(resourceName))
        {
            Debug.Log("<color=red>no resource exist</color>");
            return 0; // Initialize if not present
        }
        return _resourceCounts[resourceName];
    }
    public void UseResourceCount(string resourceName,int usedAmount)
    {
        _resourceCounts[resourceName] -= usedAmount;
        if (_resourceCounts[resourceName] < 0)
        {
            _resourceCounts[resourceName] = 0; // Prevent negative counts
        }
    }
    //dictionary to hold resource counts
}