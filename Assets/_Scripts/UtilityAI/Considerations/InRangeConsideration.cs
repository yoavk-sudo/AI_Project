using UnityEngine;
using UnityEngine.InputSystem;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "UtilityAI/Considerations/InRangeConsideration")]
    public class InRangeConsideration : Consideration
    {
        public float maxDistance = 10f;
        public float maxAngle = 360f;
        public string targetTag = "Target";
        public AnimationCurve curve;

        public override float Evaluate(Context context)
        {

            context.sensor.AddTag(targetTag);


            Transform targetTransform = context.sensor.GetClosestTarget(targetTag);
            if (targetTransform == null) return 0f;
            Vector3 targetPosY0 = targetTransform.position;

            Transform agentTransform = context.agent.transform;
            Vector3 agentPosY0 = agentTransform.position;
            targetPosY0.y = 0;
            agentPosY0.y = 0;
            bool isInRange = Vector3.Distance(agentPosY0, targetPosY0) <= maxDistance;//is in max range
            if (!isInRange) return 0f;

            Vector3 directionToTarget = targetPosY0 - agentPosY0;// Ignore vertical distance
            float distanceToTarget = directionToTarget.magnitude;

            float normalizedDistance = Mathf.Clamp01(distanceToTarget / maxDistance);

            float utility = curve.Evaluate(normalizedDistance);
            return Mathf.Clamp01(utility);
        }

        void Reset()
        {
            curve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f)
            );
        }
    }
}