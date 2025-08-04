using UnityEngine;
public class GatherResourceAction : UtilityActionBase
{
    public override float GetScore()
    {
        // Example: return a random score or base it on agent state
        return Random.Range(0f, 1f);
    }

    public override void Execute()
    {
        Debug.Log("Gathering resource!");
    }
}
