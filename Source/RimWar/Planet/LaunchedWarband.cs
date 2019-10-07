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
     

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append(base.GetInspectString());            

            if (Find.World.worldObjects.AnySettlementAt(this.destinationTile))
            {
                WorldObject wo = Find.World.worldObjects.ObjectsAt(this.destinationTile).FirstOrDefault();
                if (wo != null)
                {
                    if (wo.Faction != this.Faction)
                    {
                        stringBuilder.Append("RW_WarbandInspectString".Translate(this.Name, "RW_Attacking".Translate(), wo.Label));
                    }
                    else
                    {
                        stringBuilder.Append("RW_WarbandInspectString".Translate(this.Name, "RW_ReturningTo".Translate(), wo.Label));
                    }
                }
            }

            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append("RW_CombatPower".Translate(this.RimWarPoints));

            return stringBuilder.ToString();
        }

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
            WorldUtility.CreateWarband(this.RimWarPoints, this.rimwarData, this.ParentSettlement, this.destinationTile, this.destinationTile, dtDef, true);
            //Log.Message("ending arrival actions");
            base.ArrivalAction();
        }      
    }
}
