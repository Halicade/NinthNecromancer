using ItsSorceryFramework;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace NinthNecromancer
{
    public class Ninth_EnergyTrackerCompProperties_OnConsumeCorpse : EnergyTrackerCompProperties
    {
        public Ninth_EnergyTrackerCompProperties_OnConsumeCorpse() => compClass = typeof(Ninth_EnergyTrackerComp_OnConsumeCorpse);

        public StatDef scalingStatDef;

        public List<RotStage> acceptableStages;
        /*
         *  Can be 
         * 	Fresh,
         * 	Rotting,
         * 	Dessicated
        */
    }

    public class EnergyConsumable
    {
        public ThingDef thingDef;

        public float energy = 1f;
    }
}