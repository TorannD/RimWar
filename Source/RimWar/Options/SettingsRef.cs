using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWar.Options
{
    public class SettingsRef
    {
        public bool randomizeFactionBehavior = Settings.Instance.randomizeFactionBehavior;
        public bool storytellerBasedDifficulty = Settings.Instance.storytellerBasedDifficulty;
        public float rimwarDifficulty = Settings.Instance.rimwarDifficulty;
        public bool createDiplomats = Settings.Instance.createDiplomats;
        public bool useRimWarVictory = Settings.Instance.useRimWarVictory;
        public bool restrictEvents = Settings.Instance.restrictEvents;
        public bool randomizeAttributes = Settings.Instance.randomizeAttributes;

        public int maxFactionSettlements = Settings.Instance.maxFactionSettlements;
        public float settlementScanRangeDivider = Settings.Instance.settlementScanRangeDivider;
        public float objectMovementMultiplier = Settings.Instance.objectMovementMultiplier;

        public int averageEventFrequency = Settings.Instance.averageEventFrequency;
        public int settlementEventDelay = Settings.Instance.settlementEventDelay;
        public int settlementScanDelay = Settings.Instance.settlementScanDelay;
        public float maxSettelementScanRange = Settings.Instance.maxSettlementScanRange;
        public int woEventFrequency = Settings.Instance.woEventFrequency;
        public int rwdUpdateFrequency = Settings.Instance.rwdUpdateFrequency;
        public bool forceRandomObject = Settings.Instance.forceRandomObject;

        public int alertRange = Settings.Instance.alertRange;
        public int letterNotificationRange = Settings.Instance.letterNotificationRange;

        public bool playerVS = Settings.Instance.playerVS;
        public float planetCoverageCustom = Settings.Instance.planetCoverageCustom;
        public bool randomizeFactionRelations = Settings.Instance.randomizeFactionRelations; 
        
    }
}
