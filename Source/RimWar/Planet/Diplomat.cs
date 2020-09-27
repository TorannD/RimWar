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
    public class Diplomat : WarObject
    {
        private int lastEventTick = 0;
        private int ticksPerMove = 2500;
        private int searchTick = 60;               

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 2500, false);                       
        }

        public override void Tick()
        {
            base.Tick();
            if(Find.TickManager.TicksGame % this.searchTick == 0)
            {
                //scan for nearby engagements
                this.searchTick = Rand.Range(2000, 3000);
                ScanForNearbyEnemy(1); //WorldUtility.GetRimWarDataForFaction(this.Faction).GetEngagementRange()
                if (this.DestinationTarget != null && this.DestinationTarget.Tile != pather.Destination)
                {
                    PathToTarget(this.DestinationTarget);
                }
                if (DestinationTarget is Caravan)
                {
                    EngageNearby();
                }
                
            }
            if(true) // no delay, was 60ticks
            {
                if (this.ParentSettlement == null)
                {
                    FindParentSettlement();                    
                }
                //target is gone; return home
                if (this.DestinationTarget == null)
                {
                    this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                    if (DestinationTarget != null && DestinationTarget.Tile != pather.Destination)
                    {
                        pather.StartPath(DestinationTarget.Tile, true, false);
                    }
                    else
                    {
                        //not heading in the right direction; pause then attempt to reroute
                        pather.StopDead();
                    }
                }                
            }
        }

        public void ScanForNearbyEnemy(int range)
        {
            List<WorldObject> worldObjects = WorldUtility.GetWorldObjectsInRange(this.Tile, range);
            if (worldObjects != null && worldObjects.Count > 0)
            {
                for (int i = 0; i < worldObjects.Count; i++)
                {
                    WorldObject wo = worldObjects[i];
                    if (wo.Faction != this.Faction && wo != this.DestinationTarget)
                    {
                        //Log.Message("" + this.Name + " scanned nearby object " + this.targetWorldObject.Label);
                        if (wo is Caravan && wo.Faction.HostileTo(this.Faction)) //or rimwar caravan, or diplomat, or merchant; ignore scouts and settlements
                        {
                            //Log.Message(this.Label + " engaging nearby warband " + wo.Label);
                            this.DestinationTarget = worldObjects[i];                            
                            break;
                        }
                    }
                }
            }
        }

        public void EngageNearby()
        {
            if(this.DestinationTarget != null && (this.DestinationTarget.Tile == this.Tile || Find.WorldGrid.TraversalDistanceBetween(this.Tile, this.DestinationTarget.Tile) <= 1))
            {
                ImmediateAction(this.DestinationTarget);
            }
            else
            {
                this.DestinationTarget = null;
            }
                   
        }

        public Diplomat()
        {

        }

        public override int RimWarPoints { get => base.RimWarPoints; set => base.RimWarPoints = value; }
        public override bool MovesAtNight { get => base.MovesAtNight; set => base.MovesAtNight = value; }

        public override bool NightResting
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                if (pather.Moving && pather.nextTile == pather.Destination && Caravan_PathFollower.IsValidFinalPushDestination(pather.Destination) && Mathf.CeilToInt(pather.nextTileCostLeft / 1f) <= 10000)
                {
                    return false;
                }
                if (MovesAtNight)
                {
                    return !CaravanNightRestUtility.RestingNowAt(base.Tile);
                }
                return CaravanNightRestUtility.RestingNowAt(base.Tile);
            }
        }

        public override int TicksPerMove
        {
            get
            {
                return this.ticksPerMove;
            }
            set
            {
                this.ticksPerMove = value;
            }
        }       

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append(base.GetInspectString());            

            if (Find.World.worldObjects.AnySettlementAt(pather.Destination))
            {
                WorldObject wo = Find.World.worldObjects.ObjectsAt(pather.Destination).FirstOrDefault();
                if (wo != null)
                {
                    if (wo.Faction != this.Faction)
                    { 
                        stringBuilder.Append("RW_WarObjectInspectString".Translate(this.Name, "RW_Diplomacy".Translate(), wo.Label));
                    }
                    else
                    {
                        stringBuilder.Append("RW_WarObjectInspectString".Translate(this.Name, "RW_ReturningTo".Translate(), wo.Label));
                    }
                }
            }

            if (pather.Moving)
            {
                float num6 = (float)Utility.ArrivalTimeEstimator.EstimatedTicksToArrive(base.Tile, pather.Destination, this) / 60000f;
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append("RW_EstimatedTimeToDestination".Translate(num6.ToString("0.#")));
                stringBuilder.Append("\n" + Find.WorldGrid.TraversalDistanceBetween(this.Tile, pather.Destination) + " tiles");
            }
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append("RW_CombatPower".Translate(this.RimWarPoints));
            if (!pather.MovingNow)
            {

            }
            return stringBuilder.ToString();
        }

        public override void ImmediateAction(WorldObject wo)
        {
            if(wo != null)
            {
                if(wo.Faction != null)
                {
                    if(wo is Caravan)
                    {
                        IncidentUtility.DoPeaceTalks_Caravan(this, wo as Caravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                        base.ImmediateAction(null);
                    }
                }                
            }
            else
            {
                base.ImmediateAction(wo);
            }
            
        }

        public override void ArrivalAction()
        {
            //Log.Message("beginning arrival actions");
            WorldObject wo = this.DestinationTarget;
            if (wo != null)
            {
                if (wo.Faction != this.Faction)
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
                                //IncidentUtility.DoPeaceTalks_Settlement(this, playerSettlement, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                            }
                            else if (playerCaravan != null)
                            {
                                //Raid player caravan
                                IncidentUtility.DoPeaceTalks_Caravan(this, playerCaravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                            }
                        }
                        else
                        {
                            RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(this.Tile);
                            if (rwsc != null)
                            {
                                rwsc.RimWarPoints += Mathf.RoundToInt(Rand.Range(.5f, .8f) * this.RimWarPoints);
                                this.Faction.TryAffectGoodwillWith(rwsc.parent.Faction, 4, true, true, null, null);
                                rwsc.parent.Faction.TryAffectGoodwillWith(this.Faction, 4, true, true, null, null);
                            }
                            else if (wo is WarObject)
                            {
                                IncidentUtility.ResolveWorldEngagement(this, wo);
                            }
                        }
                    }
                    base.ArrivalAction();
                }
                else
                {
                    //Log.Message("this tile: " + this.Tile + " parent settlement tile: " + this.ParentSettlement.Tile);
                    if (this.Tile == ParentSettlement.Tile)
                    {
                        if (Find.World.worldObjects.AnyMapParentAt(this.Tile))
                        {
                            //reinforce
                            //Log.Message("attempting to reinforce");
                            //Log.Message("map is spawn " + Find.World.worldObjects.MapParentAt(this.Tile).Spawned);
                            //Log.Message("map " + Find.World.worldObjects.MapParentAt(this.Tile).Map + " has faction " + Find.World.worldObjects.MapParentAt(this.Tile).Faction);
                            WorldUtility.GetRimWarSettlementAtTile(this.Tile).RimWarPoints += this.RimWarPoints;
                        }
                        else
                        {
                            //Log.Message("parent settlement points: " + this.ParentSettlement.RimWarPoints);
                            if (wo.Faction != this.Faction) //could happen if parent town is taken over while army is away, in which case - perform another raid
                            {

                            }
                            WorldUtility.GetRimWarSettlementAtTile(this.Tile).RimWarPoints += this.RimWarPoints;
                        }
                        base.ArrivalAction();
                    }                    
                }
            }
            //Log.Message("ending arrival actions");            
        }       
    }
}
