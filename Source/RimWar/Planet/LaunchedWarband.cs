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
    public class LaunchedWarband : LaunchedWarObject
    {
        private int lastEventTick = 0;
        private bool movesAtNight = false;
        private int ticksPerMove = 3300;
        private int searchTick = 60;               

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.movesAtNight, "movesAtNight", false, false);
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 3300, false);                       
        }

        public override void Tick()
        {
            base.Tick();
        }

        public LaunchedWarband()
        {

        }

        public override int RimWarPoints { get => base.RimWarPoints; set => base.RimWarPoints = value; }        

        public override void ArrivalAction()
        {
            //Log.Message("beginning arrival actions");
            WorldObjectDef dtDef = null;
            if(this.DestinationTarget == null)
            {
                dtDef = WorldObjectDefOf.Settlement;
            }
            else
            {
                dtDef = this.DestinationTarget.def;
            }
            WorldUtility.CreateWarband(this.RimWarPoints, this.rimwarData, this.ParentSettlement, this.destinationTile, this.DestinationTarget, dtDef, true);
            //Log.Message("ending arrival actions");
            base.ArrivalAction();
        }      
    }
}
