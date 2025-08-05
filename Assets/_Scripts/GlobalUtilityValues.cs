using System.Collections.Generic;
using UnityEngine;

public class GlobalUtilityValues : MonoBehaviour
{
    public static GlobalUtilityValues Instance { get; private set; }

    private Dictionary<string, int> resourceValuePairs = new Dictionary<string, int>();

    public int Wood { get { return resourceValuePairs["Wood"]; } }
    public int Crystal { get { return resourceValuePairs["Crystal"]; } }
    public int Iron { get { return resourceValuePairs["Iron"]; } }

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

        resourceValuePairs.Add("Wood", 1);
        resourceValuePairs.Add("Crystal", 1);
        resourceValuePairs.Add("Iron", 1);
    }

    public void Prioritize(string resourceName)
    {
        resourceValuePairs[resourceName] = 1;
    }

    public void Deprioritize(string resourceName)
    {
        resourceValuePairs[resourceName] = 0;
    }
}
