using UnityEngine;

public abstract class AIAction : ScriptableObject {
    public string targetTag;
    public Consideration consideration;

    public virtual void Initialize(Context context) {
        // Optional initialization logic
    }
        
    public virtual float CalculateUtility(Context context) => consideration.Evaluate(context);
        
    public abstract void Execute(Context context);
}
