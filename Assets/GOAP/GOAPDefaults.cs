using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GOAPDefaults
{
    public static int watchdog = 5000;

    public static WorldState DefaultWorldState = new WorldState()
    {
        coin = 100,
        alive = true,
        hasEscaped = false,
        brokenCabinet = false,
        angryOwner = false,
        equipment = "none",
        playDarts = false,
    };

    public static float Heuristic(WorldState current)
    {
        return current.steps 
            + (current.hasEscaped == true ? 0 : 5);
    }

    public static IEnumerable<AStar<WorldState>.Arc> Expand(WorldState current, IEnumerable<GOAPAction> actions)
    {
        if (watchdog == 0) return Enumerable.Empty<AStar<WorldState>.Arc>();
        else watchdog--;

        return actions
            .Where(action => action.preconditions.All(condition => condition(current)))
            .Aggregate(new List<AStar<WorldState>.Arc>(), (possibleList, action) =>
            {
                var newState = current.Clone();
                newState.steps++;
                action.effects.ForEach(applyEffect => applyEffect(newState));
                newState.generatingAction = action;
                possibleList.Add(new AStar<WorldState>.Arc(newState, action.cost));

                return possibleList;
            });
    }

    // Setp 1
    public static GOAPAction ActionStealKey = new GOAPAction("Steal key")
            .AddCost(1f)
            .AddPrecondition(wrld => wrld.equipment == Equipment.None)

            .AddEffect(wrld => { wrld.equipment = Equipment.Key; return wrld; })
            .AddEffect(wrld => { wrld.angryOwner = true; return wrld; });

    public static GOAPAction ActionGetBreakingObject = new GOAPAction("Get breaking object")
            .AddCost(1f)
            .AddPrecondition(wrld => wrld.coin >= 70)
            .AddPrecondition(wrld => wrld.equipment == Equipment.None)

            .AddEffect(wrld => { wrld.coin = wrld.coin - 70; return wrld; })
            .AddEffect(wrld => { wrld.equipment = Equipment.BreakingObject; return wrld; });

    public static GOAPAction ActionPayForGrog = new GOAPAction("Pay for grog")
            .AddCost(2f)
            .AddPrecondition(wrld => wrld.equipment == Equipment.None)
            .AddPrecondition(wrld => wrld.coin >= 10)

            .AddEffect(wrld => { wrld.coin = wrld.coin - 10; return wrld; })
            .AddEffect(wrld => { wrld.drunkenness += 0.2f; return wrld; });

    public static GOAPAction ActionChallengeOwner = new GOAPAction("Challenge owner")
            .AddCost(4f)
            .AddPrecondition(wrld => wrld.equipment == Equipment.None)
            .AddEffect(wrld => { wrld.equipment = Equipment.Darts; return wrld; });


    public static GOAPAction ActionPlayDarts = new GOAPAction("Play darts")
            .AddCost(1f)
            .AddPrecondition(wrld => wrld.equipment == Equipment.Darts)
            .AddPrecondition(wrld => wrld.drunkenness >= 0)

            .AddEffect(wrld => { wrld.equipment = Equipment.None; return wrld; })
            .AddEffect(wrld => { wrld.playDarts = true; return wrld; });

    public static GOAPAction ActionStealKeyWithoutAngryOwner = new GOAPAction("Steal key without angry owner")
           .AddCost(1f)
           .AddPrecondition(wrld => wrld.equipment == Equipment.None)
           .AddPrecondition(wrld => wrld.playDarts == true)

           .AddEffect(wrld => { wrld.equipment = Equipment.Key; return wrld; });


    // Step 2
    public static GOAPAction ActionOpenCabinet = new GOAPAction("Open cabinet")
            .AddCost(1f)
            .AddPrecondition(wrld => wrld.equipment == Equipment.Key)

            .AddEffect(wrld => { wrld.equipment = Equipment.None; return wrld; })
            .AddEffect(wrld => { wrld.cabinetOpen = true; return wrld; });

    public static GOAPAction ActionBreakCabinet = new GOAPAction("Break cabinet")
            .AddCost(1f)
            .AddPrecondition(wrld => wrld.equipment == Equipment.BreakingObject)

            .AddEffect(wrld => { wrld.equipment = Equipment.None; return wrld; })
            .AddEffect(wrld => { wrld.brokenCabinet = true; return wrld; })
            .AddEffect(wrld => { wrld.cabinetOpen = true; return wrld; });



    // Step 3
    public static GOAPAction ActionStealElfBlood = new GOAPAction("Steal elf blood")
            .AddCost(1f)
            .AddPrecondition(wrld => wrld.cabinetOpen == true)

            .AddEffect(wrld => { wrld.cabinetOpen = false; return wrld; })
            .AddEffect(wrld => { wrld.equipment = Equipment.ElfBlood; return wrld; });

    public static GOAPAction ActionStealCabinet = new GOAPAction("Steal cabinet")
            .AddCost(5f)
            .AddPrecondition(wrld => wrld.equipment == Equipment.None)
            .AddPrecondition(wrld => wrld.playDarts == false)

            .AddEffect(wrld => { wrld.alive = false; return wrld; })
            .AddEffect(wrld => { wrld.equipment = Equipment.Cabinet; return wrld; });

    public static GOAPAction ActionStealCabinetWithoutDying = new GOAPAction("Steal cabinet without dying")
            .AddCost(3f)
            .AddPrecondition(wrld => wrld.drunkenness >= 0.3f)
            .AddPrecondition(wrld => wrld.equipment == Equipment.None)

            .AddEffect(wrld => { wrld.equipment = Equipment.Cabinet; return wrld; });


    // Step 4
    public static GOAPAction ActionEscape = new GOAPAction("Escape")
            .AddCost(1f)
            .AddPrecondition(wrld => wrld.equipment == Equipment.ElfBlood || wrld.equipment == Equipment.Cabinet)

            .AddEffect(wrld => { wrld.hasEscaped = true; return wrld; });


    public static List<GOAPAction> ActionList = new List<GOAPAction>()
    {
        ActionStealKey,
        ActionStealKeyWithoutAngryOwner,
        ActionOpenCabinet,
        ActionGetBreakingObject,
        ActionChallengeOwner,
        ActionBreakCabinet,
        ActionPlayDarts,
        ActionStealElfBlood,
        ActionStealCabinet,
        ActionEscape,
        ActionPayForGrog,
        ActionStealCabinetWithoutDying,
    };  
}

public static class Equipment
{
    public static string ElfBlood = "Elf blood";
    public static string Key = "key";
    public static string Cabinet = "cabinet";
    public static string None = "none";
    public static string BreakingObject = "breaking object";
    public static string Darts = "darts";
}
