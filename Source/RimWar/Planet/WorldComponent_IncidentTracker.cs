using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimWar;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace RimWar.Planet
{
    public class WorldComponent_IncidentTracker : WorldComponent
    {
        //Historic Variables
        public int worldEncounters = 0;
        //Faction victory percent

        //Saved


        //Temp Stored

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public WorldComponent_IncidentTracker(World world) : base(world)
        {
            //Log.Message("world component power tracker init");
            //return;
        }

    }
}
