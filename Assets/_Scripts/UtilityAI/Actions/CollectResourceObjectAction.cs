using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/CollectResourceObjectAction")]
public class CollectResourceObjectAction : AIAction
{
    public static Dictionary<UtilityAIAgent, ResourceHandler> AgentResourcePairs = new();
    public override void Initialize(Context context)
    {
        context.sensor.AddTag(targetTag);
        base.Initialize(context);
    }
    public override float CalculateUtility(Context context)
    {
        if (AgentResourcePairs.TryGetValue(context.brain, out var res) && res)
        {
            return 0;
        }
        return base.CalculateUtility(context);
    }
    public override void Execute(Context context)
    {
        var target = context.sensor.GetClosestTarget(targetTag);
        if (!target) return;
        var res = target.GetComponent<ResourceHandler>();
        if (!res)
        {
            PerformActionOverTime(context, 2, null);
            return;
        }
        AgentResourcePairs[context.brain] = res;
        context.target = target;
        target.SetParent(context.brain.transform);
    }
}
