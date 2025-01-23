﻿using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RIMSPR;

namespace RIMSPR;

public class WorkGiver_HaulNutrientsToRIMSPRLab : WorkGiver_Scanner
{
    //public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.SubcoreScanner);
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(RIMSPR_DefOfs.RimsprLab);
    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!ModLister.CheckBiotech("Haul to subcore scanner"))
        {
            return false;
        }
        if (t.IsForbidden(pawn))
        {
            return false;
        }
        if (!(t is Building_RimsprLab building_RimsprLab) || building_RimsprLab.State != RIMSPERLabState.WaitingForNutrients)
        {
            return false;
        }
        if (!pawn.CanReserve(t, 1, -1, null, forced))
        {
            return false;
        }
        return FindIngredients(pawn, building_RimsprLab).Thing != null;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!(t is Building_RimsprLab building_RimsprLab) || building_RimsprLab.State != RIMSPERLabState.WaitingForNutrients)
        {
            return null;
        }
        ThingCount thingCount = FindIngredients(pawn, building_RimsprLab);
        if (thingCount.Thing != null)
        {
            Job job = HaulAIUtility.HaulToContainerJob(pawn, thingCount.Thing, t);
            job.count = Mathf.Min(job.count, thingCount.Count);
            return job;
        }
        return null;
    }

    private ThingCount FindIngredients(Pawn pawn, Building_RimsprLab lab)
    {
        Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
        if (thing == null)
        {
            return default(ThingCount);
        }
        int b = Mathf.CeilToInt(lab.GetRequiredNutrition() / thing.GetStatValue(StatDefOf.Nutrition));
        return new ThingCount(thing, Mathf.Min(thing.stackCount, b));
        bool Validator(Thing x)
        {
            if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
            {
                return false;
            }
            if (!lab.PassNutritionFilter(x))
            {
                return false;
            }
            return lab.CanAcceptNutrition();
        }
    }
}
