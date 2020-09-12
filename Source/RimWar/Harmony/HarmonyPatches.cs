using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.AI;
using System.Reflection.Emit;
using RimWar.Planet;
using RimWar.Utility;
using RimWar;

namespace RimWar.Harmony
{
    //[StaticConstructorOnStartup]
    //internal class HarmonyPatches
    //{
    //    private static readonly Type patchType = typeof(HarmonyPatches);

    //    static HarmonyPatches()
    //    {
    [StaticConstructorOnStartup]
    public class RimWarMod : Mod
    {
        private static readonly Type patchType = typeof(RimWarMod);        

        public RimWarMod(ModContentPack content) : base(content)
        {
            HarmonyLib.Harmony harmonyInstance = new HarmonyLib.Harmony("rimworld.torann.rimwar");
            //Postfix
            //harmonyInstance.Patch(AccessTools.Method(typeof(FactionUIUtility), "DrawFactionRow", new Type[]
            //    {
            //        typeof(Faction),
            //        typeof(float),
            //        typeof(Rect)
            //    }, null), null, new HarmonyMethod(typeof(HarmonyPatches), "DrawFactionRow_WithFactionPoints_Postfix", null), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(SettlementUtility), "AttackNow", new Type[]
                {
                    typeof(Caravan),
                    typeof(RimWorld.Planet.Settlement)
                }, null), null, new HarmonyMethod(patchType, "AttackNow_SettlementReinforcement_Postfix", null), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(RimWorld.Planet.Settlement), "GetInspectString", new Type[]
                {
                }, null), null, new HarmonyMethod(patchType, "Settlement_InspectString_WithPoints_Postfix", null), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(Caravan_PathFollower), "StartPath", new Type[]
                {
                    typeof(int),
                    typeof(CaravanArrivalAction),
                    typeof(bool),
                    typeof(bool)
                }, null), null, new HarmonyMethod(patchType, "Pather_StartPath_WarObjects", null), null);

            //GET

            //Transpiler
            //harmonyInstance.Patch(AccessTools.Method(typeof(Page_CreateWorldParams), "DoWindowContents"), null, null,
            //    new HarmonyMethod(patchType, nameof(RimWar_WorldParams_CoverageTranspiler)));

            //Prefix
            //harmonyInstance.Patch(AccessTools.Method(typeof(Page_CreateWorldParams), "DoWindowsContents", new Type[]
            //    {
            //        typeof(Rect)
            //    }, null), new HarmonyMethod(typeof(RimWarMod), "Page_CreateRimWarWorldParams_CoveragePatch", null), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(IncidentWorker), "TryExecute", new Type[]
                {
                    typeof(IncidentParms)
                }, null), new HarmonyMethod(patchType, "IncidentWorker_Prefix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(IncidentWorker_CaravanDemand), "ActionGive", new Type[]
                {
                    typeof(Caravan),
                    typeof(List<ThingCount>),
                    typeof(List<Pawn>)
                }, null), new HarmonyMethod(patchType, "Caravan_Give_Prefix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(IncidentWorker_NeutralGroup), "TryResolveParms", new Type[]
                {
                    typeof(IncidentParms)
                }, null), new HarmonyMethod(patchType, "TryResolveParms_Points_Prefix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(Faction), "TryAffectGoodwillWith", new Type[]
                {
                    typeof(Faction),
                    typeof(int),
                    typeof(bool),
                    typeof(bool),
                    typeof(string),
                    typeof(GlobalTargetInfo?)
                }, null), new HarmonyMethod(patchType, "TryAffectGoodwillWith_Reduction_Prefix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(IncidentQueue), "Add", new Type[]
                {
                    typeof(IncidentDef),
                    typeof(int),
                    typeof(IncidentParms),
                    typeof(int)
                }, null), new HarmonyMethod(patchType, "IncidentQueueAdd_Replacement_Prefix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(FactionDialogMaker), "CallForAid", new Type[]
                {
                    typeof(Map),
                    typeof(Faction)
                }, null), new HarmonyMethod(patchType, "CallForAid_Replacement_Patch", null), null, null);
        }

        //public static IEnumerable<CodeInstruction> RimWar_WorldParams_CoverageTranspiler(IEnumerable<CodeInstruction> instructions)
        //{

        //    foreach (CodeInstruction instruction in instructions)
        //    {
        //        var strOperand = instruction.ToString();
        //        if (strOperand.Contains("ldsfld") && !strOperand.Contains("Tick_Tiny"))
        //        {
        //            Log.Message("operand string is " + strOperand);
        //            yield return instruction;
        //            yield return new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: patchType, name: nameof(RimWar_PlanetCoverage)));
        //        }
        //        else
        //        {
        //            yield return instruction;
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(Scenario), "GetFirstConfigPage", null)]
        //public class RimWarConfigs_Scenario_Patch
        //{
        //    public static bool Prefix(Scenario __instance, ref Page __result)
        //    {
        //        List<ScenPart> parts = Traverse.Create(__instance).Field(name: "parts").GetValue<List<ScenPart>>();
        //        List<Page> list = new List<Page>();
        //        list.Add(new Page_SelectStoryteller());
        //        list.Add(new RimWar.Options.Page_CreateRimWarWorldParams());
        //        list.Add(new Page_SelectStartingSite());
        //        foreach (Page item in parts.SelectMany((ScenPart p) => p.GetConfigPages()))
        //        {
        //            list.Add(item);
        //        }
        //        Page page = PageUtility.StitchedPages(list);
        //        if (page != null)
        //        {
        //            Page page2 = page;
        //            while (page2.next != null)
        //            {
        //                page2 = page2.next;
        //            }
        //            page2.nextAct = delegate
        //            {
        //                PageUtility.InitGameStart();
        //            };
        //        }
        //        __result = page;
        //        return false;
        //    }
        //}

        //private static void RimWar_PlanetCoverage()
        //{
        //    float planetCoverage = (float)AccessTools.Field(typeof(Page_CreateWorldParams), "planetCoverage").GetValue(null);
        //    List<FloatMenuOption> list = new List<FloatMenuOption>();
        //    float[] array = Prefs.DevMode ? RimWar_PlanetCoveragesDev() : RimWar_PlanetCoveragesDev();
        //    foreach (float coverage in array)
        //    {
        //        string text = coverage.ToStringPercent();
        //        if (coverage <= 0.1f)
        //        {
        //            text += " (dev)";
        //        }
        //        FloatMenuOption item = new FloatMenuOption(text, delegate
        //        {
        //            if (planetCoverage != coverage)
        //            {
        //                planetCoverage = coverage;
        //                if (planetCoverage == 1f)
        //                {
        //                    Messages.Message("MessageMaxPlanetCoveragePerformanceWarning".Translate(), MessageTypeDefOf.CautionInput, historical: false);
        //                }
        //            }
        //        });
        //        list.Add(item);
        //    }
        //    Find.WindowStack.Add(new FloatMenu(list));
        //}

        //private static float[] RimWar_PlanetCoverages()
        //{
        //    float[] pCoverages = new float[7]
        //    {
        //        0.07f,
        //        0.1f,
        //        0.15f,
        //        0.2f,
        //        0.3f,
        //        0.5f,
        //        1f
        //    };
        //    float[] array = pCoverages;
        //    return array;
        //}

        //private static float[] RimWar_PlanetCoveragesDev()
        //{
        //    float[] pCoverages = new float[8]
        //    {
        //        0.07f,
        //        0.1f,
        //        0.15f,
        //        0.2f,
        //        0.3f,
        //        0.5f,
        //        1f,
        //        0.05f
        //    };
        //    float[] array = pCoverages;
        //    return array;
        //}

        [HarmonyPatch(typeof(WorldObject), "Destroy")]
        public static class SettlementDestroyed_Patch
        {
            private static void Postfix(WorldObject __instance)
            {
                if(__instance is RimWorld.Planet.Settlement)
                {
                    RimWarData rwd = WorldUtility.GetRimWarDataForFaction(__instance.Faction);
                    for(int i = 0; i < rwd.FactionSettlements.Count; i++)
                    {
                        if(rwd.FactionSettlements[i].Tile == __instance.Tile)
                        {
                            rwd.FactionSettlements.Remove(rwd.FactionSettlements[i]);
                            break;
                        }
                    }
                    if(rwd.FactionSettlements.Count <= 0)
                    {
                        WorldUtility.RemoveRWDFaction(rwd);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FactionManager), "Remove")]
        public static class RemoveFaction_Patch
        {
            private static void Postfix(FactionManager __instance, Faction faction)
            {
                RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
                if(rwd != null)
                {
                    WorldUtility.RemoveRWDFaction(rwd);
                }
            }
        }

        [HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents")]
        public static class Patch_Page_CreateWorldParams_DoWindowContents
        {
            private static void Postfix(Page_CreateWorldParams __instance, Rect rect)
            {
                float y = rect.y + rect.height - 120f;
                Text.Font = GameFont.Small;
                string label = "RW_RimWar".Translate();
                if (Widgets.ButtonText(new Rect(0f, y, 150f, 32f), label))
                {
                    OpenSettingsWindow(__instance);
                }
            }

            public static void OpenSettingsWindow(Page_CreateWorldParams __instance)
            {
                Find.WindowStack.TryRemove(typeof(EditWindow_Log));
                if (!Find.WindowStack.TryRemove(typeof(Options.RimWarSettingsWindow)))
                {
                    Options.RimWarSettingsWindow rwsw = new Options.RimWarSettingsWindow();
                    rwsw.page_ref = __instance;
                    Find.WindowStack.Add(rwsw);
                }
            }
        }

        public static void Pather_StartPath_WarObjects(Caravan_PathFollower __instance, int destTile, CaravanArrivalAction arrivalAction, ref bool __result, bool repathImmediately = false, bool resetPauseStatus = true)
        {
            if(__result == true)
            {
                if (arrivalAction is RimWar.Planet.CaravanArrivalAction_AttackWarObject)
                {
                    //Log.Message("assigning war object action: attack");
                    Caravan caravan = Traverse.Create(root: __instance).Field(name: "caravan").GetValue<Caravan>();
                    CaravanArrivalAction_AttackWarObject woAction = arrivalAction as CaravanArrivalAction_AttackWarObject;
                    RimWar.Planet.WorldUtility.Get_WCPT().AssignCaravanTargets(caravan, woAction.wo);
                }
                else if(arrivalAction is RimWar.Planet.CaravanArrivalAction_EngageWarObject)
                {
                    //Log.Message("assigning war object action: engage");
                    Caravan caravan = Traverse.Create(root: __instance).Field(name: "caravan").GetValue<Caravan>();
                    CaravanArrivalAction_EngageWarObject woAction = arrivalAction as CaravanArrivalAction_EngageWarObject;
                    RimWar.Planet.WorldUtility.Get_WCPT().AssignCaravanTargets(caravan, woAction.wo);
                }
                else
                {
                    Caravan caravan = Traverse.Create(root: __instance).Field(name: "caravan").GetValue<Caravan>();
                    List<CaravanTargetData> ctdList = RimWar.Planet.WorldUtility.Get_WCPT().caravanTargetData;
                    for (int i = 0; i < ctdList.Count; i++)
                    {
                        if(ctdList[i].caravan == caravan)
                        {
                            ctdList.Remove(ctdList[i]);
                        }
                    }
                }
            }
        }

        public static void AttackNow_SettlementReinforcement_Postfix(SettlementUtility __instance, Caravan caravan, RimWorld.Planet.Settlement settlement)
        {
            RimWar.Planet.Settlement rwSettlement = WorldUtility.GetRimWarSettlementAtTile(settlement.Tile);
            if(rwSettlement != null && rwSettlement.RimWarPoints > 1050)
            {
                WorldUtility.CreateWarband((rwSettlement.RimWarPoints - 1000), WorldUtility.GetRimWarDataForFaction(rwSettlement.Faction), rwSettlement, rwSettlement.Tile, rwSettlement.Tile, WorldObjectDefOf.Settlement);
            }
        }

        public static bool CallForAid_Replacement_Patch(Map map, Faction faction)
        {
            Faction ofPlayer = Faction.OfPlayer;
            int goodwillChange = -25;
            bool canSendMessage = false;
            string reason = "GoodwillChangedReason_RequestedMilitaryAid".Translate();
            faction.TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage, true, reason);
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = map;
            incidentParms.faction = faction;
            incidentParms.raidArrivalModeForQuickMilitaryAid = true;
            incidentParms.points = DiplomacyTuning.RequestedMilitaryAidPointsRange.RandomInRange;
            faction.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
            RimWar.Planet.Settlement rwdTown = WorldUtility.GetClosestRimWarSettlementOfFaction(faction, map.Tile, 40);
            if (rwdTown != null)
            {
                RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
                int pts = Mathf.RoundToInt(rwdTown.RimWarPoints / 2);
                if (rwd.CanLaunch)
                {
                    WorldUtility.CreateLaunchedWarband(pts, rwd, rwdTown, rwdTown.Tile, map.Tile, WorldObjectDefOf.Settlement);
                }
                else
                {
                    WorldUtility.CreateWarband(pts, rwd, rwdTown, rwdTown.Tile, map.Tile, WorldObjectDefOf.Settlement);
                }
                rwdTown.RimWarPoints = pts;
                return false;
            }
            return true;            
        }

        public static bool IncidentQueueAdd_Replacement_Prefix(IncidentQueue __instance, IncidentDef def, int fireTick, IncidentParms parms = null, int retryDurationTicks = 0)
        {
            if(def == IncidentDefOf.TraderCaravanArrival && fireTick == (Find.TickManager.TicksGame + 120000))
            {
                RimWar.Planet.Settlement rwdTown = WorldUtility.GetClosestRimWarSettlementOfFaction(parms.faction, parms.target.Tile, 40);
                if(rwdTown != null)
                {
                    WorldUtility.CreateTrader(Mathf.RoundToInt(rwdTown.RimWarPoints / 2), WorldUtility.GetRimWarDataForFaction(rwdTown.Faction), rwdTown, rwdTown.Tile, parms.target.Tile, WorldObjectDefOf.Settlement);
                    rwdTown.RimWarPoints = Mathf.RoundToInt(rwdTown.RimWarPoints / 2);
                    return false;
                }
            }
            return true;
        }

        public static bool TryAffectGoodwillWith_Reduction_Prefix(Faction __instance, Faction other, ref int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null, GlobalTargetInfo? lookTarget = default(GlobalTargetInfo?))
        {
            //if((__instance.IsPlayer || other.IsPlayer))
            //{
            //    if (reason == null || (reason != null && reason != "Rim War"))
            //    {
            //        goodwillChange = Mathf.RoundToInt(goodwillChange / 5);
            //    }
            //}
            return true;
        }

        public static bool TryResolveParms_Points_Prefix(IncidentParms parms)
        {
            if(parms.points <= 1000)
            {
                return true;
            }
            return false;
        }

        public static bool Caravan_Give_Prefix(Caravan caravan, List<ThingCount> demands, List<Pawn> attackers)
        {
            List<WarObject> warObject = WorldUtility.GetHostileWarObjectsInRange(caravan.Tile, 1, caravan.Faction);
            //Log.Message("checking action give");
            if(warObject != null && warObject.Count > 0 && attackers != null && attackers.Count > 0)
            {
                //Log.Message("found " + warObject.Count + " warObjects");
                for(int i =0; i < warObject.Count; i++)
                {
                    if(warObject[i].Faction !=null )//&& warObject[i].Faction == attackers[0].Faction)
                    {
                        float marketValue = 0;
                        for(int j =0; j < demands.Count; j++)
                        {
                            marketValue += (demands[j].Thing.MarketValue * demands[j].Count);
                        }
                        //Log.Message("market value of caravan ransom is " + marketValue);
                        int points = warObject[i].RimWarPoints + Mathf.RoundToInt(marketValue / 20);
                        if (warObject[i].ParentSettlement != null)
                        {
                            ConsolidatePoints reconstitute = new ConsolidatePoints(points, Mathf.RoundToInt(Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, warObject[i].ParentSettlement.Tile) * warObject[i].TicksPerMove) + Find.TickManager.TicksGame);
                            warObject[i].ParentSettlement.SettlementPointGains.Add(reconstitute);
                            warObject[i].ImmediateAction(null);
                        }
                        break;
                    }
                }
            }

            return true;
        }

        public static bool IncidentWorker_Prefix(IncidentWorker __instance, IncidentParms parms, ref bool __result)
        {
            //Log.Message("def " + __instance.def);
            if (__instance.def == null)
            {
                Traverse.Create(root: __instance).Field(name: "def").SetValue(IncidentDefOf.RaidEnemy);
                __instance.def = IncidentDefOf.RaidEnemy;
            }
            //Log.Message("def tale " + __instance.def.tale);
            //Log.Message("def category tale " + __instance.def.category.tale);
            return true;
        }

        //private static void DrawFactionRow_WithFactionPoints_Postfix(Faction faction, float rowY, Rect fillRect, ref float __result)
        //{
        //    if (!Prefs.DevMode)
        //    {
        //        Rect rect = new Rect(35f, rowY + __result, 250f, 80f);
        //        StringBuilder stringBuilder = new StringBuilder();
        //        string text = stringBuilder.ToString();
        //        float width = fillRect.width - rect.xMax;
        //        float num = Text.CalcHeight(text, width);
        //        float num2 = Mathf.Max(80f, num);
        //        Rect position = new Rect(10f, rowY + 10f, 15f, 15f);
        //        Rect rect2 = new Rect(0f, rowY + __result, fillRect.width, num2);
        //        if (Mouse.IsOver(rect2))
        //        {
        //            GUI.DrawTexture(rect2, TexUI.HighlightTex);
        //        }
        //        Text.Font = GameFont.Small;
        //        Text.Anchor = TextAnchor.UpperLeft;
        //        Widgets.DrawRectFast(position, faction.Color);
        //        string label = "RW_FactionPower".Translate(WorldUtility.GetRimWarDataForFaction(faction) == null ? 0 : WorldUtility.GetRimWarDataForFaction(faction).TotalFactionPoints);
        //        label += "\n" + "RW_FactionBehavior".Translate(WorldUtility.GetRimWarDataForFaction(faction).behavior.ToString());
        //        Widgets.Label(rect, label);
        //        if (!faction.IsPlayer)
        //        {

        //        }
        //        __result += num2;
        //    }
        //}

        private static void Settlement_InspectString_WithPoints_Postfix(RimWorld.Planet.Settlement __instance, ref string __result)
        {
            if (!__instance.Faction.def.hidden)
            {
                RimWarData rwData = WorldUtility.GetRimWarDataForFaction(__instance.Faction);
                if (rwData != null)
                {
                    string text = "";
                    if (!__result.NullOrEmpty())
                    {
                        text += "\n";
                    }
                    for (int i = 0; i < rwData.FactionSettlements.Count; i++)
                    {
                        if(rwData.FactionSettlements[i].Tile == __instance.Tile)
                        {
                            text += "RW_SettlementPoints".Translate(rwData.FactionSettlements[i].RimWarPoints + "\n" + rwData.behavior.ToString());
                            break;
                        }
                    }
                    __result += text;
                }               
            }
        }

        [HarmonyPatch(typeof(WorldPathPool), "GetEmptyWorldPath", null)]
        public class WorldPathPool_Prefix_Patch
        {
            public static bool Prefix(WorldPathPool __instance, ref WorldPath __result)
            {
                List<WorldPath> paths = Traverse.Create(root: __instance).Field(name: "paths").GetValue<List<WorldPath>>();
                for (int i = 0; i < paths.Count; i++)
                {
                    if (!paths[i].inUse)
                    {
                        paths[i].inUse = true;
                        __result = paths[i];
                        return false;
                    }
                }
                if (paths.Count > Find.WorldObjects.CaravansCount + 2 + (Find.WorldObjects.RoutePlannerWaypointsCount - 1))
                {
                    //Log.ErrorOnce("WorldPathPool leak: more paths than caravans. Force-recovering.", 664788);
                    paths.Clear();
                }
                WorldPath worldPath = new WorldPath();
                paths.Add(worldPath);
                worldPath.inUse = true;
                __result = worldPath;
                return false;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_Ambush_EnemyFaction), "CanFireNowSub", null)]
        public class CanFireNow_Ambush_EnemyFaction_RemovalPatch
        {
            public static bool Prefix(IncidentWorker_Ambush_EnemyFaction __instance, IncidentParms parms, ref bool __result)
            {
                if (__instance != null && __instance.def.defName != "VisitorGroup" && __instance.def.defName != "VisitorGroupMax" && !__instance.def.defName.Contains("Cult") && parms.quest == null && !parms.forced && !__instance.def.workerClass.ToString().StartsWith("Rumor_Code"))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_CaravanDemand), "CanFireNowSub", null)]
        public class CanFireNow_CaravanDemand_RemovalPatch
        {
            public static bool Prefix(IncidentWorker_CaravanDemand __instance, IncidentParms parms, ref bool __result)
            {
                if (__instance != null && __instance.def.defName != "VisitorGroup" && __instance.def.defName != "VisitorGroupMax" && !__instance.def.defName.Contains("Cult") && parms.quest == null && !parms.forced && !__instance.def.workerClass.ToString().StartsWith("Rumor_Code"))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_CaravanMeeting), "CanFireNowSub", null)]
        public class CanFireNow_CaravanMeeting_RemovalPatch
        {
            public static bool Prefix(IncidentWorker_CaravanMeeting __instance, IncidentParms parms, ref bool __result)
            {
                if (__instance != null && __instance.def.defName != "VisitorGroup" && __instance.def.defName != "VisitorGroupMax" && !__instance.def.defName.Contains("Cult") && parms.quest == null && !parms.forced && !__instance.def.workerClass.ToString().StartsWith("Rumor_Code"))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(IncidentWorker_QuestPeaceTalks), "CanFireNowSub", null)]
        //public class CanFireNow_QuestPeaceTalks_RemovalPatch
        //{
        //    public static bool Prefix(IncidentWorker_QuestPeaceTalks __instance, IncidentParms parms, ref bool __result)
        //    {
        //        __result = false;
        //        return false;
        //    }
        //}

        [HarmonyPatch(typeof(IncidentWorker_PawnsArrive), "CanFireNowSub", null)]
        public class CanFireNow_PawnsArrive_RemovalPatch
        {
            public static bool Prefix(IncidentWorker_PawnsArrive __instance, IncidentParms parms, ref bool __result)
            {
                if (__instance != null && __instance.def.defName != "VisitorGroup" && __instance.def.defName != "VisitorGroupMax" && !__instance.def.defName.Contains("Cult") && parms.quest == null && !parms.forced && !__instance.def.workerClass.ToString().StartsWith("Rumor_Code"))
                {
                    if (__instance.def == IncidentDefOf.RaidEnemy || __instance.def == IncidentDefOf.RaidFriendly || __instance.def == IncidentDefOf.TraderCaravanArrival)
                    {
                        __result = false;
                        return false;
                    }
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(IncidentWorker), "CanFireNow", null)]
        //public class CanFireNow_Monitor
        //{
        //    public static bool Prefix(IncidentWorker __instance, IncidentParms parms, ref bool __result)
        //    {                
        //        Log.Message("incident of " + __instance.def.defName + " with type " + __instance.GetType().ToString() + " attempting to fire with points " + parms.points + " against " + parms.target.ToString());
        //        return true;
        //    }
        //}
    }
}
