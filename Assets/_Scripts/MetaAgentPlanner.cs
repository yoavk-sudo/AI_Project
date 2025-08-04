using System.Collections.Generic;
using UnityEngine;

public class MetaAgentPlanner : MonoBehaviour
{
    [Header("Recipes")]
    [SerializeField] private CraftingRecipeSO enchantedStaffRecipe;
    [SerializeField] private CraftingRecipeSO runedShieldRecipe;

    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private Transform buildLocation;

    private bool staffBuilt = false;
    private bool shieldBuilt = false;

    [SerializeField] private MageAgent mage;
    [SerializeField] private List<VillagerAgent> villagers;
    [SerializeField] private List<DroneAgent> drones;

    private Queue<GOAPAction> actionQueue = new();

    private void Start()
    {
        villagers = new List<VillagerAgent>(FindObjectsByType<VillagerAgent>(FindObjectsSortMode.None));
        drones = new List<DroneAgent>(FindObjectsByType<DroneAgent>(FindObjectsSortMode.None));
        mage = FindAnyObjectByType<MageAgent>();

        PlanNextGoal();
    }

    private void Update()
    {
        if (actionQueue.Count > 0)
        {
            var action = actionQueue.Peek();
            if (action.CanExecute())
            {
                action.Execute();
                actionQueue.Dequeue();
            }
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
            int needed = ingredient.amount - resourceManager.GetResourceCount(ingredient.resource.resourceName);
            for (int i = 0; i < needed; i++)
            {
                if (ingredient.resource.resourceName == "Oak Logs")
                    actionQueue.Enqueue(new ChopTreeAction(resourceManager));
                else if (ingredient.resource.resourceName == "Crystal Shards")
                    actionQueue.Enqueue(new RefineCrystalAction(resourceManager));
                else
                    actionQueue.Enqueue(new CollectIronAction(resourceManager));

                actionQueue.Enqueue(new DeliverToBuildSiteAction(buildLocation, drones, villagers));
            }
        }

        actionQueue.Enqueue(new BuildArtifactAction(recipe, mage, buildLocation, onComplete));
    }

    private void PlanConnectArtifacts()
    {
        actionQueue.Enqueue(new ConnectArtifactsAction(mage, buildLocation));
    }
}

public abstract class GOAPAction
{
    public abstract bool CanExecute();
    public abstract void Execute();
}

// Placeholder action implementations (real ones will require agent control)
public class ChopTreeAction : GOAPAction
{
    private ResourceManager manager;
    public ChopTreeAction(ResourceManager mgr) => manager = mgr;
    public override bool CanExecute() => true;
    public override void Execute()
    {
        
    }
}

public class RefineCrystalAction : GOAPAction
{
    private ResourceManager manager;
    public RefineCrystalAction(ResourceManager mgr) => manager = mgr;
    public override bool CanExecute() => true;
    public override void Execute()
    {

    }
}

public class CollectIronAction : GOAPAction
{
    private ResourceManager manager;
    public CollectIronAction(ResourceManager mgr) => manager = mgr;
    public override bool CanExecute() => true;
    public override void Execute()
    {

    }
}

//This should be called every time a resource is prepared for delivery
public class DeliverToBuildSiteAction : GOAPAction
{
    private Transform buildLocation;
    private List<DroneAgent> drones;
    private List<VillagerAgent> villagers;
    public DeliverToBuildSiteAction(Transform buildLoc, List<DroneAgent> dr, List<VillagerAgent> vl)
    {
        buildLocation = buildLoc;
        drones = dr;
        villagers = vl;
    }
    public override bool CanExecute() => true;
    public override void Execute()
    {
        UtilityAIAgent agent = null;
        foreach(DroneAgent droneAgent in drones)
        {
            if(droneAgent.IsIdle) { agent = droneAgent; break; }
        }
        if(agent == null)
        {
            //get a nearby villager
            //agent.CarryResourceToBuild
        }
        else
        {
            //activate drone
            //agent.FlyToPickupLocation
        }
    }
}

public class BuildArtifactAction : GOAPAction
{
    private CraftingRecipeSO recipe;
    private MageAgent mage;
    private Transform buildLoc;
    private System.Action onComplete;

    public BuildArtifactAction(CraftingRecipeSO r, MageAgent m, Transform b, System.Action onComplete)
    {
        recipe = r; mage = m; buildLoc = b; this.onComplete = onComplete;
    }
    public override bool CanExecute() => true;
    public override void Execute()
    {
        //mage.BuildArtifact(recipe);
        onComplete?.Invoke();
    }
}

public class ConnectArtifactsAction : GOAPAction
{
    private MageAgent mage;
    private Transform buildLoc;
    public ConnectArtifactsAction(MageAgent m, Transform b) { mage = m; buildLoc = b; }
    public override bool CanExecute() => true;
    public override void Execute()
    {
        //mage.ConnectArtifacts();
    }
}

public class MageAgent : UtilityAIAgent { }
public class VillagerAgent : UtilityAIAgent { }
public class DroneAgent : UtilityAIAgent { }
