using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/Attack")]
public class Attack : AIAction
{
    [SerializeField] int damageAmount = 15;
    [SerializeField] float attackCooldown = 1.5f;

    float timer = 0f;

    public override void Initialize(Context context)
    {
        base.Initialize(context);
        timer = attackCooldown;
    }

    public override void Execute(Context context)
    {
        if (timer < attackCooldown)
        {
            timer += Time.deltaTime;
            return; // Wait for cooldown
        }
        timer = 0;
        var target = context.sensor.GetClosestTarget(targetTag);
        Debug.Log($"<color=orange>Executing Attack action on target: {target?.name}</color>");
        if (target && target.TryGetComponent<Health>(out var targetHealth))
        {
            targetHealth.TakeDamage(damageAmount);
            Debug.Log($"<color=red>Attacked {target.name} for {damageAmount} damage</color>");
        }
        else if(target && target.parent.TryGetComponent(out Health targetHP))
        {
            targetHP.TakeDamage(damageAmount);
            Debug.Log($"<color=red>Attacked {target.parent.name} for {damageAmount} damage</color>");
        }
        else
        {
            Debug.LogWarning($"<color=orange>No valid target found for attack</color>");
        }
    }
}
