using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/CollectResourceAction")]
public class CollectResourceAction : AIAction
{
    public override void Initialize(Context context)
    {
        context.sensor.targetTags.Add(targetTag);
    }
    public override void Execute(Context context)
    {
        var target = context.sensor.GetClosestTarget(targetTag);
        if (target == null) return;
        context.target = target;
        Debug.Log($"Collecting resource from {target.name}");
    }
}
