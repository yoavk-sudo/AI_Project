using TMPro;
using UnityEngine;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] ResourceManager resourceManager;
    [SerializeField] TextMeshProUGUI woodCount;
    [SerializeField] TextMeshProUGUI ironCount;
    [SerializeField] TextMeshProUGUI crystalCount;
    [SerializeField] TextMeshProUGUI shieldStatus;
    [SerializeField] TextMeshProUGUI staffStatus;
    [SerializeField] TextMeshProUGUI combinedArtifactStatus;

    private void OnEnable()
    {
        resourceManager.UpdateResourceEvent += UpdateResourceCount;
    }

    void UpdateResourceCount(string resource, int newAmount)
    {
        switch (resource)
        {
            case "Wood":
                woodCount.text = newAmount.ToString();
                break;
            case "Iron":
                ironCount.text = newAmount.ToString();
                break;
            case "Crystal":
                crystalCount.text = newAmount.ToString();
                break;
            case "Shield":
                shieldStatus.text = newAmount == 0 ? "Not ready" : "Ready";
                break;
            case "Staff":
                staffStatus.text = newAmount == 0 ? "Not ready" : "Ready";
                break;
            case "UltimateArtifact":
                combinedArtifactStatus.text = newAmount == 0 ? "Not ready" : "Ready";
                break;
            default:
                Debug.Log("unknown resource " + resource);
                break;
        }
    }
}
