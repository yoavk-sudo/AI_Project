using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Considerations/ResourcesAvailable")]
public class ResourcesAvailableConsideration : Consideration
{
    [SerializeField] List<CraftingIngredientSO> resources;

    public override float Evaluate(Context context)
    {
        if(!ResourceManager.Instance) return 0f; // If ResourceManager is not available, return 0 utility
        foreach (var resource in resources)
        {
            var amountInResources = ResourceManager.Instance.GetResourceCount(resource.resource.resourceName);
            if (resource.amount > amountInResources)
            {
                return 0f; // If any resource is not available, return 0 utility
            }
        }
        return 1;
    }
}
