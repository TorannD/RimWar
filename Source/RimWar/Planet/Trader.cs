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
    public class Trader : WarObject
    {
        private int lastEventTick = 0;
        private int ticksPerMove = 4500;
        private int searchTick = 60;
        private List<WorldObject> tradedWith;

        public List <WorldObject> TradedWith
        {
            get
            {
                if(tradedWith == null)
                {
                    tradedWith = new List<WorldObject>();
                    tradedWith.Clear();
                }
                return tradedWith;
            }
            set
            {
                if(tradedWith == null)
                {
                    tradedWith = new List<WorldObject>();
                    tradedWith.Clear();
                }
                tradedWith = value;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 4500, false);
            //Scribe_Collections.Look<WorldObject>(ref this.tradedWith, "tradedWith", LookMode.Value);
        }        

        public Trader()
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
                        stringBuilder.Append("RW_WarObjectInspectString".Translate(this.Name, "RW_Trading".Translate(), wo.Label));
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

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % this.searchTick == 0)
            {
                //scan for nearby engagements
                this.searchTick = Rand.Range(2000, 3000);
                ScanNearby(1); //WorldUtility.GetRimWarDataForFaction(this.Faction).GetEngagementRange()
                if (this.DestinationTarget != null && this.DestinationTarget.Tile != pather.Destination)
                {
                    PathToTarget(this.DestinationTarget);
                }
                if(this.RimWarPoints <= 0 || this.RimWarPoints > 100000000)
                {
                    this.ImmediateAction(null);
                }

            }
            if (true) //Find.TickManager.TicksGame % 60 == 0)
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

        public void ScanNearby(int range)
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
                        if (wo is Caravan) //or rimwar caravan, or diplomat, or merchant; ignore scouts and settlements
                        {
                            //Log.Message(this.Label + " engaging nearby warband " + wo.Label);
                            EngageNearby(wo);
                            break;
                        }
                        else if (wo is Trader)
                        {
                            EngageNearby(wo);
                            break;
                        }
                    }
                }
            }
        }

        public void EngageNearby(WorldObject wo)
        {
            if (wo.Faction != null)
            {
                if (wo.Faction.HostileTo(this.Faction))
                {
                    //resolve combat
                    if (wo is Caravan)
                    {
                        IncidentUtility.DoCaravanAttackWithPoints(this, wo as Caravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                        base.ImmediateAction(wo);
                    }
                }
                else
                {
                    if (wo is Caravan && !TradedWith.Contains(wo))
                    {
                        //trade with player
                        IncidentUtility.DoCaravanTradeWithPoints(this, wo as Caravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                        this.TradedWith.Add(wo);
                    }
                    else if (wo is Trader && !TradedWith.Contains(wo))
                    {
                        //trade with another AI faction
                        IncidentUtility.ResolveRimWarTrade(this, wo as Trader);
                    }
                }
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
                        IncidentUtility.DoCaravanAttackWithPoints(this, wo as Caravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                        base.ImmediateAction(wo);
                    }
                }    
                else if(wo.Faction != null && !wo.Faction.HostileTo(this.Faction))
                {
                    if (wo is Caravan && !TradedWith.Contains(wo))
                    {
                        //trade with player
                        IncidentUtility.DoCaravanTradeWithPoints(this, wo as Caravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                        this.TradedWith.Add(wo);
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
                        //this.DestinationTarget = null;
                    }
                    else
                    {
                        if (wo.Faction.IsPlayer)
                        {
                            RimWorld.Planet.Settlement playerSettlement = Find.World.worldObjects.SettlementAt(this.Tile);
                            if (playerSettlement != null)
                            {
                                IncidentUtility.DoSettlementTradeWithPoints(this, playerSettlement, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                                if (this.ParentSettlement != null)
                                {
                                    this.ParentSettlement.RimWarPoints += Mathf.RoundToInt(this.RimWarPoints * (Rand.Range(1.05f, 1.25f)));
                                }
                            }
                        }
                        else
                        {
                            Settlement settlement = WorldUtility.GetRimWarSettlementAtTile(this.Tile);
                            if(settlement != null)
                            {
                                IncidentUtility.ResolveSettlementTrade(this, settlement);
                            }
                            else
                            {
                                this.DestinationTarget = Find.WorldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                                PathToTarget(this.DestinationTarget);
                            }
                        }
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
            //Log.Message("ending arrival actions");
            base.ArrivalAction();
        }       
    }
}
