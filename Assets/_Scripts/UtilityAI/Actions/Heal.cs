using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/Heal")]
public class Heal : AIAction
{
    [SerializeField, Min(1)] int healAmount = 20;

    Health healComponent;

    public override void Initialize(Context context)
    {
        base.Initialize(context);
        context.brain.TryGetComponent(out healComponent);
        Debug.Log($"<color=white>Heal action initialized for {context.brain.name} with heal amount: {healAmount}</color>");
    }

    public override void Execute(Context context)
    {
        if(!healComponent)
        {
            Debug.LogWarning($"<color=orange>No Health component found on {context.brain.name}</color>");
            return;
        }
        healComponent.Heal(healAmount);
        Debug.Log($"<color=green>{context.brain.name} healed for {healAmount} points.</color>");
    }
}
