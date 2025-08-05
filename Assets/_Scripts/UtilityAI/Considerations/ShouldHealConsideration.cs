using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Considerations/HealConsideration")]
public class ShouldHealConsideration : Consideration
{
    [SerializeField] int healthThreshold = 10;
    public override float Evaluate(Context context)
    {
        if(context.brain.TryGetComponent(out Health healthComponent))
        {
            if (healthComponent.HP <= healthThreshold)
                return 1f;
        }
        return 0;
    }
}
