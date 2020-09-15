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
        public bool launched = false;
        private bool cachedImmobilized;
        private int cachedImmobilizedForTicks = -99999;
        private const int ImmobilizedCacheDuration = 60;

        private Settlement parentSettlement = null;
        private int parentSettlementTile = -1;
        private WorldObject targetWorldObject = null;
        private int destinationTile = -1;

        public int nextMoveTickIncrement = 0;
        public bool canReachDestination = true;
        public bool playerNotified = false;

        private bool useDestinationTile = false;
        public virtual bool UseDestinationTile
        {
            get
            {
                return useDestinationTile;
            }                
        }

        private int nextMoveTick;
        public virtual int NextMoveTick
        {
            get
            {
                return nextMoveTick;
            }
            set
            {
                nextMoveTick = value;
            }
        }

        private static readonly Color WarObjectDefaultColor = new Color(1f, 1f, 1f);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
            Scribe_Values.Look(ref nameInt, "name");
            Scribe_Values.Look<bool>(ref this.movesAtNight, "movesAtNight", false, false);
            Scribe_Values.Look<bool>(ref this.playerNotified, "playerNotified", false, false);
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
                if (targetWorldObject != null && targetWorldObject.Destroyed)
                {
                    targetWorldObject = null;
                }
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
                this.warPointsInt = Mathf.Clamp(warPointsInt, 50, 100000);
                return this.warPointsInt;
            }
            set
            {
                this.warPointsInt = Mathf.Max(0, value);
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
            if (Find.TickManager.TicksGame >= this.NextMoveTick)
            {                
                pather.PatherTick();
                tweener.TweenerTick();
                if (this.DestinationReached)
                {
                    ValidateParentSettlement();
                    ArrivalAction();
                }
                Options.SettingsRef settingsRef = new Options.SettingsRef();
                this.nextMoveTickIncrement = (int)Rand.Range(settingsRef.woEventFrequency * .9f, settingsRef.woEventFrequency * 1.1f);
                this.NextMoveTick = Find.TickManager.TicksGame + this.nextMoveTickIncrement;
                if (!UseDestinationTile)
                {
                    if (this.DestinationTarget != null)
                    {
                        if (DestinationTarget.Tile != pather.Destination)
                        {
                            this.launched = false;
                            PathToTargetTile(DestinationTarget.Tile);
                        }
                    }
                    else
                    {
                        canReachDestination = false;
                        pather.StopDead();
                    }
                }
                if (!canReachDestination)
                {
                    ValidateParentSettlement();
                    if (this.ParentSettlement == null)
                    {
                        FindParentSettlement();
                    }
                    if(ParentSettlement != null && (!Utility.WorldReachability.CanReach(this.Tile, ParentSettlement.Tile) || Find.WorldGrid.ApproxDistanceInTiles(this.Tile, ParentSettlement.Tile) > 100))
                    {
                        this.Destroy();
                    }
                    this.canReachDestination = true;
                    this.DestinationTarget = ParentSettlement.RimWorld_Settlement;
                    PathToTarget(this.DestinationTarget);
                }
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

        public virtual void Notify_Player()
        {

        }

        public virtual void ImmediateAction(WorldObject wo)
        {
            if (!this.Destroyed)
            {
                this.Destroy();
            }
            if (Find.WorldObjects.Contains(this))
            {
                Find.WorldObjects.Remove(this);
            }
        }

        public virtual void ArrivalAction()
        {
            if (!this.Destroyed)
            {
                this.Destroy();
            }
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

        public void ValidateParentSettlement()
        {
            if (this.ParentSettlement != null)
            {
                RimWorld.Planet.Settlement settlement = Find.World.worldObjects.SettlementAt(this.ParentSettlement.Tile);
                if (settlement == null || settlement.Faction != this.Faction)
                {
                    if (WorldUtility.GetRimWarDataForFaction(this.Faction).FactionSettlements.Contains(this.ParentSettlement))
                    {
                        WorldUtility.GetRimWarDataForFaction(this.Faction).FactionSettlements.Remove(this.ParentSettlement);
                    }
                    this.ParentSettlement = null;
                }
            }
        }

        public void FindParentSettlement()
        {
            ParentSettlement = WorldUtility.GetClosestRimWarSettlementInRWDTo(WorldUtility.GetRimWarDataForFaction(this.Faction), this.Tile);
            //List<Settlement> rwdTownList = WorldUtility.GetFriendlyRimWarSettlementsInRange(this.Tile, 30, this.Faction, WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.Faction));
            //if(rwdTownList != null && rwdTownList.Count <= 0)
            //{
            //    Log.Message("initial check did not find nearby settlement");
            //    rwdTownList = WorldUtility.GetFriendlyRimWarSettlementsInRange(this.Tile, 100, this.Faction, WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.Faction));
            //}

            //if(rwdTownList != null && rwdTownList.Count > 0)
            //{
            //    this.ParentSettlement = rwdTownList.RandomElement(); //WorldUtility.GetFriendlyRimWarSettlementsInRange(this.Tile, 100, this.Faction, WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.Faction)).RandomElement();
            //    Log.Message("found parent at " + this.ParentSettlement.Tile + " current tile is " + this.Tile);
            //    PathToTarget(Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement));                
            //}
            if(this.parentSettlement == null)
            {
                //warband is lost, no nearby parent settlement
                this.Destroy();
                if (Find.WorldObjects.Contains(this))
                {
                    Find.WorldObjects.Remove(this);
                }
            }
        }

        public void ReAssignParentSettlement()
        {
            this.ValidateParentSettlement();
            WorldUtility.Get_WCPT().UpdateFactionSettlements(WorldUtility.GetRimWarDataForFaction(this.Faction));
            FindParentSettlement();
            this.DestinationTarget = Find.World.worldObjects.WorldObjectAt(this.ParentSettlement.Tile, WorldObjectDefOf.Settlement);
        }

        public void FindHostileSettlement()
        {
            this.DestinationTarget = Find.World.worldObjects.WorldObjectOfDefAt(WorldObjectDefOf.Settlement, WorldUtility.GetHostileRimWarSettlementsInRange(this.Tile, 25, this.Faction, WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.Faction)).RandomElement().Tile);
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

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            //using (IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(caravan).GetEnumerator())
            //{
            //    if (enumerator.MoveNext())
            //    {
            //        FloatMenuOption o = enumerator.Current;
            //        yield return o;
            //    }
            //}
            using (IEnumerator<FloatMenuOption> enumerator2 = CaravanArrivalAction_AttackWarObject.GetFloatMenuOptions(caravan, this).GetEnumerator())
            {
                if (enumerator2.MoveNext())
                {
                    FloatMenuOption f2 = enumerator2.Current;
                    yield return f2;
                }
            }
            using (IEnumerator<FloatMenuOption> enumerator3 = CaravanArrivalAction_EngageWarObject.GetFloatMenuOptions(caravan, this).GetEnumerator())
            {
                if (enumerator3.MoveNext())
                {
                    FloatMenuOption f3 = enumerator3.Current;
                    yield return f3;
                }
            }
            yield break;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Prefs.DevMode)
            {
                List<Gizmo> gizmoIE = base.GetGizmos().ToList();
                Command_Action command_Action1 = new Command_Action();
                command_Action1.defaultLabel = "Dev: Destroy";
                command_Action1.defaultDesc = "Destroys the Rim War object.";
                command_Action1.action = delegate
                {
                    Destroy();
                };
                gizmoIE.Add(command_Action1);
                return gizmoIE;
            }
            return base.GetGizmos();
        }
    }
}
