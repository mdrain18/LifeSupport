using RimWorld;
using Verse;

namespace LifeSupport.Classes
{
    [DefOf]
    public static class LifeSupportDefOf
    {
        public static HediffDef QeLifeSupport;

        static LifeSupportDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(LifeSupportDefOf));
        }
    }
}