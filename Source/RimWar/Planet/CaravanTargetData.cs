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
