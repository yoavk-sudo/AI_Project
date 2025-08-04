using UnityEngine;
public class GatherResourceAction : UtilityActionBase
{

    public override float GetScore()
    {
        // Example: Score is higher if wood is low in the global resource manager
        int woodCount = ResourceManager.Instance.GetResourceCount("Wood");
        if (woodCount < 10)
            return 1.0f; // High priority
        if (woodCount < 20)
            return 0.5f; // Medium priority
        return 0.1f; // Low priority
    }

    public override void Execute()
    {
        Debug.Log("Gathering resource!");
    }
}
