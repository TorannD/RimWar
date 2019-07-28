using Harmony;
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

namespace RimWar.Harmony
{   
    [StaticConstructorOnStartup]
    internal class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(id: "rimworld.torann.rimwar");

            harmonyInstance.Patch(AccessTools.Method(typeof(FactionUIUtility), "DrawFactionRow", new Type[]
                {
                    typeof(Faction),
                    typeof(float),
                    typeof(Rect)
                }, null), null, new HarmonyMethod(typeof(HarmonyPatches), "DrawFactionRow_WithFactionPoints_Postfix", null), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(SettlementBase), "GetInspectString", new Type[]
                {
                }, null), null, new HarmonyMethod(typeof(HarmonyPatches), "SettlementBase_InspectString_WithPoints_Postfix", null), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(IncidentWorker), "TryExecute", new Type[]
                {
                    typeof(IncidentParms)
                }, null), new HarmonyMethod(typeof(HarmonyPatches), "IncidentWorker_Prefix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(IncidentWorker_CaravanDemand), "ActionGive", new Type[]
                {
                    typeof(Caravan),
                    typeof(List<ThingCount>),
                    typeof(List<Pawn>)
                }, null), new HarmonyMethod(typeof(HarmonyPatches), "Caravan_Give_Prefix", null), null, null);
        }

        public static bool Caravan_Give_Prefix(Caravan caravan, List<ThingCount> demands, List<Pawn> attackers)
        {
            List<Warband> warband = WorldUtility.GetHostileWarbandsInRange(caravan.Tile, 2, caravan.Faction);
            Log.Message("checking action give");
            if(warband != null && warband.Count > 0 && attackers != null && attackers.Count > 0)
            {
                Log.Message("found " + warband.Count + " warbands");
                for(int i =0; i < warband.Count; i++)
                {
                    if(warband[i].Faction !=null )//&& warband[i].Faction == attackers[0].Faction)
                    {
                        float marketValue = 0;
                        for(int j =0; j < demands.Count; j++)
                        {
                            marketValue += (demands[j].Thing.MarketValue * demands[j].Count);
                        }
                        Log.Message("market value of caravan ransom is " + marketValue);
                        int points = warband[i].RimWarPoints + Mathf.RoundToInt(marketValue / 20);
                        if (warband[i].ParentSettlement != null)
                        {
                            ConsolidatePoints reconstitute = new ConsolidatePoints(points, Mathf.RoundToInt(Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, warband[i].ParentSettlement.Tile) * warband[i].TicksPerMove) + Find.TickManager.TicksGame);
                            warband[i].ParentSettlement.SettlementPointGains.Add(reconstitute);
                            warband[i].ImmediateAction(null);
                        }
                        break;
                    }
                }
            }

            return true;
        }

        public static bool IncidentWorker_Prefix(IncidentWorker __instance, IncidentParms parms, ref bool __result)
        {
            Log.Message("def " + __instance.def);
            if (__instance.def == null)
            {
                Traverse.Create(root: __instance).Field(name: "def").SetValue(IncidentDefOf.RaidEnemy);
                __instance.def = IncidentDefOf.RaidEnemy;
            }
            Log.Message("def tale " + __instance.def.tale);
            Log.Message("def category tale " + __instance.def.category.tale);
            return true;
        }

        private static void DrawFactionRow_WithFactionPoints_Postfix(Faction faction, float rowY, Rect fillRect, ref float __result)
        {
            Rect rect = new Rect(35f, rowY + __result, 250f, 80f);
            StringBuilder stringBuilder = new StringBuilder();
            string text = stringBuilder.ToString();
            float width = fillRect.width - rect.xMax;
            float num = Text.CalcHeight(text, width);
            float num2 = Mathf.Max(80f, num);
            Rect position = new Rect(10f, rowY + 10f, 15f, 15f);
            Rect rect2 = new Rect(0f, rowY + __result, fillRect.width, num2);
            if (Mouse.IsOver(rect2))
            {
                GUI.DrawTexture(rect2, TexUI.HighlightTex);
            }
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.DrawRectFast(position, faction.Color);
            string label = "RW_FactionPower".Translate(WorldUtility.GetRimWarDataForFaction(faction) == null ? 0 : WorldUtility.GetRimWarDataForFaction(faction).TotalFactionPoints);
            label += "\n" + "RW_FactionBehavior".Translate(WorldUtility.GetRimWarDataForFaction(faction).behavior.ToString());
            Widgets.Label(rect, label);
            if (!faction.IsPlayer)
            {

            }
            __result += num2;
        }

        private static void SettlementBase_InspectString_WithPoints_Postfix(SettlementBase __instance, ref string __result)
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
    }
}
