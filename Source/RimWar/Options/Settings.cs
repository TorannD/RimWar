using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWar.Options
{
    public class Settings : Verse.ModSettings
    {
        //mod behavior
        public bool randomizeFactionBehavior = false;
        public bool storytellerBasedDifficulty = true;
        public float rimwarDifficulty = 1f;
        public bool createDiplomats = false;
        public bool useRimWarVictory = true;
        public bool restrictEvents = true;
        public bool randomizeAttributes = true;

        //limit controls
        public int maxFactionSettlements = 20;
        public float maxSettlementScanRange = 30f;
        public float settlementScanRangeDivider = 50f;        
        public float objectMovementMultiplier = 1f;

        //performance controls
        public int averageEventFrequency = 50;        
        public int settlementEventDelay = 60000;
        public int settlementScanDelay = 120000;               
        public int woEventFrequency = 200;
        public int rwdUpdateFrequency = 2500;
        public bool forceRandomObject = false;        

        //alerts
        public int alertRange = 6;
        public int letterNotificationRange = 7;

        //unused
        public int maxScanObjects = 100; //potentially an option to limit the iterations a search function performs before returning the result
        public int maxFactionObjects = 100; //potentially an option to limit the total number of objects a faction has - debug statistics show this is never reach for normal games

        //unsaved
        public bool playerVS = false;
        public float planetCoverageCustom = .12f;
        public bool randomizeFactionRelations = false;        

        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.randomizeFactionBehavior, "randomizeFactionBehavior", false, false);
            Scribe_Values.Look<bool>(ref this.storytellerBasedDifficulty, "storytellerBasedDifficulty", true, false);
            Scribe_Values.Look<bool>(ref this.createDiplomats, "createDiplomats", false, false);
            Scribe_Values.Look<bool>(ref this.restrictEvents, "restrictEvents", true, false);
            Scribe_Values.Look<bool>(ref this.useRimWarVictory, "useRimWarVictory", true, true);
            Scribe_Values.Look<float>(ref this.rimwarDifficulty, "rimwarDifficulty", 1f, false);
            Scribe_Values.Look<int>(ref this.maxFactionSettlements, "maxFactionSettlements", 20, false);
            Scribe_Values.Look<float>(ref this.maxSettlementScanRange, "maxSettlementScanRange", 30f, false);
            Scribe_Values.Look<float>(ref this.settlementScanRangeDivider, "settlementScanRangeDivider", 50f, false);
            Scribe_Values.Look<int>(ref this.maxFactionObjects, "maxFactionObjects", 30, false);
            Scribe_Values.Look<bool>(ref this.forceRandomObject, "forceRandomObject", false, false);
            Scribe_Values.Look<int>(ref this.maxScanObjects, "maxScanObjects", 100, false);
            Scribe_Values.Look<int>(ref this.averageEventFrequency, "averageEventFrequency", 50, false);
            Scribe_Values.Look<int>(ref this.settlementEventDelay, "settlementEventDelay", 60000, false);
            Scribe_Values.Look<int>(ref this.settlementScanDelay, "settlementScanDelay", 120000, false);
            Scribe_Values.Look<int>(ref this.woEventFrequency, "woEventFrequency", 200, false);
            Scribe_Values.Look<float>(ref this.objectMovementMultiplier, "objectMovementMultiplier", 1f, false);
            Scribe_Values.Look<int>(ref this.rwdUpdateFrequency, "rwdUpdateFrequency", 2500, false);
            Scribe_Values.Look<int>(ref this.alertRange, "alertRange", 6, false);
            Scribe_Values.Look<int>(ref this.letterNotificationRange, "letterNotificationRange", 7, false);
            Scribe_Values.Look<bool>(ref this.randomizeAttributes, "randomizeAttributes", true);
        }

        public static Settings Instance;

        public Settings()
        {
            Settings.Instance = this;
        }
    }
}
