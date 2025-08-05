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
        context.sensor.AddTag(targetTag);
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
        if (target == null || (!target.parent.TryGetComponent<RenewableResourceHandler>(out var handler) || !handler.readyToCollect))
        {
            PerformActionOverTime(context, timeToCollect, null);
            return;
        }
        context.target = target;
        PerformActionOverTime(context, timeToCollect, () => GainResources(target));
    }

    private void GainResources(Transform target)
    {
        if (target.parent.TryGetComponent<RenewableResourceHandler>(out var resourceHandler))
        {
            resourceHandler.CreateResource();
            Debug.Log($"<color=green>Renewable resource collected from {target.name}</color>");
            return; // If it's a renewable resource, handle it and exit
        }
    }
}
