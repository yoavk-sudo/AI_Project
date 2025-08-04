using System.Collections.Generic;
using UnityEngine;

public class UtilityAIAgent : MonoBehaviour
{
    public List<UtilityActionBase> actions = new List<UtilityActionBase>();

    void Update()
    {
        UtilityActionBase bestAction = null;
        float bestScore = float.MinValue;

        foreach (var action in actions)
        {
            float score = action.GetScore();
            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        if (bestAction != null)
        {
            bestAction.Execute();
        }
    }
}
