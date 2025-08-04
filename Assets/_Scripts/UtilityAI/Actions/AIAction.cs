using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIAction : ScriptableObject {
    public string targetTag;
    public Consideration consideration;
    public static Dictionary<(UtilityAIAgent, AIAction), bool> isCollecting = new();

    public virtual void Initialize(Context context) {
        // Optional initialization logic
    }
        
    public virtual float CalculateUtility(Context context) => consideration.Evaluate(context);
        
    public abstract void Execute(Context context);

    public IEnumerator DisableAction(Context context, float time, Action callback)
    {
        isCollecting[(context.brain, this)] = false;
        Debug.Log($"<color=green>{context.brain.name} is collecting resources...</color>");
        yield return new WaitForSeconds(time); // Simulate collection time
        callback?.Invoke();
        isCollecting[(context.brain, this)] = true;
    }

    public void PerformActionOverTime(Context context, float time, Action callback)
    {
        context.brain.StartCoroutine(DisableAction(context, time, callback));
    }
}
