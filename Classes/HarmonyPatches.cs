using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using static Verse.PawnCapacityUtility;

namespace LifeSupport.Classes
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        internal static readonly Type HediffDeathRattle;

        static HarmonyPatches()
        {
            //Harmony
            var harmony = new Harmony("LifeSupport");

            harmony.Patch(
                AccessTools.Method(typeof(Pawn_HealthTracker), "ShouldBeDeadFromRequiredCapacity"),
                new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_ShouldBeDeadFromRequiredCapacity)))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Toils_LayDown), "LayDown"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_LayDown)))
            );

            harmony.Patch(
                AccessTools.Method(typeof(PawnCapacityUtility), "CalculateLimbEfficiency"),
                new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_CalculateLimbEfficiency)))
            );

            HediffDeathRattle = AccessTools.TypeByName("DeathRattle.Hediff_DeathRattle");
            if (!(HediffDeathRattle is null)) Log.Message("[LifeSupport] found DeathRattle");
        }

        public static bool Patch_ShouldBeDeadFromRequiredCapacity(ref Pawn_HealthTracker instance,
            ref PawnCapacityDef result)
        {
            // Check if consciousness is there. If it is then its okay.

            var health = instance;
            var pawn = health.hediffSet.pawn;

            if (!health.hediffSet.HasHediff(LifeSupportDefOf.QeLifeSupport)) // not on life support
                return true;
            if (!pawn.ValidLifeSupportNearby()) // life support is unpowered
                return true;
            if (!health.capacities.CapableOf(PawnCapacityDefOf.Consciousness)) // no consciousness
                return true;

            result = null;
            return false;
        }

        public static void Patch_LayDown(ref Toil result)
        {
            var toil = result;
            if (toil == null)
                return;

            toil.AddPreTickAction(delegate
            {
                var pawn = toil.actor;
                if (pawn is null || pawn.Dead) return;

                pawn.SetHediffs();
            });
        }

        public static bool Patch_CalculateLimbEfficiency(ref float result, HediffSet diffSet,
            BodyPartTagDef limbCoreTag, BodyPartTagDef limbSegmentTag,
            BodyPartTagDef limbDigitTag, float appendageWeight, out float functionalPercentage,
            List<CapacityImpactor> impactors)
        {
            functionalPercentage = 0f;

            if (limbCoreTag != BodyPartTagDefOf.MovingLimbCore) return true;

            var hediff = diffSet.GetFirstHediffOfDef(LifeSupportDefOf.QeLifeSupport);
            if (hediff is null) return true;

            if (hediff.Severity < 1f) return true;

            result = 0f;

            if (!(impactors is null))
            {
                var capacityImpactor = new CapacityImpactorHediff
                {
                    hediff = hediff
                };
                impactors.Add(capacityImpactor);
            }

            return false;
        }
    }
}