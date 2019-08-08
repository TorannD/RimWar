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
    public class Settler : WarObject
    {
        private int lastEventTick = 0;
        private int ticksPerMove = 2000;
        private int searchTick = 60;        

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);            
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 2000, false);                       
        }        

        public Settler()
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
                if (wo.Faction != this.Faction)
                {
                    stringBuilder.Append("RW_SettlerInspectString".Translate(this.Name, "RW_EstablishingSettlement".Translate()));
                }
                else
                {
                    stringBuilder.Append("RW_WarObjectInspectString".Translate(this.Name, "RW_ReturningTo".Translate(), wo.Label));
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

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % this.searchTick == 0)
            {
                //scan for nearby engagements
                this.searchTick = Rand.Range(3000, 5000);
                ScanForNearbyEnemy(1); //WorldUtility.GetRimWarDataForFaction(this.Faction).GetEngagementRange()
                if(DestinationTarget != null)
                {
                    if (this.DestinationTarget.Tile != pather.Destination)
                    {
                        PathToTarget(DestinationTarget);
                    }
                }
                else if (this.DestinationTile > 0 && this.DestinationTile != pather.Destination)
                {
                    PathToTargetTile(this.DestinationTile);
                }
                //else
                //{
                //    if (this.ParentSettlement != null)
                //    {
                //        this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                //    }
                //}
            }
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (this.ParentSettlement == null)
                {
                    FindParentSettlement();
                }
                //target is gone; return home
                if (this.DestinationTile <= 0)
                {
                    this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                    if (DestinationTarget != null && DestinationTarget.Tile != pather.Destination)
                    {
                        PathToTarget(DestinationTarget);
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
                    if (wo.Faction != this.Faction && wo.Faction.HostileTo(this.Faction))
                    {
                        //Log.Message("" + this.Name + " scanned nearby object " + this.targetWorldObject.Label);
                        if (wo is Caravan) //or rimwar caravan, or diplomat, or merchant; ignore scouts and settlements
                        {
                            //Log.Message(this.Label + " engaging nearby warband " + wo.Label);
                            this.DestinationTarget = wo;
                            EngageNearbyEnemy();
                            break;
                        }
                    }
                }
            }
        }

        public void EngageNearbyEnemy()
        {
            if (this.DestinationTarget != null && (this.DestinationTarget.Tile == this.Tile))
            {
                ImmediateAction(this.DestinationTarget);
            }
            else if (this.DestinationTarget != null && Find.WorldGrid.TraversalDistanceBetween(this.Tile, this.DestinationTarget.Tile) >= 1)
            {
                PathToTargetTile(this.DestinationTile);
                this.DestinationTarget = null;
            }
            else
            {
                this.DestinationTarget = null;
            }
        }

        public override void ImmediateAction(WorldObject wo)
        {
            if(wo != null)
            {
                if(wo.Faction != null && wo.Faction.HostileTo(this.Faction))
                {
                    if(wo is Caravan)
                    {
                        IncidentUtility.DoCaravanAttackWithPoints(this, wo as Caravan, this.rimwarData, PawnsArrivalModeDefOf.EdgeWalkIn);
                        base.ImmediateAction(wo);
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
            WorldObject wo = Find.World.worldObjects.ObjectsAt(pather.Destination).FirstOrDefault();
            if (wo != null && wo != this)
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
                                IncidentUtility.DoRaidWithPoints(this.RimWarPoints, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), PawnsArrivalModeDefOf.EdgeWalkIn);
                            }
                            else if (playerCaravan != null)
                            {
                                //Raid player caravan
                                IncidentUtility.DoCaravanAttackWithPoints(this, playerCaravan, this.rimwarData, PawnsArrivalModeDefOf.EdgeWalkIn);
                            }
                        }
                        else
                        {
                            this.DestinationTile = this.ParentSettlement.Tile;
                            this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                            PathToTargetTile(DestinationTile);
                        }
                    }
                    else
                    {
                        this.DestinationTile = this.ParentSettlement.Tile;
                        this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                        PathToTargetTile(DestinationTile);
                    }
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
            }
            else
            {
                WorldUtility.CreateSettlement(this, this.rimwarData, DestinationTile, this.Faction);                
            }
            //Log.Message("ending arrival actions");
            base.ArrivalAction();
        }    
    }
}
