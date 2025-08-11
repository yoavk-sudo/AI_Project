using System;
using UnityEngine;
[CreateAssetMenu(fileName = "ResourcePriorityConsideration", menuName = "UtilityAI/Considerations/Resource Priority Consideration")]
public class ResourcePriorityConsideration : Consideration
{
    [SerializeField] SourceSO CraftingIngredientSO;
    public override float Evaluate(Context context)
    {
        return (CraftingIngredientSO.resourceName) switch
        {

            "Wood" => GlobalUtilityValues.Instance.Wood,
            "Iron" => GlobalUtilityValues.Instance.Iron,
            "Crystal" => GlobalUtilityValues.Instance.Crystal,
            _ => throw new ArgumentException($"Can't find resource with name {CraftingIngredientSO.resourceName}"),
        };
    }
}

