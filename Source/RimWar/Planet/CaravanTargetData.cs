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
    public class CaravanTargetData : IExposable
    {
        public Caravan caravan = null;
        public WarObject caravanTarget = null;

        //public bool shouldRegenerateCaravanTarget = false;
        //public WarObject rwo = null;
        //public int rwoPts = 0;
        //public int rwoTile = 0;
        //public int dstTile = 0;
        //public WorldObject dest = null;
        //public RimWorld.Planet.Settlement parent = null;
        //public Faction fac = null;
        //public RimWarData rwd = null;
        
        public int CaravanTile
        {
            get
            {
                if (caravan != null)
                {
                    return caravan.Tile;
                }
                return 0;
            }
        }

        public int CaravanTargetTile
        {
            get
            {
                if(caravanTarget != null)
                {
                    return caravanTarget.Tile;
                }
                return 0;
            }
        }

        public int CaravanDestination
        {
            get
            {
                if(caravan.pather != null)
                {
                    return caravan.pather.Destination;
                }
                return 0;
            }
        }

        public int TargetDestination
        {
            get
            {
                if(caravanTarget.pather != null)
                {
                    return caravanTarget.pather.Destination;
                }
                return 0;
            }
        }

        public bool IsValid()
        {
            //if(shouldRegenerateCaravanTarget)
            //{
            //    WorldUtility.CreateWarObjectOfType(caravanTarget, caravanTarget.RimWarPoints, caravanTarget.rimwarData, caravanTarget.ParentSettlement, caravanTarget.Tile, caravanTarget.DestinationTarget, null, caravanTarget.DestinationTile, false, false);
            //}
            if(caravan == null || caravanTarget == null)
            {
                return false;
            }
            if(!Find.WorldObjects.Caravans.Contains(caravan))
            {
                return false;
            }
            if(!Find.WorldObjects.Contains(caravanTarget))
            {
                caravan.pather.StopDead();
                return false;
            }

            return true;
        }


        public void ExposeData()
        {
            Scribe_References.Look<Caravan>(ref this.caravan, "caravan");
            Scribe_References.Look<WarObject>(ref this.caravanTarget, "caravanTarget");
        }
    }
}
