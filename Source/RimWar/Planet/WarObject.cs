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
    [StaticConstructorOnStartup]
    public class WarObject : WorldObject
    {
        private int uniqueId = -1;
        private string nameInt;
        private int warPointsInt = -1;

        public WarObject_PathFollower pather;
        public WarObject_GotoMoteRenderer gotoMote;
        public WarObject_Tweener tweener;

        private Material cachedMat;

        private bool movesAtNight = false;
        private bool cachedImmobilized;
        private int cachedImmobilizedForTicks = -99999;
        private const int ImmobilizedCacheDuration = 60;

        private Settlement parentSettlement = null;
        private int parentSettlementTile = -1;
        private WorldObject targetWorldObject = null;
        private int destinationTile = -1;


        private static readonly Color WarObjectDefaultColor = new Color(1f, 1f, 1f);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
            Scribe_Values.Look(ref nameInt, "name");
            Scribe_Values.Look<bool>(ref this.movesAtNight, "movesAtNight", false, false);
            Scribe_Values.Look<int>(ref this.warPointsInt, "warPointsInt", -1, false);
            Scribe_Values.Look<int>(ref this.parentSettlementTile, "parentSettlementTile", -1, false);
            Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", -1, false);
            Scribe_Deep.Look(ref pather, "pather", this);
            Scribe_References.Look<Settlement>(ref this.parentSettlement, "parentSettlement");
            Scribe_References.Look<WorldObject>(ref this.targetWorldObject, "targetWorldObject");
        }

        public Settlement ParentSettlement
        {
            get
            {
                if(this.parentSettlement != null && parentSettlement.Faction == this.Faction)
                {
                    this.parentSettlementTile = this.parentSettlement.Tile;
                    return this.parentSettlement;
                }
                else
                {
                    if (this.parentSettlementTile != -1)
                    {
                        WorldObject wo = Find.World.worldObjects.WorldObjectAt(this.parentSettlementTile, WorldObjectDefOf.Settlement);
                        if(wo != null && wo.Faction == this.Faction)
                        {
                            this.parentSettlement = WorldUtility.GetRimWarSettlementAtTile(this.parentSettlementTile);
                            if(this.parentSettlement == null)
                            {
                                this.parentSettlementTile = -1;
                            }
                        }
                    }
                    FindParentSettlement();
                    return this.parentSettlement;
                }
            }
            set
            {
                this.parentSettlement = value;
            }
        }

        public WorldObject DestinationTarget
        {
            get
            {
                return this.targetWorldObject;
            }
            set
            {
                this.targetWorldObject = value;
            }
        }

        public int DestinationTile
        {
            get
            {
                return this.destinationTile;
            }
            set
            {
                this.destinationTile = value;
            }
        }

        public virtual int RimWarPoints
        {
            get
            {               
                return this.warPointsInt;
            }
            set
            {
                this.warPointsInt = value;
            }
        } 

        public override Material Material
        {
            get
            {
                if (cachedMat == null)
                {
                    cachedMat = MaterialPool.MatFrom(color: (base.Faction == null) ? Color.white : ((!base.Faction.IsPlayer) ? base.Faction.Color : WarObjectDefaultColor), texPath: def.texture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: WorldMaterials.DynamicObjectRenderQueue);
                }
                return cachedMat;
            }
        }

        public virtual bool MovesAtNight
        {
            get
            {
                return movesAtNight;
            }
            set
            {
                movesAtNight = value;
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            gotoMote.RenderMote();
        }        

        public string Name
        {
            get
            {
                return nameInt;
            }
            set
            {
                nameInt = value;
            }
        }

        public override bool HasName => !nameInt.NullOrEmpty();

        public override string Label
        {
            get
            {
                if(!HasName)
                {
                    return "";
                }
                if (nameInt != null)
                {
                    return nameInt;
                }
                return base.Label;
            }
        }

        public override Vector3 DrawPos => tweener.TweenedPos;

        public Faction FactionOwner => base.Faction;

        public bool IsPlayerControlled => base.Faction == Faction.OfPlayer;

        public override bool AppendFactionToInspectString => true;

        public bool CantMove => NightResting;

        public RimWarData rimwarData => WorldUtility.GetRimWarDataForFaction(this.Faction);

        public virtual bool NightResting
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
                return CaravanNightRestUtility.RestingNowAt(base.Tile);
            }
        }

        public virtual int TicksPerMove //CaravanTicksPerMoveUtility.GetTicksPerMove(this);
        {
            get
            {
                return 10;
            }
            set
            {
                
            }
        }

        public string TicksPerMoveExplanation
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append( "explination = warobject type and faction type"); //CaravanTicksPerMoveUtility.GetTicksPerMove(this, stringBuilder);
                return stringBuilder.ToString();
            }
        }

        public virtual float Visibility => 0; //CaravanVisibilityCalculator.Visibility(this);

        public string VisibilityExplanation
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("explination = warobject type and faction type"); //CaravanVisibilityCalculator.Visibility(this, stringBuilder);
                return stringBuilder.ToString();
            }
        }

        public WarObject()
        {
            pather = new WarObject_PathFollower(this);
            gotoMote = new WarObject_GotoMoteRenderer();
            tweener = new WarObject_Tweener(this);
        }

        public void SetUniqueId(int newId)
        {
            if (uniqueId != -1 || newId < 0)
            {
                Log.Error("Tried to set warobject with uniqueId " + uniqueId + " to have uniqueId " + newId);
            }
            uniqueId = newId;
        }

        public override void Tick()
        {
            base.Tick();
            pather.PatherTick();
            tweener.TweenerTick();
            if(this.DestinationReached)
            {
                ArrivalAction();
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            tweener.ResetTweenedPosToRoot();
        }
        
        public void Notify_Teleported()
        {
            tweener.ResetTweenedPosToRoot();
            pather.Notify_Teleported_Int();
        }

        public virtual void ImmediateAction(WorldObject wo)
        {
            if (Find.WorldObjects.Contains(this))
            {
                Find.WorldObjects.Remove(this);
            }
        }

        public virtual void ArrivalAction()
        {
            if (Find.WorldObjects.Contains(this))
            {
                Find.WorldObjects.Remove(this);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            
            if (pather.Moving)
            {

                stringBuilder.Append("RW_WarObjectInspectString".Translate(this.def.defName, this.Faction.Name, this.pather.Destination));
                
            }

            if (pather.Moving)
            {
                float num6 = (float)Utility.ArrivalTimeEstimator.EstimatedTicksToArrive(base.Tile, pather.Destination, this);// / 60000f;
                stringBuilder.AppendLine();
                stringBuilder.Append("RW_EstimatedTimeToDestination".Translate(num6.ToString("0.#")));
            }            
            if (!pather.MovingNow)
            {

            }
            return stringBuilder.ToString();
        }

        public virtual bool DestinationReached
        {
            get
            {
                return this.Tile == pather.Destination;
            }
        }

        public void PathToTarget(WorldObject wo)
        {
            pather.StartPath(wo.Tile, true);
            tweener.ResetTweenedPosToRoot();
        }

        public void PathToTargetTile(int tile)
        {
            pather.StartPath(tile, true);
            tweener.ResetTweenedPosToRoot();
        }

        public void FindParentSettlement()
        {
            List<Settlement> rwdTownList = WorldUtility.GetFriendlyRimWarSettlementsInRange(this.Tile, 20, this.Faction, WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.Faction));
            if(rwdTownList != null && rwdTownList.Count <= 0)
            {
                rwdTownList = WorldUtility.GetFriendlyRimWarSettlementsInRange(this.Tile, 200, this.Faction, WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.Faction));
            }

            if(rwdTownList != null && rwdTownList.Count > 0)
            { 
                this.ParentSettlement = WorldUtility.GetFriendlyRimWarSettlementsInRange(this.Tile, 200, this.Faction, WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.Faction)).RandomElement();
                PathToTarget(Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement));                
            }
            else
            {
                //warband is lost, no nearby parent settlement
                if (Find.WorldObjects.Contains(this))
                {
                    Find.WorldObjects.Remove(this);
                }
            }
        }

        public void FindHostileSettlement()
        {
            this.DestinationTarget = Find.World.worldObjects.WorldObjectOfDefAt(WorldObjectDefOf.Settlement, WorldUtility.GetHostileRimWarSettlementsInRange(this.Tile, 20, this.Faction, WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.Faction)).RandomElement().Tile);
            if (this.DestinationTarget != null)
            {
                PathToTarget(this.DestinationTarget);
            }
            else
            {
                if (this.ParentSettlement == null)
                {
                    FindParentSettlement();
                }
                else
                {
                    PathToTarget(Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement));
                }
            }
        }
    }
}
