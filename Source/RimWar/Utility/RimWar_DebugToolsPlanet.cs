using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using RimWorld.BaseGen;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWar.Planet;

namespace RimWar.Utility
{
    public static class RimWar_DebugToolsPlanet
    {
        [DebugAction("Rim War", "Add 1k pts", actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void Add1000RWP()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null && s.Faction != Faction.OfPlayer)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null && rwsc.parent.Faction != Faction.OfPlayer)
                    {
                        rwsc.RimWarPoints += 1000;
                    }
                }
                WarObject rwo = Find.WorldObjects.WorldObjectAt(tile, RimWarDefOf.RW_WarObject) as WarObject;
                if(rwo != null)
                {
                    rwo.RimWarPoints += 1000;
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnTrader()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptTradeMission(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, false, true);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnTraderToPlayer()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptTradeMission(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, true, true);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnScout()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptScoutMission(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, false, false, true);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnScoutToPlayerTown()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptScoutMission(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, true, false, true);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnScoutToPlayerCaravan()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptScoutMission(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, false, true, true);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnWarband()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptWarbandActionAgainstTown(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, false, true);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnWarbandToPlayer()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptWarbandActionAgainstTown(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, true, true);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnLaunchedWarband()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptLaunchedWarbandAgainstTown(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, false, true);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnLaunchedWarbandToPlayer()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptLaunchedWarbandAgainstTown(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, true, true);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnSettler()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptSettlerMission(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, false, false);
                    }
                }
            }
        }

        [DebugAction("Rim War", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void SpawnSettlerIgnoreProximity()
        {
            int tile = GenWorld.MouseTile();
            if (tile < 0 || Find.World.Impassable(tile))
            {
                Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
            }
            else
            {
                RimWorld.Planet.Settlement s = Find.WorldObjects.SettlementAt(tile);
                if (s != null)
                {
                    RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(tile);
                    if (rwsc != null)
                    {
                        WorldUtility.Get_WCPT().AttemptSettlerMission(WorldUtility.GetRimWarDataForFaction(s.Faction), s, rwsc, false, true);
                    }
                }
            }
        }

        [DebugAction("Rim War - Debug Log", "War Object Report", actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void LogWarObjectData()
        {
            int rwoCount = 0;
            int rwoPts = 0;
            StringBuilder str = new StringBuilder();
            List<RimWarData> rwdList = WorldUtility.GetRimWarData();
            if (rwdList != null)
            {
                if (rwdList.Count > 0)
                {
                    List<WorldObject> woList = Find.WorldObjects.AllWorldObjects;
                    if (woList != null && woList.Count > 0)
                    {
                        for (int i = 0; i < woList.Count; i++)
                        {
                            str.Clear();
                            if (woList[i] is WarObject)
                            {
                                rwoCount++;
                                WarObject rwo = woList[i] as WarObject;
                                str.Append(rwo.Name + " ID: " + rwo.ID);
                                if (rwo.rimwarData == null || rwo.rimwarData.WorldSettlements == null || rwo.rimwarData.WorldSettlements.Count <= 0)
                                {
                                    Log.Warning("Invalid Rim War Data!");
                                }
                                if (rwo.ParentSettlement != null)
                                {
                                    rwoPts += rwo.RimWarPoints;
                                    if (rwo.ParentSettlement != null)
                                    {
                                        str.Append(" Parent: " + rwo.ParentSettlement.Label + " ID " + rwo.ParentSettlement.ID);
                                        if (rwo.ParentSettlement.Destroyed)
                                        {
                                            Log.Warning("Parent Settlement is Destroyed!");
                                        }
                                    }
                                    else
                                    {
                                        Log.Warning("Parent Settlement has Null World Object!");
                                    }
                                }
                                else
                                {
                                    Log.Warning("No Parent Settlement!");
                                }
                                if (rwo.UseDestinationTile)
                                {
                                    if (Find.WorldObjects.AnySettlementAt(rwo.DestinationTile))
                                    {
                                        Log.Warning("Settlement detected at destination!");
                                    }
                                }
                                else if (rwo.DestinationTarget != null)
                                {
                                    str.Append(" Destination " + rwo.DestinationTarget.Label + " ID " + rwo.DestinationTarget.ID);
                                    if (!rwo.DestinationTarget.Destroyed)
                                    {
                                        if (!rwo.canReachDestination)
                                        {
                                            Log.Warning("Pather unable to reach destination!");
                                        }
                                        int distance = Mathf.RoundToInt(Find.WorldGrid.ApproxDistanceInTiles(rwo.Tile, rwo.DestinationTarget.Tile));
                                        if (distance > 100)
                                        {
                                            Log.Warning("Object travel distance is " + distance);
                                        }
                                    }
                                    else
                                    {
                                        Log.Warning("Destination destroyed!");
                                    }
                                }
                                else
                                {
                                    Log.Warning("No Destination or Destination is Null!");
                                }
                                Log.Message("" + str);
                            }
                        }
                    }
                    else
                    {
                        Log.Warning("Debug: no world objects found");
                    }
                }
                else
                {
                    Log.Warning("Debug: RWD count = 0");
                }
                Log.Message("Total objects: " + rwoCount);
                Log.Message("Total points: " + rwoPts);
            }
            else
            {
                Log.Warning("Debug: Rim War Data was null.");
            }
        }

        [DebugAction("Rim War - Debug Log", "Settlement Summary Log", actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void LogRimWarSettlementData()
        {
            Debug_FixRimWarSettlements(true, false);
        }

        [DebugAction("Rim War - Debug", "Debug Settlements", actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void DebugRimWarSettlementData()
        {
            Debug_FixRimWarSettlements(true, true);
        }



        public static void Debug_FixRimWarSettlements(bool generateReport = false, bool cleanupErrors = false)
        {
            int rwsCount = 0;
            int wosCount = 0;
            int rws_no_wosCount = 0;
            int wos_no_rwsCount = 0;
            int factionMismatchCount = 0;

            List<WorldObject> woList = Find.WorldObjects.AllWorldObjects;
            List<WorldObject> wosList = new List<WorldObject>();
            wosList.Clear();
            for (int i = 0; i < woList.Count; i++)
            {
                RimWorld.Planet.Settlement wos = woList[i] as RimWorld.Planet.Settlement;
                if (wos != null)
                {
                    wosList.Add(wos);
                    wosCount++;
                    RimWarSettlementComp rws = WorldUtility.GetRimWarSettlementAtTile(wos.Tile);
                    if (rws != null)
                    {
                        if (wos.Destroyed)
                        {
                            if (generateReport) { Log.Warning(wos.Label + " destroyed but has RWSC"); }
                            if (cleanupErrors)
                            {
                                if (generateReport) { Log.Message("Cleaning RWS..."); }
                                //WorldUtility.GetRimWarDataForFaction(wos.Faction)?.WorldSettlements?.Remove(rws);                                
                            }
                        }
                        else if (wos.Faction != rws.parent.Faction)
                        {
                            factionMismatchCount++;
                            if (generateReport) { Log.Warning(wos.Label + " of Faction " + wos.Faction + " different from RWS Faction " + rws.parent.Faction); }
                            if (cleanupErrors)
                            {
                                if (generateReport) { Log.Message("Removing RWS from " + rws.parent.Faction + "..."); }
                                //WorldUtility.GetRimWarDataForFaction(rws.parent.Faction)?.WorldSettlements?.Remove(rws);
                                if (generateReport) { Log.Message("Adding RWS to " + wos.Faction + "..."); }
                                //rws.Faction = wos.Faction;
                                //WorldUtility.GetRimWarDataForFaction(wos.Faction)?.WorldSettlements?.Add(rws);
                            }
                        }
                    }
                    else
                    {
                        wos_no_rwsCount++;
                        if (generateReport) { Log.Warning("" + wos.Label + " has no RWS"); }
                        if (cleanupErrors)
                        {
                            if (generateReport) { Log.Message("Generating RWS for " + wos.Label + "..."); }
                            WorldUtility.CreateRimWarSettlement(WorldUtility.GetRimWarDataForFaction(wos.Faction), wos);
                        }
                    }
                }
            }

            List<RimWar.Planet.Settlement> rwsList = new List<RimWar.Planet.Settlement>();
            rwsList.Clear();
            List<RimWarData> rwdList = WorldUtility.Get_WCPT().RimWarData;
            for (int i = 0; i < rwdList.Count; i++)
            {
                RimWarData rwd = rwdList[i];
                if (rwd.WorldSettlements != null)
                {
                    for (int j = 0; j < rwd.WorldSettlements.Count; j++)
                    {
                        RimWarSettlementComp rws = rwd.WorldSettlements[j].GetComponent<RimWarSettlementComp>();
                        rwsCount++;
                        int wosHere = 0;
                        List<RimWorld.Planet.Settlement> wosHereList = new List<RimWorld.Planet.Settlement>();
                        wosHereList.Clear();
                        for (int k = 0; k < wosList.Count; k++)
                        {
                            if (wosList[k].Tile == rws.parent.Tile)
                            {
                                wosHere++;
                                wosHereList.Add(wosList[k] as RimWorld.Planet.Settlement);
                                if (wosList[k].Faction != rws.parent.Faction)
                                {
                                    factionMismatchCount++;
                                    if (generateReport) { Log.Warning(wosList[k].Label + " of Faction " + wosList[k].Faction + " different from RWS Faction " + rws.parent.Faction); }
                                    if (cleanupErrors)
                                    {
                                        if (generateReport) { Log.Message("Removing RWS from " + rws.parent.Faction + "..."); }
                                        //WorldUtility.GetRimWarDataForFaction(rws.parent.Faction)?.WorldSettlements?.Remove(rws);
                                        if (generateReport) { Log.Message("Adding RWS to " + wosList[k].Faction + "..."); }
                                        //rws.Faction = wosList[k].Faction;
                                        //WorldUtility.GetRimWarDataForFaction(wosList[k].Faction)?.WorldSettlements?.Add(rws);
                                    }
                                }
                            }
                        }
                        if (wosHere == 0)
                        {
                            rws_no_wosCount++;
                            if (generateReport) { Log.Warning("No settlement found at " + Find.WorldGrid.LongLatOf(rws.parent.Tile)); }
                            if (cleanupErrors)
                            {
                                if (generateReport) { Log.Warning("Removing RWS..."); }
                                //rwd.FactionSettlements.Remove(rws);
                            }
                        }
                        if (wosHere > 1)
                        {
                            if (generateReport) { Log.Warning("Stacked settlements (" + wosHere + ") found at " + Find.WorldGrid.LongLatOf(rws.parent.Tile)); }
                            if (cleanupErrors)
                            {
                                while (wosHereList.Count > 1)
                                {
                                    if (generateReport) { Log.Message("Destroying settlement..."); }
                                    RimWorld.Planet.Settlement wosHereDes = wosHereList[0];
                                    wosHereList.Remove(wosHereDes);
                                    if (!wosHereDes.Destroyed)
                                    {
                                        wosHereDes.Destroy();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (generateReport) { Log.Warning("Found null RWD"); }
                    if (cleanupErrors)
                    {
                        if (generateReport) { Log.Message("Removing RWD..."); }
                        rwdList.Remove(rwd);
                    }
                }
            }

            if (generateReport)
            {
                bool errors = wos_no_rwsCount != 0 || rws_no_wosCount != 0 || factionMismatchCount != 0;
                Log.Message("Rim War Settlement Count: " + rwsCount);
                Log.Message("World Settlement Count: " + wosCount);
                if (!errors) { Log.Message("No errors found."); }
                if (wos_no_rwsCount > 0) { Log.Warning("Settlements without RWS component: " + wos_no_rwsCount); }
                if (rws_no_wosCount > 0) { Log.Warning("Rim War components without Settlement: " + rws_no_wosCount); }
                if (factionMismatchCount > 0) { Log.Warning("Faction mismatches: " + factionMismatchCount); }
            }
        }

        [DebugAction("Rim War - Debug", "Reset Units", actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void DebugResetAllMobileUnits()
        {
            int resetCount = 0;
            List<WorldObject> woList = Find.WorldObjects.AllWorldObjects;
            if (woList != null && woList.Count > 0)
            {
                for (int i = 0; i < woList.Count; i++)
                {
                    WarObject rwo = woList[i] as WarObject;
                    if (rwo != null)
                    {
                        RimWarData rwd = WorldUtility.GetRimWarDataForFaction(rwo.Faction);
                        if (rwd != null && rwd.WorldSettlements != null && rwd.WorldSettlements.Count > 0)
                        {
                            resetCount++;
                            RimWorld.Planet.Settlement settlement = rwd.WorldSettlements.RandomElement();
                            if(settlement != null)
                            {
                                if(settlement.Destroyed)
                                {
                                    Log.Warning("Detected destroyed settlement in Rim War data for " + rwd.RimWarFaction.Name);
                                }
                                else
                                {
                                    RimWarSettlementComp rwsc = settlement.GetComponent<RimWarSettlementComp>();
                                    if(rwsc != null)
                                    {
                                        rwsc.RimWarPoints += rwo.RimWarPoints;
                                    }
                                    else
                                    {
                                        Log.Warning("Found no Rim War component for settlement " + settlement.Label);
                                        Log.Warning("Settlement in faction " + settlement.Faction);
                                        Log.Warning("Settlement defname " + settlement.def.defName);
                                    }
                                }
                            }
                            else
                            {
                                Log.Warning("Detected null settlement in Rim War data for " + rwd.RimWarFaction.Name);
                            }
                            if (!rwo.Destroyed)
                            {
                                rwo.Destroy();
                            }
                        }
                        else
                        {
                            Log.Warning("Tried to reset unit but no Faction data exists - cleaning up object.");
                            if (!rwo.Destroyed)
                            {
                                rwo.Destroy();
                            }
                        }
                    }
                }
                Log.Message("Reset " + resetCount + " Rim War units.");
            }
        }

        public static void ValidateAndResetSettlements()
        {
            int sCount = 0;
            int sPoints = GetPointsFromAllSettlements(out sCount);
            if (sCount > 0)
            {
                if (Mathf.RoundToInt(sPoints / sCount) < 110)
                {
                    DebugResetAllSettlements();
                }
            }
        }

        [DebugAction("Rim War - Debug", "Reset Settlements", actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        private static void DebugResetAllSettlements()
        {
            Log.Message("Reseting Rim War Settlement Points...");
            int totalSettlements = 0;
            int initialPoints = GetPointsFromAllSettlements(out totalSettlements);
            float yearMultiplier = 1f + (GenDate.YearsPassed * .1f);
            List<RimWarData> rwdList = WorldUtility.Get_WCPT().RimWarData;
            if(rwdList != null && rwdList.Count > 0)
            {
                for(int i = 0; i < rwdList.Count; i++)
                {
                    List<RimWarSettlementComp> rwscList = rwdList[i].WarSettlementComps;
                    if (rwscList != null && rwscList.Count > 0 && rwdList[i].behavior != RimWarBehavior.Player)
                    {
                        for(int j = 0; j < rwscList.Count; j++)
                        {
                            rwscList[j].RimWarPoints = Mathf.RoundToInt(WorldUtility.CalculateSettlementPoints(rwscList[j].parent, rwscList[j].parent.Faction) * Rand.Range(.5f, 1.5f) * yearMultiplier);
                        }
                    }
                }
            }            
            int adjustedPoints = GetPointsFromAllSettlements(out totalSettlements);
            Log.Message(totalSettlements + " settlements initially with " + initialPoints + "; " + Mathf.RoundToInt(initialPoints/totalSettlements) + " per settlement");
            Log.Message(totalSettlements + " settlements adjusted to " + adjustedPoints + "; " + Mathf.RoundToInt(adjustedPoints / totalSettlements) + " per settlement");
        }

        private static int GetPointsFromAllSettlements (out int totalSettlements)
        {
            int totalPoints = 0;
            totalSettlements = 0;
            List<RimWarData> rwdList = WorldUtility.Get_WCPT().RimWarData;
            if (rwdList != null && rwdList.Count > 0)
            {
                for (int i = 0; i < rwdList.Count; i++)
                {
                    List<RimWarSettlementComp> rwscList = rwdList[i].WarSettlementComps;
                    if (rwscList != null && rwscList.Count > 0 && rwdList[i].behavior != RimWarBehavior.Player)
                    {
                        for (int j = 0; j < rwscList.Count; j++)
                        {
                            totalPoints += rwscList[j].RimWarPoints;
                            totalSettlements++;
                        }
                    }
                }
            }
            return totalPoints;            
        }
    }
}
