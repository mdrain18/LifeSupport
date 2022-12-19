using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace LifeSupport.Classes
{
    public static class LifeSupportUtility
    {
        public static bool ValidLifeSupportNearby(this Pawn pawn)
        {
            return pawn.CurrentBed() is Building_Bed bed &&
                   GenAdj.CellsAdjacent8Way(bed).Any(cell =>
                   {
                       return cell.GetThingList(bed.Map).Any(cellThing =>
                       {
                           if (cellThing.TryGetComp<LifeSupportComp>() is LifeSupportComp lifeSupport)
                               return lifeSupport.Active;
                           return false;
                       });
                   });
        }

        internal static bool WouldDieWithoutLifeSupport(this Pawn pawn)
        {
            var capacitiesHandler = pawn.health.capacities;
            var isFlesh = pawn.RaceProps.IsFlesh;
            foreach (var pawnCapacityDef in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
                if (isFlesh ? !pawnCapacityDef.lethalFlesh : !pawnCapacityDef.lethalMechanoids)
                {
                    // not deadly
                }
                else if (!capacitiesHandler.CapableOf(pawnCapacityDef))
                {
                    return true;
                }

            return false;
        }

        public static void SetHediffs(this Pawn pawn)
        {
            var validLifeSupportNearby = pawn.ValidLifeSupportNearby();
            pawn.SetHediffs(validLifeSupportNearby);
        }

        public static void SetHediffs(this Pawn pawn, bool validLifeSupportNearby)
        {
            var health = pawn.health;
            var hediffDeathrattle = new List<Hediff>();

            Hediff hediffLifesupport = null;
            var hediffDeathRattle = HarmonyPatches.HediffDeathRattle;
            foreach (var hediff in health.hediffSet.hediffs)
                if (hediff.def == LifeSupportDefOf.QeLifeSupport)
                    hediffLifesupport = hediff;
                else if (!(hediffDeathRattle is null) && hediffDeathRattle.IsInstanceOfType(hediff))
                    hediffDeathrattle.Add(hediff);

            if (validLifeSupportNearby)
            {
                if (hediffLifesupport is null) hediffLifesupport = health.AddHediff(LifeSupportDefOf.QeLifeSupport);
                hediffLifesupport.Severity = pawn.WouldDieWithoutLifeSupport() ? 1.0f : 0.5f;

                foreach (var hediff in hediffDeathrattle) health.RemoveHediff(hediff);
            }
            else if (!(hediffLifesupport is null))
            {
                health.RemoveHediff(hediffLifesupport);
            }
        }
    }
}