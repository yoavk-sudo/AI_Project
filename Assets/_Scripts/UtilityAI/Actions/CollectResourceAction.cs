using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/CollectResourceAction")]
public class CollectResourceAction : AIAction
{
    [SerializeField] float timeToCollect;
    public override void Initialize(Context context)
    {
        context.sensor.targetTags.Add(targetTag);
        if (!isCollecting.ContainsKey((context.brain, this)))
        {
            isCollecting[(context.brain, this)] = true; // Initialize collecting state
        }
    }

    public override float CalculateUtility(Context context)
    {
        if (isCollecting.TryGetValue((context.brain, this), out bool collecting) && !collecting)
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
        PerformActionOverTime(context, timeToCollect, GainResources);
    }

    private void GainResources()
    {
        if(!ResourceManager.Instance)
        {
            return;
        }
        ResourceManager.Instance.GainResourceAmount(targetTag, 1);
    }
}
