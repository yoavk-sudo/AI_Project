
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftPriorityConsideration", menuName = "UtilityAI/Considerations/Craft Priority Consideration")]
internal class CraftPriorityConsideration : Consideration
{
    public static Dictionary<CraftingRecipeSO, bool> Priorities = new();
    [SerializeField] CraftingRecipeSO recipe;

    public override float Evaluate(Context context)
    {
        if (Priorities.TryGetValue(recipe, out var res) && res) return 1;
        else if (!Priorities.ContainsKey(recipe))
        {
            Priorities.Add(recipe, false);
        }
        return 0;
    }
}
