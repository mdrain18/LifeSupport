using RimWorld;
using Verse;

namespace LifeSupport.Classes
{
    public class HediffLifeSupport : HediffWithComps
    {
        public override bool ShouldRemove => pawn.CurrentBed() == null;

        public override bool Visible => true;
    }
}