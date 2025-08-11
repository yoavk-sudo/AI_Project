using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/Craft")]
public class CraftArtifact : AIAction
{
    [SerializeField] CraftingRecipeSO recipe;
    [SerializeField] GameObject artifactPrefab;
    public override void Execute(Context context)
    {
        foreach (var item in recipe.ingredients)
        {
            ResourceManager.Instance.UseResourceAmount(item.resource.resourceName, item.amount);
        }
        Instantiate(artifactPrefab, ResourceManager.Instance.transform.position + Vector3.up * 0.5f, Quaternion.identity);
        ResourceManager.Instance.GainResourceAmount(recipe.recipeName, 1);
    }
}
