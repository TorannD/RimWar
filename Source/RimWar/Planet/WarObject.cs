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
    public class WarObject : WorldObject, ILoadReferenceable
    {
        private int uniqueId = -1;
        private string nameInt;

        public WarObject_PathFollower pather;
        public WarObject_GotoMoteRenderer gotoMote;
        public WarObject_Tweener tweener;

        private Material cachedMat;

        private bool cachedImmobilized;
        private int cachedImmobilizedForTicks = -99999;
        private const int ImmobilizedCacheDuration = 60;

        private static readonly Color WarObjectDefaultColor = new Color(1f, 1f, 1f);

        public int warPoints;

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

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            gotoMote.RenderMote();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
            Scribe_Values.Look(ref nameInt, "name");
            Scribe_Values.Look<int>(ref this.warPoints, "warPoints", 0, false);
            Scribe_Deep.Look(ref pather, "pather", this);
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

        public bool NightResting
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

        public int TicksPerMove => 10;  //CaravanTicksPerMoveUtility.GetTicksPerMove(this);

        public string TicksPerMoveExplanation
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append( "explination = warobject type and faction type"); //CaravanTicksPerMoveUtility.GetTicksPerMove(this, stringBuilder);
                return stringBuilder.ToString();
            }
        }

        public float Visibility => 0; //CaravanVisibilityCalculator.Visibility(this);

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
            //CheckAnyNonWorldPawns();
            pather.PatherTick();
            tweener.TweenerTick();
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
                float num6 = (float)Utility.ArrivalTimeEstimator.EstimatedTicksToArrive(base.Tile, pather.Destination, this) / 60000f;
                stringBuilder.AppendLine();
                stringBuilder.Append("RW_EstimatedTimeToDestination".Translate(num6.ToString("0.#")));
            }            
            if (!pather.MovingNow)
            {

            }
            return stringBuilder.ToString();
        }
    }
}
