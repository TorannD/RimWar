using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace RimWar.Planet
{
    public class WorldUtility
    {
        public IncidentParms parms = new IncidentParms();

        private static List<Pawn> tmpPawns = new List<Pawn>();

        public static void CreateWarObject(int points, Faction faction, int startingTile, int destinationTile, WorldObjectDef type)
        {
            Log.Message("creating war object");
            WarObject warObject = new WarObject();
            warObject = MakeWarObject(10, faction, startingTile, destinationTile, type, true);
            if (!warObject.pather.Moving && warObject.Tile != destinationTile)
            {
                warObject.pather.StartPath(destinationTile, true, true);
                warObject.pather.nextTileCostLeft /= 2f;
                warObject.tweener.ResetTweenedPosToRoot();
            }

        }

        public static WarObject MakeWarObject(int points, Faction faction, int startingTile, int destinationTile, WorldObjectDef type, bool addToWorldPawnsIfNotAlready)
        {
            Log.Message("making world object");
            WarObject warObject = (WarObject)WorldObjectMaker.MakeWorldObject(RimWarDefOf.RW_WarObject);
            if (startingTile >= 0)
            {
                warObject.Tile = startingTile;
            }
            warObject.SetFaction(faction);
            if (startingTile >= 0)
            {
                Find.WorldObjects.Add(warObject);
            }            
            warObject.Name = "default war object";
            warObject.SetUniqueId(Find.UniqueIDsManager.GetNextCaravanID());
            
            return warObject;
        }

        public static void CreateWarband(int power, Faction faction, int startingTile, int destinationTile, IIncidentTarget target)
        {
            Log.Message("generating warband");
            Log.Message("storyteller threat points of target is " + StorytellerUtility.DefaultThreatPointsNow(target));
            Warband_Caravan warband = new Warband_Caravan();
            IncidentParms parms = new IncidentParms();
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            parms.faction = faction;
            parms.generateFightersOnly = true;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            parms.target = target;
            parms = ResolveRaidStrategy(parms, combat);
            parms.points = AdjustedRaidPoints((float)power, parms.raidArrivalMode, parms.raidStrategy, faction, combat);
            Log.Message("adjusted points " + parms.points);
            PawnGroupMakerParms warbandPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms);
            List<Pawn> warbandPawnList = PawnGroupMakerUtility.GeneratePawns(warbandPawnGroupMakerParms).ToList();
            if(warbandPawnList.Count == 0)
            {
                Log.Error("Tried to create a warband without points");
                return;
            }
            //for(int i = 0; i < warbandPawnList.Count; i++)
            //{                
            //    warband.AddPawn(warbandPawnList[i], true);
            //    warbandPawnList[i].Notify_PassedToWorld();                
            //}
            warband = MakeWarband(warbandPawnList, faction, startingTile, true);
            warband.parms = parms;
            if(!warband.pather.Moving && warband.Tile != destinationTile)
            {
                warband.pather.StartPath(destinationTile, null, repathImmediately: true);
                warband.pather.nextTileCostLeft /= 2f;
                warband.tweener.ResetTweenedPosToRoot();
            }
            Log.Message("end create warband");
        }

        public static Warband_Caravan MakeWarband(List<Pawn> pawns, Faction faction, int startingTile, bool addToWorldPawnsIfNotAlready)
        {
            Log.Message("making world object warband");
            tmpPawns.Clear();
            tmpPawns.AddRange(pawns);
            //Caravan warband = (Caravan)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Caravan);
            Warband_Caravan warband = (Warband_Caravan)WorldObjectMaker.MakeWorldObject(RimWarDefOf.RW_Warband);
            if (startingTile >= 0)
            {
                warband.Tile = startingTile;
            }
            warband.SetFaction(faction);
            if (startingTile >= 0)
            {
                Find.WorldObjects.Add(warband);
            }
            for (int i = 0; i < tmpPawns.Count; i++)
            {
                Pawn pawn = tmpPawns[i];
                if (pawn.Dead)
                {
                    Log.Warning("Tried to form a caravan with a dead pawn " + pawn);
                }
                else
                {
                    Log.Message("adding pawn " + pawn.LabelShort);
                    warband.AddPawn(pawn, addToWorldPawnsIfNotAlready);
                    if (addToWorldPawnsIfNotAlready && !pawn.IsWorldPawn())
                    {
                        Find.WorldPawns.PassToWorld(pawn);
                    }
                }
            }
            warband.Name = CaravanNameGenerator.GenerateCaravanName(warband);
            tmpPawns.Clear();
            warband.SetUniqueId(Find.UniqueIDsManager.GetNextCaravanID());
            
            return warband;
        }

        public static float AdjustedRaidPoints(float points, PawnsArrivalModeDef raidArrivalMode, RaidStrategyDef raidStrategy, Faction faction, PawnGroupKindDef groupKind)
        {
            if (raidArrivalMode.pointsFactorCurve != null)
            {
                points *= raidArrivalMode.pointsFactorCurve.Evaluate(points);
            }
            if (raidStrategy.pointsFactorCurve != null)
            {
                points *= raidStrategy.pointsFactorCurve.Evaluate(points);
            }
            points = Mathf.Max(points, raidStrategy.Worker.MinimumPoints(faction, groupKind) * 1.05f);
            return points;
        }

        public static IncidentParms ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy == null)
            {
                Map map = (Map)parms.target;
                if (!(from d in DefDatabase<RaidStrategyDef>.AllDefs
                      where d.Worker.CanUseWith(parms, groupKind) && (parms.raidArrivalMode != null || (d.arriveModes != null && d.arriveModes.Any((PawnsArrivalModeDef x) => x.Worker.CanUseWith(parms))))
                      select d).TryRandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionWeight(map, parms.points), out parms.raidStrategy))
                {
                    Log.Error("No raid stategy for " + parms.faction + " with points " + parms.points + ", groupKind=" + groupKind + "\nparms=" + parms);
                    parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                    if (!Prefs.DevMode)
                    {
                        parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                    }
                }
            }
            return parms;
        }
    }
}
