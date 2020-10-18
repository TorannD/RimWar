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
        private int ticksPerMove = 2500;
        private int searchTick = 60;

        public override bool UseDestinationTile => true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);            
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 2500, false);                       
        }        

        public Settler()
        {

        }

        public override int RimWarPoints { get => base.RimWarPoints; set => base.RimWarPoints = value; }
        public override bool MovesAtNight { get => base.MovesAtNight; set => base.MovesAtNight = value; }
        public override float MovementModifier => (2500f/(float)TicksPerMove);

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
                    if (wo != null)
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
                stringBuilder.Append(" (" + Find.WorldGrid.TraversalDistanceBetween(this.Tile, pather.Destination) + "RW_TilesAway_Verbatum".Translate() + ")");
                if(this.NightResting)
                {
                    stringBuilder.Append("\n"+"RW_UnitCamped".Translate());
                }
            }
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append("RW_CombatPower".Translate(this.RimWarPoints));
            stringBuilder.Append("\n" + this.Faction.PlayerRelationKind.ToString());
            if (!pather.MovingNow)
            {

            }
            return stringBuilder.ToString();
        }

        //NextSearchTick
        //NextSearchTickIncrement (override by type)
        //ScanRange (override by type)
        //EngageNearbyWarObject --> IncidentUtility -- > ImmediateAction
        //EngageNearbyCaravan --> IncidentUtility --> ImmediateAction
        //NotifyPlayer
        //NextMoveTick
        //NextMoveTickIncrement (default is settings based)
        //ArrivalAction
        //public override int NextSearchTickIncrement => Rand.Range(1000, 3000);

        public override void Tick()
        {
            base.Tick();
            if (DestinationTarget != null)
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

        public void EngageNearbyEnemy()
        {
            ImmediateAction(this.DestinationTarget);
        }

        public override void ImmediateAction(WorldObject wo)
        {
            base.ImmediateAction(wo);
        }

        public override void ArrivalAction()
        {
            //Log.Message("beginning arrival actions");
            List<WorldObject> woList = WorldUtility.GetAllWorldObjectsAtExcept(this.Tile, this);
            if (woList != null && woList.Count > 0)
            {
                WorldObject wo = woList.RandomElement();
                for (int i = 0; i < woList.Count; i++)
                {
                    if(woList[i] is RimWorld.Planet.Settlement)
                    {
                        wo = woList[i];
                        break;
                    }
                }
                
                if (wo != null && wo != this)
                {
                    if (wo.Faction != this.Faction)
                    {
                        if (wo.Faction.HostileTo(this.Faction))
                        {
                            if (wo.Faction == Faction.OfPlayer)
                            {
                                //Do Raid
                                RimWorld.Planet.Settlement playerSettlement = wo as RimWorld.Planet.Settlement;
                                Caravan playerCaravan = wo as Caravan;
                                if (playerSettlement != null)
                                {
                                    //Raid Player Map
                                    IncidentUtility.DoRaidWithPoints(this.RimWarPoints, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                                }
                                else if (playerCaravan != null)
                                {
                                    //Raid player caravan
                                    IncidentUtility.DoCaravanAttackWithPoints(this, playerCaravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                                }
                            }
                            else if(wo is WarObject)
                            {
                                IncidentUtility.ResolveRimWarBattle(this, wo as WarObject);                                    
                            }
                            else
                            {
                                ValidateParentSettlement();
                                this.DestinationTile = this.ParentSettlement.Tile;
                                this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                                PathToTargetTile(DestinationTile);
                            }
                        }
                        else
                        {
                            ValidateParentSettlement();
                            this.DestinationTile = this.ParentSettlement.Tile;
                            this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                            PathToTargetTile(DestinationTile);
                        }
                    }
                    else
                    {
                        //Log.Message("this tile: " + this.Tile + " parent settlement tile: " + this.ParentSettlement.Tile);
                        if(wo is RimWorld.Planet.Settlement)
                        {
                            RimWarSettlementComp rwsc = wo.GetComponent<RimWarSettlementComp>();
                            if(rwsc != null)
                            {
                                rwsc.RimWarPoints += this.RimWarPoints;
                            }
                        }
                    }
                }                
            }
            else
            {
                WorldUtility.CreateSettlement(this, woList, this.rimwarData, DestinationTile, this.Faction);
            }
            //Log.Message("ending arrival actions");
            base.ArrivalAction();
        }    
    }
}
