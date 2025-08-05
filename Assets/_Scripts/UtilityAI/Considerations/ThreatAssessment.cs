using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Considerations/ThreatAssessment")]
public class ThreatAssessment : Consideration
{
    [SerializeField] int threatThreshold = 3;
    public override float Evaluate(Context context)
    {
        if(context.sensor.detectedObjects.Count > threatThreshold)
        {
            Debug.Log($"<color=red>Threat detected: {context.sensor.detectedObjects.Count} objects</color>");
            return 1;
        }
        return 0;
    }
}
