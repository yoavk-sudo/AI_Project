using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/CollectResourceAction")]
public class CollectResourceAction : AIAction
{
    public static Dictionary<UtilityAIAgent, bool> isCollecting = new Dictionary<UtilityAIAgent, bool>();
    public override void Initialize(Context context)
    {
        context.sensor.targetTags.Add(targetTag);
        if (!isCollecting.ContainsKey(context.brain))
        {
            isCollecting[context.brain] = true; // Initialize collecting state
        }
    }

    public override float CalculateUtility(Context context)
    {
        if (isCollecting.TryGetValue(context.brain, out bool collecting) && !collecting)
        {
            return 0f; // If not collecting, return 0 utility
        }
        return base.CalculateUtility(context);
    }

    public override void Execute(Context context)
    {
        var target = context.sensor.GetClosestTarget(targetTag);
        if (target == null) return;
        context.target = target;
        context.brain.StartCoroutine(DisableAction(context));
    }

    public IEnumerator DisableAction(Context context)
    {
        isCollecting[context.brain] = false;
        Debug.Log($"<color=green>{context.brain.name} is collecting resources...</color>");
        yield return new WaitForSeconds(2f); // Simulate collection time
        isCollecting[context.brain] = true;
    }
}
