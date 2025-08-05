using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Scriptable Objects/CraftingRecipe")]
public class CraftingRecipeSO : ScriptableObject
{
    public string recipeName;
    public List<CraftingIngredientSO> ingredients;
    public int resultAmount = 1;
    public bool CanCraft()
    {
        foreach (var ingredient in ingredients)
        {
            if (ingredient.amount <= ResourceManager.Instance.GetResourceCount(ingredient.resource.resourceName)) return false;
        }
        return true;
    }
}
