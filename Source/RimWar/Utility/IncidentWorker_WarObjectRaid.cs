using System;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;


namespace RimWar.Utility
{
    public class IncidentWorker_WarObjectRaid : IncidentWorker_Raid
    {              

        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelEnemy + ": " + parms.faction.Name;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name.ApplyTag(parms.faction)).CapitalizeFirst();
            str += "\n\n";
            str += parms.raidStrategy.arrivalTextEnemy;
            Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (pawn != null)
            {
                str += "\n\n";
                str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
            }
            return str;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }

        public void TryExecuteCustomWorker(IncidentParms parms, PawnGroupKindDef _combat)
        {
            combat = _combat;
            TryExecuteWorker(parms);
        }

        private PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            parms.raidStrategy.Worker.TryGenerateThreats(parms);
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            
            float points = parms.points;
            List<Pawn> list = parms.raidStrategy.Worker.SpawnThreats(parms);
            if (list == null)
            {
                list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms)).ToList();
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms);
                    return false;
                }
                parms.raidArrivalMode.Worker.Arrive(list, parms);
            }
            GenerateRaidLoot(parms, points, list);
            TaggedString letterLabel = GetLetterLabel(parms);
            TaggedString letterText = GetLetterText(parms, list);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, GetRelatedPawnsInfoLetterText(parms), true);
            List<TargetInfo> list2 = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
                if (list4.Any())
                {
                    list2.Add(list4[0]);
                }
                for (int i = 0; i < list3.Count; i++)
                {
                    if (list3[i] != list4 && list3[i].Any())
                    {
                        list2.Add(list3[i][0]);
                    }
                }
            }
            else if (list.Any())
            {
                foreach (Pawn item in list)
                {
                    list2.Add(item);
                }               
                
            }
            SendRimWarLetter(letterLabel, letterText, GetLetterDef(), parms, list2);
            parms.raidStrategy.Worker.MakeLords(parms, list);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            Find.StoryWatcher.statsRecord.numRaidsEnemy++;
            return true;
        }

        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            if (parms.points <= 0f)
            {
                Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
            }
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy == null)
            {
                Map map = (Map)parms.target;
                DefDatabase<RaidStrategyDef>.AllDefs.Where(delegate (RaidStrategyDef d)
                {
                    if (d.Worker.CanUseWith(parms, groupKind))
                    {
                        if (parms.raidArrivalMode == null)
                        {
                            if (d.arriveModes != null)
                            {
                                return d.arriveModes.Any((PawnsArrivalModeDef x) => x.Worker.CanUseWith(parms));
                            }
                            return false;
                        }
                        return true;
                    }
                    return false;
                }).TryRandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionWeight(map, parms.points), out RaidStrategyDef result);
                parms.raidStrategy = result;
                if (parms.raidStrategy == null)
                {
                    Log.Error("No raid stategy found, defaulting to ImmediateAttack. Faction=" + parms.faction.def.defName + ", points=" + parms.points + ", groupKind=" + groupKind + ", parms=" + parms);
                    parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                }
            }
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (parms.faction != null)
            {
                return true;
            }
            float num = parms.points;
            if (num <= 0f)
            {
                num = 999999f;
            }
            if (PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => FactionCanBeGroupSource(f, map), allowNonHostileToPlayer: true, allowHidden: true, allowDefeated: true))
            {
                return true;
            }
            if (PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => FactionCanBeGroupSource(f, map, desperate: true), allowNonHostileToPlayer: true, allowHidden: true, allowDefeated: true))
            {
                return true;
            }
            return false;
        }

        public void SendRimWarLetter(TaggedString baseLetterLabel, TaggedString baseLetterText, LetterDef baseLetterDef, IncidentParms parms, LookTargets lookTargets, params NamedArgument[] textArgs)
        {
            if (baseLetterLabel.NullOrEmpty() || baseLetterText.NullOrEmpty())
            {
                Log.Error("Sending standard incident letter with no label or text.");
            }
            TaggedString taggedString = baseLetterText.Formatted(textArgs);
            TaggedString text;
            if (parms.customLetterText.NullOrEmpty())
            {
                text = taggedString;
            }
            else
            {
                List<NamedArgument> list = new List<NamedArgument>();
                if (textArgs != null)
                {
                    list.AddRange(textArgs);
                }
                list.Add(taggedString.Named("BASETEXT"));
                text = parms.customLetterText.Formatted(list.ToArray());
            }
            TaggedString taggedString2 = baseLetterLabel.Formatted(textArgs);
            TaggedString label;
            if (parms.customLetterLabel.NullOrEmpty())
            {
                label = taggedString2;
            }
            else
            {
                List<NamedArgument> list2 = new List<NamedArgument>();
                if (textArgs != null)
                {
                    list2.AddRange(textArgs);
                }
                list2.Add(taggedString2.Named("BASELABEL"));
                label = parms.customLetterLabel.Formatted(list2.ToArray());
            }
            ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, parms.customLetterDef ?? baseLetterDef, lookTargets, parms.faction, parms.quest, parms.letterHyperlinkThingDefs);
            //List<HediffDef> list3 = new List<HediffDef>();
            //if (!parms.letterHyperlinkHediffDefs.NullOrEmpty())
            //{
            //    list3.AddRange(parms.letterHyperlinkHediffDefs);
            //}
            //Log.Message("5");
            //if (!def.letterHyperlinkHediffDefs.NullOrEmpty())
            //{
            //    if (list3 == null)
            //    {
            //        list3 = new List<HediffDef>();
            //    }
            //    list3.AddRange(def.letterHyperlinkHediffDefs);
            //}
            //Log.Message("6");
            //choiceLetter.hyperlinkHediffDefs = list3;
            Find.LetterStack.ReceiveLetter(choiceLetter);
        }
    }
}
