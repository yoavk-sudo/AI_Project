using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/Retreat")]
public class RetreatAction : AIAction
{
    [SerializeField] float retreatDistance = 15f;

    Vector3 retreatPosition;

    public override void Initialize(Context context)
    {
        base.Initialize(context);
        retreatPosition = new Vector3(-retreatDistance, 0, 0);
    }

    public override void Execute(Context context)
    {
        context.agent.SetDestination(context.agent.transform.position - retreatPosition);
    }
}
