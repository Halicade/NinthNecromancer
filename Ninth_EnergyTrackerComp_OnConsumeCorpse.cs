using ItsSorceryFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NinthNecromancer
{


    public class Ninth_EnergyTrackerComp_OnConsumeCorpse : EnergyTrackerComp
    {
        public Ninth_EnergyTrackerCompProperties_OnConsumeCorpse Props => (Ninth_EnergyTrackerCompProperties_OnConsumeCorpse)props;

        public Pawn Pawn => parent.pawn;

        public StatDef ScalingStatDef => Props.scalingStatDef is null ? StatDefOf_ItsSorcery.ISF_ScalingStat : Props.scalingStatDef;

        public bool AtLimit => parent.schema.limitLocked && (!parent.def.inverse ? parent.currentEnergy >= parent.MaxEnergy : parent.currentEnergy <= parent.MinEnergy);

        public override IEnumerable<FloatMenuOption> CompPostConsume(Vector3 vec3) // for effects when using a "consumption" item
        {
            if (Props.acceptableStages.NullOrEmpty()) yield break; // no consumables to look for = don't look for them.

            String text = "";

            // find the consumable at the location; if not, skip to the next item in the list.
            Corpse deadBody = vec3.ToIntVec3().GetFirstThing<Corpse>(Pawn.Map);
            //if (ammo is null) continue;
            if (deadBody == null) yield break;

            if (!Props.acceptableStages.Contains(deadBody.GetRotStage())) yield break;

            if (!Pawn.CanReach(deadBody, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn)) // No path = no consumption.
            {
                text = "ISF_Charge".Translate(parent.schema.def.LabelCap.ToString(), parent.def.LabelCap, deadBody.def.label)
                    + "ISF_ChargeNoPath".Translate();
                yield return new FloatMenuOption(text, null, MenuOptionPriority.Default,
                    null, null, 0f, null, null, true, 0);
            }
            else if (AtLimit) // if limit locked AND curr energy > max or curr energy < min (depending on inverse; i.e. 100 >= 50)
            {
                text = "ISF_Charge".Translate(parent.schema.def.LabelCap.ToString(), parent.def.LabelCap, deadBody.def.label)
                    + "ISF_ChargeFull".Translate();
                yield return new FloatMenuOption(text, null, MenuOptionPriority.Default,
                    null, null, 0f, null, null, true, 0);
            }
            else
            {
                int count = 0;
                float energyDiff = 0;
                int endcount = deadBody.stackCount;
                Log.Message("scale stat is " + Pawn.GetStatValue(ScalingStatDef));
                float adjConsumeEnergy = deadBody.InnerPawn.BodySize * (Pawn.GetStatValue(ScalingStatDef)*2) * 5;
                //Times 2 is because I want to keep the scaling stat but nullify it for now
                //Times 5 is temporary. Don't know what the value should be yet
                float gain = endcount * adjConsumeEnergy;

                if (!parent.def.inverse)
                {
                    energyDiff = Mathf.Max(0f, parent.schema.limitLocked ? parent.MaxEnergy - parent.currentEnergy : parent.AbsMaxEnergy - parent.currentEnergy);
                    count = (int)Math.Ceiling(energyDiff / adjConsumeEnergy);
                    endcount = Math.Min(count, deadBody.stackCount);
                    gain = Math.Min(endcount * adjConsumeEnergy, energyDiff);

                    text = "ISF_Charge".Translate(parent.schema.def.LabelCap.ToString(), parent.def.LabelCap, deadBody.def.label)
                    + "ISF_ChargeCalc".Translate(endcount, deadBody.def.label,
                        gain, parent.EnergyLabel);
                }
                else
                {
                    energyDiff = Mathf.Max(0f, parent.schema.limitLocked ? parent.currentEnergy - parent.MinEnergy : parent.currentEnergy - parent.AbsMinEnergy);
                    count = (int)Math.Ceiling(energyDiff / adjConsumeEnergy);
                    endcount = Math.Min(count, deadBody.stackCount);
                    gain = Math.Min(endcount * adjConsumeEnergy, energyDiff);

                    text = "ISF_Charge".Translate(parent.schema.def.LabelCap.ToString(), parent.def.LabelCap, deadBody.def.label)
                    + "ISF_ChargeCalc".Translate(endcount, deadBody.def.label,
                        gain, parent.EnergyLabel.Translate());
                }

                Action chargeSchema = delegate ()
                {
                    Job job = JobGiver_Charge.MakeChargeEnergyJob(parent.pawn, parent.schema, parent.def, deadBody, endcount, adjConsumeEnergy);
                    parent.pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                };
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, chargeSchema,
                    MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), parent.pawn, deadBody, "ReservedBy", null);
            }




            yield break;
        }

        public override float CompDrawGUI(Rect rect)
        {
            float yMin = rect.yMin;

            // retrieve string values
            string energyLabel = parent.EnergyLabel;

            // draw label
            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnConsumeLabel".Translate(ScalingStatDef.label.Named("STAT")).Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            /*
            if (Props.consumables.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "N/A");
                rect.yMin += rect.height;
                return rect.yMin - yMin;
            }*/

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnConsumeDesc".Translate(energyLabel.Named("ENERGY")), true, false);
            rect.yMin += rect.height;
            /*
            foreach (var consumable in Props.consumables)
            {
                float energyVal = parent.InvMult * consumable.energy * Pawn.GetStatValue(ScalingStatDef);
                string energyValString = energyVal.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset);
                Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnConsumeItem".Translate(consumable.thingDef.label.Named("ITEM"), energyValString.Named("CHANGE")));
                rect.yMin += rect.height;
            }*/
            return rect.yMin - yMin;
        }

    }

}
