using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MetaAgentPlanner : MonoBehaviour
{
    [Header("Recipes")]
    [SerializeField] private CraftingRecipeSO enchantedStaffRecipe;
    [SerializeField] private CraftingRecipeSO runedShieldRecipe;
    [SerializeField] private CraftingRecipeSO connectArtifactsRecipe;

    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private Transform buildLocation;

    private bool staffBuilt = false;
    private bool shieldBuilt = false;

    [Header("Agents")]
    [SerializeField] private UtilityAIAgent mage;
    [SerializeField] private List<UtilityAIAgent> villagers;
    [SerializeField] private List<UtilityAIAgent> drones;
    //[SerializeField] private MageAgent mage;
    //[SerializeField] private List<VillagerAgent> villagers;
    //[SerializeField] private List<DroneAgent> drones;

    private Queue<GOAPAction> actionQueue = new();

    private void Start()
    {
        //create spawner and refennce to instancited here instead of find.
        //villagers = new List<VillagerAgent>(FindObjectsByType<VillagerAgent>(FindObjectsSortMode.None));
        //drones = new List<DroneAgent>(FindObjectsByType<DroneAgent>(FindObjectsSortMode.None));
        //mage = FindAnyObjectByType<MageAgent>();

        PlanNextGoal();
        if (actionQueue.Count > 0)
        {
            actionQueue.Peek().Execute();
        }
    }

    private void Update()
    {
        if (actionQueue.Count > 0)
        {
            if (actionQueue.Peek().CheckCompletion())
            {
                actionQueue.Dequeue();
            }
            if (actionQueue.Count > 0)
            {
                actionQueue.Peek().Execute();
            }
        }
        else
        {
            PlanNextGoal();
        }
    }

    private void PlanNextGoal()
    {
        if (!staffBuilt)
        {
            PlanArtifact(enchantedStaffRecipe, () => staffBuilt = true);
        }
        else if (!shieldBuilt)
        {
            PlanArtifact(runedShieldRecipe, () => shieldBuilt = true);
        }
        else
        {
            PlanConnectArtifacts();
        }
    }

    private void PlanArtifact(CraftingRecipeSO recipe, System.Action onComplete)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            //int needed = ingredient.amount - resourceManager.GetResourceCount(ingredient.resource.resourceName);
            //for (int i = 0; i < needed; i++)
            {
                if (ingredient.resource.resourceName == "Wood")
                    actionQueue.Enqueue(new GatherWoodAction(resourceManager, ingredient.amount));
                else if (ingredient.resource.resourceName == "Crystal")
                    actionQueue.Enqueue(new GatherCrystalAction(resourceManager, ingredient.amount));
                else
                    actionQueue.Enqueue(new GatherIronAction(resourceManager, ingredient.amount));

                //actionQueue.Enqueue(new DeliverToBuildSiteAction(buildLocation, drones, villagers));
            }
        }

        actionQueue.Enqueue(new BuildArtifactAction(recipe, mage, buildLocation, onComplete));
    }

    private void PlanConnectArtifacts()
    {
        actionQueue.Enqueue(new ConnectArtifactsAction(mage, buildLocation, connectArtifactsRecipe));
    }
}

public abstract class GOAPAction
{
    public abstract bool CanExecute();
    public abstract void Execute();
    public abstract bool CheckCompletion();
}

// Placeholder action implementations (real ones will require agent control)
public class GatherWoodAction : GOAPAction
{
    private ResourceManager manager;
    private int amount;
    private readonly string resourceName = "Wood";
    public GatherWoodAction(ResourceManager mgr, int amountToGather)
    {
        manager = mgr;
        amount = amountToGather;
    }

    public override bool CanExecute() => true;

    public override bool CheckCompletion()
    {
        if (manager.GetResourceCount(resourceName) >= amount)
        {
            GlobalUtilityValues.Instance.Deprioritize(resourceName);
            return true;
        }
        return false;
    }

    public override void Execute()
    {
        Debug.Log("Starting Meta Action: Gather Wood");
        GlobalUtilityValues.Instance.Prioritize(resourceName);
    }
}

public class GatherCrystalAction : GOAPAction
{
    private ResourceManager manager;
    private int amount;
    private readonly string resourceName = "Crystal";

    public GatherCrystalAction(ResourceManager mgr, int amountToGather)
    {
        manager = mgr;
        amount = amountToGather;
    }

    public override bool CanExecute() => true;

    public override bool CheckCompletion()
    {
        if (manager.GetResourceCount(resourceName) >= amount)
        {
            GlobalUtilityValues.Instance.Deprioritize(resourceName);
            return true;
        }
        return false;
    }

    public override void Execute()
    {
        Debug.Log("Starting Meta Action: Gather Crystal");
        GlobalUtilityValues.Instance.Prioritize(resourceName);
    }
}

public class GatherIronAction : GOAPAction
{
    private ResourceManager manager;
    private int amount;
    private readonly string resourceName = "Iron";

    public GatherIronAction(ResourceManager mgr, int amountToGather)
    {
        manager = mgr;
        amount = amountToGather;
    }

    public override bool CanExecute() => true;

    public override bool CheckCompletion()
    {
        if (manager.GetResourceCount(resourceName) >= amount)
        {
            GlobalUtilityValues.Instance.Deprioritize(resourceName);
            return true;
        }
        return false;
    }

    public override void Execute()
    {
        Debug.Log("Starting Meta Action: Gather Iron");

        GlobalUtilityValues.Instance.Prioritize(resourceName);
    }
}

//This could be called every time a resource is prepared for delivery
public class DeliverToBuildSiteAction : GOAPAction
{
    private Transform buildLocation;
    private List<UtilityAIAgent> drones;
    private List<UtilityAIAgent> villagers;
    public DeliverToBuildSiteAction(Transform buildLoc, List<UtilityAIAgent> dr, List<UtilityAIAgent> vl)
    {
        buildLocation = buildLoc;
        drones = dr;
        villagers = vl;
    }
    public override bool CanExecute() => true;

    public override bool CheckCompletion()
    {
        return true;
    }

    public override void Execute()
    {
        Debug.Log("Starting Meta Action: Deliver");

        UtilityAIAgent agent = null;
        foreach (UtilityAIAgent droneAgent in drones)
        {
            if (droneAgent.IsIdle) { agent = droneAgent; break; }
        }
        if (agent == null)
        {
            //get a nearby villager
            //agent.CarryResourceToBuild
        }
        else
        {
            //send drone
            //agent.FlyToPickupLocation
        }
    }
}

public class BuildArtifactAction : GOAPAction
{
    private CraftingRecipeSO recipe;
    private UtilityAIAgent mage;
    private Transform buildLoc;
    private System.Action onComplete;

    public BuildArtifactAction(CraftingRecipeSO r, UtilityAIAgent m, Transform b, System.Action onComplete)
    {
        recipe = r; mage = m; buildLoc = b; this.onComplete = onComplete;
    }
    public override bool CanExecute() => recipe.CanCraft();

    public override bool CheckCompletion()
    {
        Debug.Log("Checking if artifact is built: " + recipe.recipeName);
        var result = ResourceManager.Instance.GetResourceCount(recipe.recipeName) >= 1;
        if (result) onComplete?.Invoke();
        else Debug.LogWarning("Artifact not built yet: " + recipe.recipeName);
        return result;
    }

    public override void Execute()
    {
        Debug.Log("Starting Meta Action: Build Artifact");
        CraftPriorityConsideration.Priorities[recipe] = true;
    }
}

public class ConnectArtifactsAction : GOAPAction
{
    private UtilityAIAgent mage;
    private Transform buildLoc;
    private CraftingRecipeSO recipe;
    public ConnectArtifactsAction(UtilityAIAgent m, Transform b, CraftingRecipeSO _recipe) { mage = m; buildLoc = b; recipe = _recipe; }
    public override bool CanExecute() => true;

    public override bool CheckCompletion()
    {
        //if(//artifact is built)
        //{
        //    return true;
        //}
        return false;

    }

    public override void Execute()
    {
        Debug.Log("Starting Meta Action: Connect Artifact");

        //make the mage want to connect the artifacts
        //mage.ConnectArtifacts();
        CraftPriorityConsideration.Priorities[recipe] = true;
    }
}

//public class MageAgent : UtilityAIAgent { }
//public class VillagerAgent : UtilityAIAgent { }
//public class DroneAgent : UtilityAIAgent { }
