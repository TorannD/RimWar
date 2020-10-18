using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWar.History;

namespace RimWar.Planet
{
    public class Scout : WarObject
    {
        private int lastEventTick = 0;
        private int ticksPerMove = 2000;
        private int searchTick = 60;
        private int scanRange = 2;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 2000, false);
        }

        public override void Notify_Player()
        {
            base.Notify_Player();
            if (!playerNotified && this.DestinationTarget != null)
            {
                Options.SettingsRef settingsRef = new Options.SettingsRef();
                if (this.DestinationTarget.Faction == Faction.OfPlayer && this.Faction.HostileTo(Faction.OfPlayer) && Find.WorldGrid.TraversalDistanceBetween(this.Tile, this.DestinationTarget.Tile) <= settingsRef.letterNotificationRange && Rand.Chance(.35f))
                {
                    playerNotified = true;
                    StringBuilder stringBuilder = new StringBuilder();
                    float num6 = (float)Utility.ArrivalTimeEstimator.EstimatedTicksToArrive(base.Tile, pather.Destination, this) / 60000f;
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.Append("RW_EstimatedTimeToDestination".Translate(num6.ToString("0.#")));
                    Find.LetterStack.ReceiveLetter("RW_LetterApproachingThreatEvent".Translate(), "RW_LetterApproachingThreatEventText".Translate(this.Name, this.RimWarPoints, this.DestinationTarget.Label, stringBuilder), RimWarDefOf.RimWar_WarningEvent);
                }
            }
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

        public override void Tick()
        {
            base.Tick();            
        }

        public override void ScanAction(float range)
        {
            //Scouts will prioritize other units
            List<WorldObject> worldObjects = WorldUtility.GetWorldObjectsInRange(this.Tile, range);
            if (worldObjects != null && worldObjects.Count > 0)
            {
                for (int i = 0; i < worldObjects.Count; i++)
                {
                    WorldObject wo = worldObjects[i];
                    if (wo.Faction != this.Faction && wo != this.DestinationTarget && (this.DestinationTarget == null || (this.DestinationTarget != null && Find.WorldGrid.ApproxDistanceInTiles(wo.Tile, this.Tile) < Find.WorldGrid.ApproxDistanceInTiles(this.DestinationTarget.Tile, this.Tile))))
                    {
                        //Log.Message("" + this.Name + " scanned nearby object " + this.targetWorldObject.Label);
                        if (wo is Caravan && interactable) //or rimwar caravan, or diplomat, or merchant; ignore scouts and settlements
                        {
                            //Log.Message(this.Name + "scanned a player caravan");
                            Caravan playerCaravan = wo as Caravan;
                            //Log.Message("evaluating player caravan with " + playerCaravan.PlayerWealthForStoryteller + " wealth");
                            if (CaravanDetected(playerCaravan) && (playerCaravan.PlayerWealthForStoryteller / 100) <= (int)(this.RimWarPoints) && this.Faction.HostileTo(playerCaravan.Faction))
                            {
                                this.DestinationTarget = playerCaravan;
                                break;                                                            
                                //Log.Message(this.Label + " engaging nearby warband " + wo.Label);
                            }
                        }
                        if (wo is WarObject && this.Faction.HostileTo(wo.Faction))
                        {
                            WarObject warObject = wo as WarObject;
                            if(warObject.RimWarPoints <= this.RimWarPoints)
                            {
                                this.DestinationTarget = wo;
                                break;
                            }
                        }
                    }
                }
            }
            base.ScanAction(1f);
        }

        public override void EngageNearbyCaravan(Caravan car)
        {
            if (car.Faction != null && car.Faction == Faction.OfPlayer && this.Faction.HostileTo(car.Faction))
            {
                if (ShouldInteractWith(car, this) || (car.PlayerWealthForStoryteller / 105) <= (int)(this.RimWarPoints))
                {
                    this.interactable = false;
                    IncidentUtility.DoCaravanAttackWithPoints(this, car, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                }
            }
            else
            {
                if (ShouldInteractWith(car, this))
                {
                    this.interactable = false;
                    IncidentUtility.DoCaravanAttackWithPoints(this, car, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                }
            }
        }

        public override void EngageNearbyWarObject(WarObject rwo)
        {
            IncidentUtility.ResolveRimWarBattle(this, rwo);
            ImmediateAction(rwo);            
        }

        public Scout()
        {

        }

        public override int RimWarPoints { get => base.RimWarPoints; set => base.RimWarPoints = value; }
        public override bool MovesAtNight { get => base.MovesAtNight; set => base.MovesAtNight = value; }
        public override float MovementModifier => (2500f / (float)TicksPerMove);
        public override float DetectionModifier => base.DetectionModifier;

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

        public override void ImmediateAction(WorldObject wo)
        {
            base.ImmediateAction(wo);           
        }

        public override void ArrivalAction()
        {
            //Log.Message("beginning arrival actions - scout; destination: " + this.DestinationTarget.Label + " parent: " + this.ParentSettlement.Label);
            WorldObject wo = this.DestinationTarget;
            if(wo != null && wo.Faction != this.Faction)
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
                            IncidentUtility.DoRaidWithPoints(this.RimWarPoints, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                            base.ArrivalAction();
                        }
                        else if (playerCaravan != null)
                        {
                            //Raid player caravan
                            IncidentUtility.DoCaravanAttackWithPoints(this, playerCaravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                            this.DestinationTarget = this.ParentSettlement;
                            this.interactable = false;
                        }
                    }
                    else
                    {
                        RimWorld.Planet.Settlement wos = wo as RimWorld.Planet.Settlement;                        
                        if (wos != null)
                        {
                            RimWarSettlementComp rwscDefender = wos.GetComponent<RimWarSettlementComp>();
                            if (wos.Faction == Faction.OfPlayer)
                            {
                                IncidentUtility.DoRaidWithPoints(this.RimWarPoints, wos, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                                base.ArrivalAction();
                            }
                            else if (rwscDefender != null)
                            {
                                IncidentUtility.ResolveWarObjectAttackOnSettlement(this, this.ParentSettlement, rwscDefender, WorldUtility.GetRimWarDataForFaction(this.Faction));
                                base.ArrivalAction();
                            }
                        }
                        if (wo is WarObject)
                        {
                            IncidentUtility.ResolveWorldEngagement(this, wo);
                            base.ArrivalAction();
                        }
                    }                
                }
                else
                {
                    if(wo.Faction == Faction.OfPlayerSilentFail) // reinforce
                    {
                        RimWorld.Planet.Settlement playerSettlement = Find.WorldObjects.SettlementAt(this.Tile);
                        if (playerSettlement != null)
                        {
                            //Raid Player Map
                            IncidentUtility.DoReinforcementWithPoints(this.RimWarPoints, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                            base.ArrivalAction();
                        }
                    }
                    else
                    {
                        RimWarSettlementComp rwsc = WorldUtility.GetRimWarSettlementAtTile(this.Tile);
                        if(rwsc != null)
                        {
                            rwsc.RimWarPoints += this.RimWarPoints;
                            base.ArrivalAction();
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
                        this.WarSettlementComp.RimWarPoints += this.RimWarPoints;
                    }
                    else
                    {
                        //Log.Message("parent settlement points: " + this.ParentSettlement.RimWarPoints);
                        if (wo.Faction != this.Faction) //could happen if parent town is taken over while army is away, in which case - perform another raid
                        {

                        }
                        this.WarSettlementComp.RimWarPoints += this.RimWarPoints;
                    }
                    base.ArrivalAction();
                }
            }
            //Log.Message("ending arrival actions");
        }       
    }
}
