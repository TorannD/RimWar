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
        private int ticksPerMove = 2800;
        private int searchTick = 60;

        public override bool UseDestinationTile => true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);            
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 2800, false);                       
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
                if (interactable)
                {
                    ScanForNearbyEnemy(1); //WorldUtility.GetRimWarDataForFaction(this.Faction).GetEngagementRange()
                }
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
            if (Find.TickManager.TicksGame % (this.searchTick - 10) == 0)
            {
                this.ValidateParentSettlement();
            }
            if (true) //Find.TickManager.TicksGame % 60 == 0)
            {
                if (this.ParentSettlement == null)
                {
                    FindParentSettlement();
                }
                //target is gone; return home
                if (this.DestinationTile <= 0)
                {
                    this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                    if (this.DestinationTarget == null)
                    {
                        this.ValidateParentSettlement();
                        WorldUtility.Get_WCPT().UpdateFactionSettlements(WorldUtility.GetRimWarDataForFaction(this.Faction));
                        FindParentSettlement();
                        this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
                    }
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
            ImmediateAction(this.DestinationTarget);
            //if (this.DestinationTarget != null && (this.DestinationTarget.Tile == this.Tile))
            //{
            //    ImmediateAction(this.DestinationTarget);
            //}
            //else if (this.DestinationTarget != null && Find.WorldGrid.TraversalDistanceBetween(this.Tile, this.DestinationTarget.Tile) >= 1)
            //{
            //    PathToTargetTile(this.DestinationTile);
            //    this.DestinationTarget = null;
            //}
            //else
            //{
            //    this.DestinationTarget = null;
            //}
        }

        public override void ImmediateAction(WorldObject wo)
        {
            if(wo != null)
            {
                if(wo.Faction != null && wo.Faction.HostileTo(this.Faction))
                {
                    if(wo is Caravan && interactable)
                    {
                        IncidentUtility.DoCaravanAttackWithPoints(this, wo as Caravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn), PawnGroupKindDefOf.Peaceful);
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
