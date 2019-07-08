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
    [StaticConstructorOnStartup]
    public class Warband_Caravan : Caravan
    {
        private int warbandPowerint;

        public Faction WarBandFaction => base.Faction;

        public IncidentParms parms;

        public Warband_Caravan()
        {
            pawns = new ThingOwner<Pawn>(this, false, LookMode.Reference);
            pather = new Caravan_PathFollower(this);
            gotoMote = new Caravan_GotoMoteRenderer();
            tweener = new Caravan_Tweener(this);
            trader = new Caravan_TraderTracker(this);
            forage = new Caravan_ForageTracker(this);
            needs = new Caravan_NeedsTracker(this);
            carryTracker = new Caravan_CarryTracker(this);
            beds = new Caravan_BedsTracker(this);
            storyState = new StoryState(this);
        }
    }
}
