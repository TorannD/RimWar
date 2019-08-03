using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWar.Planet
{
    public class LaunchedWarband : LaunchedWarObject
    {
        private int lastEventTick = 0;
        private bool movesAtNight = false;
        private int ticksPerMove = 3300;
        private int searchTick = 60;               

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.movesAtNight, "movesAtNight", false, false);
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 3300, false);                       
        }

        public override void Tick()
        {
            base.Tick();
        }

        public LaunchedWarband()
        {

        }

        public override int RimWarPoints { get => base.RimWarPoints; set => base.RimWarPoints = value; }        
     

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append(base.GetInspectString());            

            if (Find.World.worldObjects.AnySettlementAt(this.destinationTile))
            {
                WorldObject wo = Find.World.worldObjects.ObjectsAt(this.destinationTile).FirstOrDefault();
                if (wo.Faction != this.Faction)
                {
                    stringBuilder.Append("RW_WarbandInspectString".Translate(this.Name, "RW_Attacking".Translate(), wo.Label));
                }
                else
                {
                    stringBuilder.Append("RW_WarbandInspectString".Translate(this.Name, "RW_ReturningTo".Translate(), wo.Label));
                }
            }

            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append("RW_CombatPower".Translate(this.RimWarPoints));

            return stringBuilder.ToString();
        }

        public override void ArrivalAction()
        {
            //Log.Message("beginning arrival actions");
            WorldObject wo = Find.World.worldObjects.ObjectsAt(this.destinationTile).FirstOrDefault();
            if(wo.Faction != this.Faction)
            {
                if (wo.Faction.HostileTo(this.Faction))
                {
                    if (wo.Faction == Faction.OfPlayer)
                    {
                        //Do Raid
                        RimWorld.Planet.Settlement playerSettlement = Find.World.worldObjects.SettlementAt(this.Tile);
                        Caravan playerCaravan = Find.World.worldObjects.PlayerControlledCaravanAt(this.Tile);
                        if (playerSettlement != null)
                        {
                            //Raid Player Map
                            IncidentUtility.DoRaidWithPoints(this.RimWarPoints, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), PawnsArrivalModeDefOf.EdgeWalkIn);
                        }
                        else if (playerCaravan != null)
                        {
                            //Raid player caravan
                            //IncidentUtility.DoCaravanAttackWithPoints(this, playerCaravan, this.rimwarData, PawnsArrivalModeDefOf.EdgeWalkIn);
                        }
                    }
                    else
                    {
                        Settlement settlement = WorldUtility.GetRimWarSettlementAtTile(this.Tile);
                        if (settlement != null)
                        {
                            //IncidentUtility.ResolveWarbandAttackOnSettlement(this, this.ParentSettlement, settlement, WorldUtility.GetRimWarDataForFaction(this.Faction));
                        }
                    }                
                }
            }
            else
            {
                //Log.Message("this tile: " + this.Tile + " parent settlement tile: " + this.ParentSettlement.Tile);
                if(this.Tile == ParentSettlement.Tile)
                {
                    if(Find.World.worldObjects.AnyMapParentAt(this.Tile))
                    {
                        //reinforce
                        //Log.Message("attempting to reinforce");
                        //Log.Message("map is spawn " + Find.World.worldObjects.MapParentAt(this.Tile).Spawned);
                        //Log.Message("map " + Find.World.worldObjects.MapParentAt(this.Tile).Map + " has faction " + Find.World.worldObjects.MapParentAt(this.Tile).Faction);
                        this.ParentSettlement.RimWarPoints += this.RimWarPoints;
                    }
                    else
                    {
                        //Log.Message("parent settlement points: " + this.ParentSettlement.RimWarPoints);
                        if (wo.Faction != this.Faction) //could happen if parent town is taken over while army is away, in which case - perform another raid
                        {

                        }
                        this.ParentSettlement.RimWarPoints += this.RimWarPoints;
                    }
                }
            }
            //Log.Message("ending arrival actions");
            base.ArrivalAction();
        }      
    }
}
