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
        private int ticksPerMove = 2500;
        private int searchTick = 60;
        public bool tradedWithSettlement = false;
        public bool tradedWithTrader = false;
        public bool tradedWithPlayer = false;
        public TraderKindDef traderKind = null;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 2500, false);
            Scribe_Values.Look<bool>(ref this.tradedWithSettlement, "tradedWithSettlement", false, false);
            Scribe_Values.Look<bool>(ref this.tradedWithTrader, "tradedWithTrader", false, false);
            Scribe_Values.Look<bool>(ref this.tradedWithPlayer, "tradedWithPlayer", false, false);
            Scribe_Defs.Look<TraderKindDef>(ref this.traderKind, "traderKind");
        }        

        public Trader()
        {

        }

        public override int RimWarPoints { get => base.RimWarPoints; set => base.RimWarPoints = value; }
        public override bool MovesAtNight { get => base.MovesAtNight; set => base.MovesAtNight = value; }
        public override float MovementModifier => (2500f / (float)TicksPerMove);

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

        //NextSearchTick
        //NextSearchTickIncrement (override by type)
        //ScanRange (override by type, base is 1f)
        //EngageNearbyWarObject --> IncidentUtility -- > ImmediateAction
        //EngageNearbyCaravan --> IncidentUtility --> ImmediateAction
        //NotifyPlayer
        //NextMoveTick
        //NextMoveTickIncrement (default is settings based)
        //ArrivalAction

        public override float ScanRange => 1.6f;

        public override void Tick()
        {
            base.Tick();            
        }

        public override void EngageNearbyCaravan(Caravan car)
        {
            if (ShouldInteractWith(car, this))
            {
                if (this.Faction.HostileTo(car.Faction))
                {
                    WorldUtility.Get_WCPT().RemoveCaravanTarget(car);
                    car.pather.StopDead();                    
                    IncidentUtility.DoCaravanAttackWithPoints(this, car, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn), PawnGroupKindDefOf.Trader);
                }
                else
                {
                    WorldUtility.Get_WCPT().RemoveCaravanTarget(car);
                    car.pather.StopDead();
                    IncidentUtility.DoCaravanTradeWithPoints(this, car, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                }
            }
        }

        public override void EngageNearbyWarObject(WarObject rwo)
        {
            if (rwo is Trader && !this.tradedWithTrader)
            {
                if (rwo.Faction != null && !rwo.Faction.HostileTo(this.Faction))
                {
                    //trade with another AI faction
                    this.tradedWithTrader = true;
                    IncidentUtility.ResolveRimWarTrade(this, rwo as Trader);
                }
            }
        }        

        public override void ImmediateAction(WorldObject wo)
        {            
            base.ImmediateAction(wo);            
        }

        public override void ArrivalAction()
        {
            //Log.Message("trader arrival action with destiantion " + this.DestinationTarget);
            WorldObject wo = this.DestinationTarget;
            if (wo != null)
            {
                //Log.Message("Trader AA 1");
                if (wo.Faction != this.Faction)
                {
                    //Log.Message("Trader AA 2");
                    if (wo.Faction.HostileTo(this.Faction))
                    {
                        //Log.Message("Trader AA 3");
                        if (wo.Faction == Faction.OfPlayer)
                        {
                            //Log.Message("Trader AA 4");
                            RimWorld.Planet.Settlement playerSettlement = wo as RimWorld.Planet.Settlement;
                            Caravan playerCaravan = wo as Caravan;
                            if (playerSettlement != null)
                            {
                                IncidentUtility.DoRaidWithPoints(Mathf.RoundToInt(this.RimWarPoints * .8f), playerSettlement, this.rimwarData, PawnsArrivalModeDefOf.EdgeWalkIn, PawnGroupKindDefOf.Trader);
                            }
                            else if(playerCaravan != null)
                            {
                                IncidentUtility.DoCaravanAttackWithPoints(this, playerCaravan, this.rimwarData, PawnsArrivalModeDefOf.EdgeWalkIn, PawnGroupKindDefOf.Trader);
                            }
                        }
                        else
                        {
                            //Log.Message("Trader AA 5");
                            if (wo is RimWorld.Planet.Settlement)
                            {
                                RimWorld.Planet.Settlement settlement = wo as RimWorld.Planet.Settlement;
                                if (settlement != null)
                                {
                                    RimWarSettlementComp rwsc = settlement.GetComponent<RimWarSettlementComp>();
                                    if (rwsc != null)
                                    {
                                        IncidentUtility.ResolveWarObjectAttackOnSettlement(this, this.ParentSettlement, rwsc, this.rimwarData);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                       // Log.Message("Trader AA 6");
                        if (wo.Faction == Faction.OfPlayer)
                        {
                            //Log.Message("Trader AA 7");
                            RimWorld.Planet.Settlement playerSettlement = wo as RimWorld.Planet.Settlement;
                            if (playerSettlement != null)
                            {
                                //Log.Message("Trader AA 7.1");
                                IncidentUtility.DoSettlementTradeWithPoints(this, playerSettlement, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn), traderKind);
                                if (this.WarSettlementComp != null)
                                {
                                    this.WarSettlementComp.RimWarPoints += Mathf.RoundToInt(this.RimWarPoints * (Rand.Range(1.05f, 1.25f)));
                                }
                            }
                        }
                        else
                        {
                            //Log.Message("Trader AA 8");
                            RimWorld.Planet.Settlement settlement = wo as RimWorld.Planet.Settlement;
                            if (settlement != null && !settlement.Destroyed)
                            {
                                RimWarSettlementComp rwsc = settlement.GetComponent<RimWarSettlementComp>();
                                if (rwsc != null && rwsc.parent.Faction != this.Faction && !tradedWithSettlement)
                                {
                                    //Log.Message("Trader AA 8.1");
                                    IncidentUtility.ResolveSettlementTrade(this, rwsc);
                                }
                                else
                                {
                                    //Log.Message("Trader AA 8.2");
                                    this.DestinationTarget = this.ParentSettlement;
                                    if (this.ParentSettlement == null || this.ParentSettlement.Destroyed || this.DestinationTarget.Tile == this.ParentSettlement.Tile)
                                    {
                                        this.ParentSettlement = null;
                                        ReAssignParentSettlement();
                                    }
                                    PathToTarget(this.DestinationTarget);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Log.Message("Trader AA 9");
                    if (wo is RimWorld.Planet.Settlement)
                    {
                        RimWorld.Planet.Settlement settlement = wo as RimWorld.Planet.Settlement;
                        if (settlement != null && !settlement.Destroyed)
                        {
                            RimWarSettlementComp rwsc = settlement.GetComponent<RimWarSettlementComp>();
                            if (rwsc != null)
                            {
                                if (wo.Tile != this.ParentSettlement.Tile)
                                {
                                    int bonusPts = Rand.Range(25, 75);
                                    RimWarData rwd = WorldUtility.GetRimWarDataForFaction(wo.Faction);
                                    if (rwd != null && rwd.behavior == RimWarBehavior.Merchant)
                                    {
                                        bonusPts += 25;
                                    }
                                    rwsc.RimWarPoints += this.RimWarPoints + bonusPts;
                                }
                                else
                                {
                                    rwsc.RimWarPoints += this.RimWarPoints;
                                }
                            }                            
                        }
                    }
                }
            }
            //Log.Message("Trader AA end");
            //Log.Message("ending arrival actions");
            base.ArrivalAction();
        }       
    }
}
