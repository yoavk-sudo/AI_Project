
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/DeliverResourceAction")]
public class DeliverResource : MoveToTargetAIAction
{
    public static List<ResourceHandler> DeliveredResources = new();
    public override float CalculateUtility(Context context)
    {
        if (CollectResourceObjectAction.AgentResourcePairs.TryGetValue(context.brain, out var res) && res)
        {
            if (DeliveredResources.Contains(res))
            {
                return 0; // Already delivered this resource
            }
            return 1;
        }
        return 0;
    }
    public override void Execute(Context context)
    {
        base.Execute(context);
        if (Vector3.Distance(context.brain.transform.position, context.target.transform.position) < 5)
        {
            var obj = CollectResourceObjectAction.AgentResourcePairs[context.brain];
            obj.transform.SetParent(null);
            DeliveredResources.Add(obj);
            obj.tag = "Untagged";
            DeliveredResources.RemoveAll(r => r == null);
            CollectResourceObjectAction.AgentResourcePairs.Remove(context.brain);
        }
    }
}
