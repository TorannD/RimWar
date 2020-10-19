using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld.Planet;
using RimWar.Planet;
using HarmonyLib;
using System.Reflection;

namespace RimWar.Utility
{
    public static class FactionDialogReMaker
    {

        public struct DisplayClass
        {
            public Pawn negotiator;
            public DiaNode root;
        }

        public static DiaNode FactionDialogFor(Pawn negotiator, Faction faction)
        {
            MethodBase RequestRoyalHeirChangeOption = AccessTools.Method(typeof(FactionDialogMaker), "RequestRoyalHeirChangeOption", null, null);
            MethodBase RoyalHeirChangeConfirm = AccessTools.Method(typeof(FactionDialogMaker), "RoyalHeirChangeConfirm", null, null);
            MethodBase RoyalHeirChangeCandidates = AccessTools.Method(typeof(FactionDialogMaker), "RoyalHeirChangeCandidates", null, null);
            MethodBase RequestAICoreQuest = AccessTools.Method(typeof(FactionDialogMaker), "RequestAICoreQuest", null, null);
            MethodBase DebugOptions = AccessTools.Method(typeof(FactionDialogMaker), "DebugOptions", null, null);

            DisplayClass dc0 = default(DisplayClass);
            Map map = negotiator.Map;
            Pawn pawn;
            string value;
            if (faction.leader != null)
            {
                pawn = faction.leader;
                value = faction.leader.Name.ToStringFull.Colorize(ColoredText.NameColor);
            }
            else
            {
                pawn = dc0.negotiator;
                value = faction.Name;
            }
            if (faction.PlayerRelationKind == FactionRelationKind.Hostile)
            {
                string key = (faction.def.permanentEnemy || !"FactionGreetingHostileAppreciative".CanTranslate()) ? "FactionGreetingHostile" : "FactionGreetingHostileAppreciative";
                dc0.root = new DiaNode(key.Translate(value).AdjustedFor(pawn));
            }
            else if (faction.PlayerRelationKind == FactionRelationKind.Neutral)
            {
                dc0.root = new DiaNode("FactionGreetingWary".Translate(value, dc0.negotiator.LabelShort, dc0.negotiator.Named("NEGOTIATOR"), pawn.Named("LEADER")).AdjustedFor(pawn));
            }
            else
            {
                dc0.root = new DiaNode("FactionGreetingWarm".Translate(value, dc0.negotiator.LabelShort, dc0.negotiator.Named("NEGOTIATOR"), pawn.Named("LEADER")).AdjustedFor(pawn));
            }
            if (map != null && map.IsPlayerHome)
            {

                AddAndDecorateOption(RequestTraderOption(map, faction, dc0.negotiator), true, ref dc0);
                AddAndDecorateOption(RequestMilitaryAid_Scouts_Option(map, faction, dc0.negotiator), true, ref dc0);
                AddAndDecorateOption(RequestMilitaryAid_Warband_Option(map, faction, dc0.negotiator), true, ref dc0);
                Pawn_RoyaltyTracker royalty = dc0.negotiator.royalty;
                if (royalty != null && royalty.HasAnyTitleIn(faction))
                {
                    foreach (RoyalTitle item in royalty.AllTitlesInEffectForReading)
                    {
                        if (item.def.permits != null)
                        {
                            foreach (RoyalTitlePermitDef permit in item.def.permits)
                            {
                                IEnumerable<DiaOption> factionCommDialogOptions = permit.Worker.GetFactionCommDialogOptions(map, dc0.negotiator, faction);
                                if (factionCommDialogOptions != null)
                                {
                                    foreach (DiaOption item2 in factionCommDialogOptions)
                                    {
                                        AddAndDecorateOption(item2, true, ref dc0);
                                    }
                                }
                            }
                        }
                    }
                    if (royalty.GetCurrentTitle(faction).canBeInherited && !dc0.negotiator.IsQuestLodger())
                    {
                        AddAndDecorateOption((DiaOption)RequestRoyalHeirChangeOption.Invoke((object)typeof(FactionDialogMaker), new object[]
                            {map, faction, pawn, dc0.negotiator }), true, ref dc0);
                    }
                }
                if (DefDatabase<ResearchProjectDef>.AllDefsListForReading.Any(delegate (ResearchProjectDef rp)
                {
                    if (rp.HasTag(ResearchProjectTagDefOf.ShipRelated))
                    {
                        return rp.IsFinished;
                    }
                    return false;
                }))
                {
                    AddAndDecorateOption((DiaOption)RequestAICoreQuest.Invoke((object)typeof(FactionDialogMaker), new object[]
                        { map, faction, dc0.negotiator }), true, ref dc0);
                }
            }
            if (Prefs.DevMode)
            {
                foreach (DiaOption item3 in (IEnumerable<DiaOption>)DebugOptions.Invoke((object)typeof(FactionDialogMaker), new object[]
                    { faction, dc0.negotiator }))
                {
                    AddAndDecorateOption(item3, false, ref dc0);
                }
            }
            AddAndDecorateOption(new DiaOption("(" + "Disconnect".Translate() + ")")
            {
                resolveTree = true
            }, false, ref dc0);
            return dc0.root;
        }

        public static void AddAndDecorateOption(DiaOption opt, bool needsSocial, ref DisplayClass dc)
        {
            if (needsSocial && dc.negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
            {
                opt.Disable("WorkTypeDisablesOption".Translate(SkillDefOf.Social.label));
            }
            dc.root.options.Add(opt);
        }

        public static DiaOption RequestMilitaryAid_Scouts_Option(Map map, Faction faction, Pawn negotiator)
        {
            RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
            float daysToArrive = 0f;
            Settlement wos = null;
            int requestCost = 15;
            int minPoints = 0;
            if (rwd != null)
            {
                minPoints = (int)(Find.WorldObjects.SettlementAt(map.Tile).GetComponent<RimWarSettlementComp>().RimWarPoints * .75f);
                wos = rwd.ClosestSettlementTo(map.Tile, minPoints);
                if (wos != null)
                {
                    daysToArrive = (float)Utility.ArrivalTimeEstimator.EstimatedTicksToArrive(wos.Tile, map.Tile, (int)(2000f / rwd.movementAttribute)) / 60000f;
                }
            }
            string text = "RequestMilitaryAid".Translate(requestCost);
            if (wos != null)
            {
                text = "RW_RequestScout".Translate(requestCost, wos.Name, daysToArrive.ToString("#.#"), (int)(minPoints * rwd.combatAttribute));
            }
            if (wos == null)
            {
                DiaOption diaOptionUnable = new DiaOption(text);
                diaOptionUnable.Disable("RW_NoTownForRequest".Translate());
                return diaOptionUnable;
            }
            if (faction.PlayerRelationKind != FactionRelationKind.Ally)
            {
                DiaOption diaOption = new DiaOption(text);
                diaOption.Disable("MustBeAlly".Translate());
                return diaOption;
            }
            if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
            {
                DiaOption diaOption2 = new DiaOption(text);
                diaOption2.Disable("BadTemperature".Translate());
                return diaOption2;
            }
            int num = faction.lastMilitaryAidRequestTick + 60000 - Find.TickManager.TicksGame;
            if (num > 0)
            {
                DiaOption diaOption3 = new DiaOption(text);
                diaOption3.Disable("WaitTime".Translate(num.ToStringTicksToPeriod()));
                return diaOption3;
            }
            //if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, faction))
            //{
            //    DiaOption diaOption4 = new DiaOption(text);
            //    diaOption4.Disable("HostileVisitorsPresent".Translate());
            //    return diaOption4;
            //}
            DiaOption diaOption5 = new DiaOption(text);
            IEnumerable<Faction> source = (from x in map.attackTargetsCache.TargetsHostileToColony
                                            where GenHostility.IsActiveThreatToPlayer(x)
                                            select ((Thing)x).Faction).Where(delegate (Faction x)
                                            {
                                                if (x != null)
                                                {
                                                    return !x.HostileTo(faction);
                                                }
                                                return false;
                                            }).Distinct();
            if (source.Any())
            {
                DiaNode diaNode = new DiaNode("MilitaryAidConfirmMutualEnemy".Translate(faction.Name, (from fa in source
                                                                                        select fa.Name).ToCommaList(useAnd: true)));
                DiaOption diaOption6 = new DiaOption("CallConfirm".Translate());
                diaOption6.action = delegate
                {
                    CallForAid(new Scout(), minPoints, map, faction, requestCost, rwd, wos);
                };
                diaOption6.link = FightersSent(faction, negotiator);
                DiaOption diaOption7 = new DiaOption("CallCancel".Translate());
                diaOption7.linkLateBind = ResetToRoot(faction, negotiator);
                diaNode.options.Add(diaOption6);
                diaNode.options.Add(diaOption7);
                diaOption5.link = diaNode;
            }
            else
            {
                diaOption5.action = delegate
                {
                    CallForAid(new Scout(), minPoints, map, faction, requestCost, rwd, wos);
                };
                diaOption5.link = FightersSent(faction, negotiator);
            }
            
            return diaOption5;
        }

        public static DiaOption RequestMilitaryAid_Warband_Option(Map map, Faction faction, Pawn negotiator)
        {
            RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
            float daysToArrive = 0f;
            Settlement wos = null;
            int requestCost = 20;
            int minPoints = 0;
            if (rwd != null)
            {
                minPoints = (int)(Find.WorldObjects.SettlementAt(map.Tile).GetComponent<RimWarSettlementComp>().RimWarPoints * 1.15f);
                wos = rwd.ClosestSettlementTo(map.Tile, minPoints);
                if (wos != null)
                {
                    daysToArrive = (float)Utility.ArrivalTimeEstimator.EstimatedTicksToArrive(wos.Tile, map.Tile, (int)(2800f * (1f / rwd.movementAttribute))) / 60000f;
                }
            }
            string text = "RequestMilitaryAid".Translate(requestCost);
            if (wos != null)
            {
                text = "RW_RequestWarband".Translate(requestCost, wos.Name, daysToArrive.ToString("#.#"), (int)(minPoints * rwd.combatAttribute));
            }
            if (wos == null)
            {
                DiaOption diaOptionUnable = new DiaOption(text);
                diaOptionUnable.Disable("RW_NoTownForRequest".Translate());
                return diaOptionUnable;
            }
            if (faction.PlayerRelationKind != FactionRelationKind.Ally)
            {
                DiaOption diaOption = new DiaOption(text);
                diaOption.Disable("MustBeAlly".Translate());
                return diaOption;
            }
            if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
            {
                DiaOption diaOption2 = new DiaOption(text);
                diaOption2.Disable("BadTemperature".Translate());
                return diaOption2;
            }
            int num = faction.lastMilitaryAidRequestTick + 60000 - Find.TickManager.TicksGame;
            if (num > 0)
            {
                DiaOption diaOption3 = new DiaOption(text);
                diaOption3.Disable("WaitTime".Translate(num.ToStringTicksToPeriod()));
                return diaOption3;
            }
            //if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, faction))
            //{
            //    DiaOption diaOption4 = new DiaOption(text);
            //    diaOption4.Disable("HostileVisitorsPresent".Translate());
            //    return diaOption4;
            //}
            DiaOption diaOption5 = new DiaOption(text);
            IEnumerable<Faction> source = (from x in map.attackTargetsCache.TargetsHostileToColony
                                           where GenHostility.IsActiveThreatToPlayer(x)
                                           select ((Thing)x).Faction).Where(delegate (Faction x)
                                           {
                                               if (x != null)
                                               {
                                                   return !x.HostileTo(faction);
                                               }
                                               return false;
                                           }).Distinct();
            if (source.Any())
            {
                DiaNode diaNode = new DiaNode("MilitaryAidConfirmMutualEnemy".Translate(faction.Name, (from fa in source
                                                                                                       select fa.Name).ToCommaList(useAnd: true)));
                DiaOption diaOption6 = new DiaOption("CallConfirm".Translate());
                diaOption6.action = delegate
                {
                    CallForAid(new Warband(), minPoints, map, faction, requestCost, rwd, wos);
                };
                diaOption6.link = FightersSent(faction, negotiator);
                DiaOption diaOption7 = new DiaOption("CallCancel".Translate());
                diaOption7.linkLateBind = ResetToRoot(faction, negotiator);
                diaNode.options.Add(diaOption6);
                diaNode.options.Add(diaOption7);
                diaOption5.link = diaNode;
            }
            else
            {
                diaOption5.action = delegate
                {
                    CallForAid(new Warband(), minPoints, map, faction, requestCost, rwd, wos);
                };
                diaOption5.link = FightersSent(faction, negotiator);
            }

            return diaOption5;
        }

        public static DiaOption RequestMilitaryAid_LaunchedWarband_Option(Map map, Faction faction, Pawn negotiator)
        {
            RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
            float daysToArrive = 0f;
            Settlement wos = null;
            int requestCost = 25;
            int minPoints = 0;
            if (rwd != null)
            {
                minPoints = (int)(Find.WorldObjects.SettlementAt(map.Tile).GetComponent<RimWarSettlementComp>().RimWarPoints *.9f);
                wos = rwd.ClosestSettlementTo(map.Tile, minPoints);
                if (wos != null)
                {
                    daysToArrive = (float)Utility.ArrivalTimeEstimator.EstimatedTicksToArrive(wos.Tile, map.Tile, 100) / 60000f;
                }
            }
            string text = "RequestMilitaryAid".Translate(requestCost);
            if (wos != null)
            {
                text = "RW_RequestLaunchedWarband".Translate(requestCost, wos.Name, daysToArrive.ToString("#.#"), (int)(minPoints*rwd.combatAttribute));
            }
            if(wos == null)
            {
                DiaOption diaOptionUnable = new DiaOption(text);
                diaOptionUnable.Disable("RW_NoTownForRequest".Translate());
                return diaOptionUnable;
            }
            if (faction.PlayerRelationKind != FactionRelationKind.Ally)
            {
                DiaOption diaOption = new DiaOption(text);
                diaOption.Disable("MustBeAlly".Translate());
                return diaOption;
            }
            if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
            {
                DiaOption diaOption2 = new DiaOption(text);
                diaOption2.Disable("BadTemperature".Translate());
                return diaOption2;
            }
            int num = faction.lastMilitaryAidRequestTick + 60000 - Find.TickManager.TicksGame;
            if (num > 0)
            {
                DiaOption diaOption3 = new DiaOption(text);
                diaOption3.Disable("WaitTime".Translate(num.ToStringTicksToPeriod()));
                return diaOption3;
            }
            if (!rwd.CanLaunch)
            {
                DiaOption diaOption4 = new DiaOption(text);
                diaOption4.Disable("RW_FactionIncapableOfTech".Translate(faction.Name));
                return diaOption4;
            }
            DiaOption diaOption5 = new DiaOption(text);
            IEnumerable<Faction> source = (from x in map.attackTargetsCache.TargetsHostileToColony
                                           where GenHostility.IsActiveThreatToPlayer(x)
                                           select ((Thing)x).Faction).Where(delegate (Faction x)
                                           {
                                               if (x != null)
                                               {
                                                   return !x.HostileTo(faction);
                                               }
                                               return false;
                                           }).Distinct();
            if (source.Any())
            {
                DiaNode diaNode = new DiaNode("MilitaryAidConfirmMutualEnemy".Translate(faction.Name, (from fa in source
                                                                                                       select fa.Name).ToCommaList(useAnd: true)));
                DiaOption diaOption6 = new DiaOption("CallConfirm".Translate());
                diaOption6.action = delegate
                {
                    Faction ofPlayer = Faction.OfPlayer;
                    bool canSendMessage = false;
                    string reason = "GoodwillChangedReason_RequestedMilitaryAid".Translate();
                    faction.TryAffectGoodwillWith(ofPlayer, -requestCost, canSendMessage, true, reason);
                    wos.GetComponent<RimWarSettlementComp>().RimWarPoints -= minPoints;
                    WorldUtility.CreateLaunchedWarband(minPoints, rwd, wos, wos.Tile, Find.WorldObjects.SettlementAt(map.Tile), WorldObjectDefOf.Settlement);
                };
                diaOption6.link = FightersSent(faction, negotiator);
                DiaOption diaOption7 = new DiaOption("CallCancel".Translate());
                diaOption7.linkLateBind = ResetToRoot(faction, negotiator);
                diaNode.options.Add(diaOption6);
                diaNode.options.Add(diaOption7);
                diaOption5.link = diaNode;
            }
            else
            {
                diaOption5.action = delegate
                {
                    Faction ofPlayer = Faction.OfPlayer;
                    bool canSendMessage = false;
                    string reason = "GoodwillChangedReason_RequestedMilitaryAid".Translate();
                    faction.TryAffectGoodwillWith(ofPlayer, -requestCost, canSendMessage, true, reason);
                    wos.GetComponent<RimWarSettlementComp>().RimWarPoints -= minPoints;
                    WorldUtility.CreateLaunchedWarband(minPoints, rwd, wos, wos.Tile, Find.WorldObjects.SettlementAt(map.Tile), WorldObjectDefOf.Settlement);
                };
                diaOption5.link = FightersSent(faction, negotiator);
            }

            return diaOption5;
        }

        public static DiaOption RequestTraderOption(Map map, Faction faction, Pawn negotiator)
        {
            RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
            float daysToArrive = 0f;
            Settlement wos = null;
            int requestCost = 15;
            if (rwd != null)
            {
                wos = rwd.ClosestSettlementTo(map.Tile, 200);
                if (wos != null)
                {
                    daysToArrive = (float)Utility.ArrivalTimeEstimator.EstimatedTicksToArrive(wos.Tile, map.Tile, (int)(2500f / rwd.movementAttribute)) / 60000f;
                }
            }
            TaggedString taggedString = "RequestTrader".Translate(requestCost);
            if (wos != null)
            {
                taggedString = "RequestTrader".Translate(requestCost) + "\n" + "RW_RequestAddition".Translate(wos.Name, daysToArrive.ToString("#.#"));
            }
            if (wos == null)
            {
                DiaOption diaOptionUnable = new DiaOption(taggedString);
                diaOptionUnable.Disable("RW_NoTownForRequest".Translate());
                return diaOptionUnable;
            }
            if (faction.PlayerRelationKind != FactionRelationKind.Ally)
            {
                DiaOption diaOption = new DiaOption(taggedString);
                diaOption.Disable("MustBeAlly".Translate());
                return diaOption;
            }
            if(rwd == null)
            {
                DiaOption diaOptionRWD = new DiaOption(taggedString);
                diaOptionRWD.Disable("RW_InvalidRWD".Translate());
                return diaOptionRWD;
            }
            if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
            {
                DiaOption diaOption2 = new DiaOption(taggedString);
                diaOption2.Disable("BadTemperature".Translate());
                return diaOption2;
            }
            int num = faction.lastTraderRequestTick + 240000 - Find.TickManager.TicksGame;
            if (num > 0)
            {
                DiaOption diaOption3 = new DiaOption(taggedString);
                diaOption3.Disable("WaitTime".Translate(num.ToStringTicksToPeriod()));
                return diaOption3;
            }
            DiaOption diaOption4 = new DiaOption(taggedString);
            DiaNode diaNode = new DiaNode("TraderSent".Translate(faction.leader).CapitalizeFirst());
            diaNode.options.Add(OKToRoot(faction, negotiator));
            DiaNode diaNode2 = new DiaNode("ChooseTraderKind".Translate(faction.leader));
            foreach (TraderKindDef item in from x in faction.def.caravanTraderKinds
                                           where x.requestable
                                           select x)
            {
                TraderKindDef localTk = item;
                DiaOption diaOption5 = new DiaOption(localTk.LabelCap);
                if (localTk.TitleRequiredToTrade != null && (negotiator.royalty == null || localTk.TitleRequiredToTrade.seniority > negotiator.GetCurrentTitleSeniorityIn(faction)))
                {
                    DiaNode diaNode3 = new DiaNode("TradeCaravanRequestDeniedDueTitle".Translate(negotiator.Named("NEGOTIATOR"), localTk.TitleRequiredToTrade.GetLabelCapFor(negotiator).Named("TITLE"), faction.Named("FACTION")));
                    DiaOption diaOption6 = new DiaOption("GoBack".Translate());
                    diaNode3.options.Add(diaOption6);
                    diaOption5.link = diaNode3;
                    diaOption6.link = diaNode2;
                }
                else
                {
                    diaOption5.action = delegate
                    {
                        Trader trader = WorldUtility.CreateTrader(200, rwd, wos, wos.Tile, Find.WorldObjects.SettlementAt(map.Tile), WorldObjectDefOf.Settlement);
                        trader.traderKind = localTk;
                        wos.GetComponent<RimWarSettlementComp>().RimWarPoints -= 200;
                        faction.lastTraderRequestTick = Find.TickManager.TicksGame;
                        faction.TryAffectGoodwillWith(Faction.OfPlayer, -requestCost, false, true, "GoodwillChangedReason_RequestedTrader".Translate());
                    };
                    diaOption5.link = diaNode;
                }
                diaNode2.options.Add(diaOption5);
            }
            DiaOption diaOption7 = new DiaOption("GoBack".Translate());
            diaOption7.linkLateBind = ResetToRoot(faction, negotiator);
            diaNode2.options.Add(diaOption7);
            diaOption4.link = diaNode2;
            return diaOption4;
        }

        public static void CallForAid(WarObject rwo, int pts, Map map, Faction faction, int callRelationsCost, RimWarData rwd, Settlement sendingSettlement)
        {
            Faction ofPlayer = Faction.OfPlayer;
            bool canSendMessage = false;
            string reason = "GoodwillChangedReason_RequestedMilitaryAid".Translate();
            faction.TryAffectGoodwillWith(ofPlayer, -callRelationsCost, canSendMessage, true, reason);
            sendingSettlement.GetComponent<RimWarSettlementComp>().RimWarPoints -= pts;
            WorldUtility.CreateWarObjectOfType(rwo, pts, rwd, sendingSettlement, sendingSettlement.Tile, Find.WorldObjects.SettlementAt(map.Tile), WorldObjectDefOf.Settlement);            
        }

        public static DiaNode FightersSent(Faction faction, Pawn negotiator)
        {
            return new DiaNode("MilitaryAidSent".Translate(faction.leader).CapitalizeFirst())
            {
                options =
            {
                OKToRoot(faction, negotiator)
            }
            };
        }

        public static Func<DiaNode> ResetToRoot(Faction faction, Pawn negotiator)
        {
            return () => FactionDialogMaker.FactionDialogFor(negotiator, faction);
        }

        private static DiaOption OKToRoot(Faction faction, Pawn negotiator)
        {
            return new DiaOption("OK".Translate())
            {
                linkLateBind = ResetToRoot(faction, negotiator)
            };
        }
    }
}
