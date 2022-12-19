using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LifeSupport.Classes
{
    /// <summary>
    ///     Tags this Thing as being valid life support.
    /// </summary>
    public class LifeSupportComp : ThingComp
    {
        public bool Active => !(parent.TryGetComp<CompPowerTrader>() is CompPowerTrader power) || power.PowerOn;

        public override void ReceiveCompSignal(string signal)
        {
            if (signal != "PowerTurnedOn" && signal != "PowerTurnedOff") return;

            //Check for state change in surrounding pawns in beds.
            var map = parent.Map;
            var pawns = new List<Pawn>();
            foreach (var cell in parent.CellsAdjacent8WayAndInside())
            foreach (var thing in cell.GetThingList(map))
                if (thing is Building_Bed bed)
                    for (int i = 0, l = bed.SleepingSlotsCount; i < l; i++)
                    {
                        var pawn = bed.GetSleepingSlotPos(i).GetFirstPawn(map);
                        if (!(pawn is null)) pawns.Add(pawn);
                    }

            foreach (var pawn in pawns)
                if (!pawn.health.Dead)
                {
                    pawn.SetHediffs(false);
                    pawn.health.CheckForStateChange(null, null);
                }
        }
    }
}